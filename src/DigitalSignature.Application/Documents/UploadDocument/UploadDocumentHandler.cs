using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Application.Common.Models;
using DigitalSignature.Domain.Entities;
using MediatR;

namespace DigitalSignature.Application.Documents.UploadDocument;

internal sealed class UploadDocumentHandler(
    IDocumentRepository documentRepository,
    IFileStorage fileStorage,
    IHashService hashService,
    IEncryptionService encryptionService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UploadDocumentCommand, Result<Guid>>
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public async Task<Result<Guid>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        if (!AllowedContentTypes.Contains(request.ContentType))
            return Result<Guid>.Failure($"Content type '{request.ContentType}' is not allowed.");

        if (request.FileSizeBytes > MaxFileSizeBytes)
            return Result<Guid>.Failure($"File exceeds maximum size of {MaxFileSizeBytes / 1024 / 1024} MB.");

        if (request.FileSizeBytes <= 0)
            return Result<Guid>.Failure("File size must be positive.");

        // Read file bytes — needed for hash + encryption
        using var ms = new MemoryStream();
        await request.FileContent.CopyToAsync(ms, cancellationToken);
        var plainBytes = ms.ToArray();

        // Compute hash before encryption (hash of original content)
        var hash = hashService.Compute(plainBytes);

        // Encrypt content before persisting
        var encryptedBytes = encryptionService.Encrypt(plainBytes);

        // Store encrypted content
        using var encryptedStream = new MemoryStream(encryptedBytes);
        var storagePath = await fileStorage.SaveAsync(encryptedStream, request.FileName, cancellationToken);

        var document = Document.Create(
            request.OwnerId,
            request.FileName,
            request.ContentType,
            request.FileSizeBytes,
            storagePath,
            hash);

        await documentRepository.AddAsync(document, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(document.Id);
    }
}
