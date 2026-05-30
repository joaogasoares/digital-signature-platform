using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Application.Common.Models;
using MediatR;

namespace DigitalSignature.Application.Documents.GetDocument;

internal sealed class GetDocumentHandler(
    IDocumentRepository documentRepository,
    IFileStorage fileStorage,
    IEncryptionService encryptionService,
    IHashService hashService)
    : IRequestHandler<GetDocumentQuery, Result<DocumentResponse>>
{
    public async Task<Result<DocumentResponse>> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
    {
        var document = await documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);

        if (document is null)
            return Result<DocumentResponse>.Failure("Document not found.");

        if (document.OwnerId != request.RequesterId)
            return Result<DocumentResponse>.Failure("Access denied.");

        await using var encryptedStream = await fileStorage.ReadAsync(document.StoragePath, cancellationToken);
        using var ms = new MemoryStream();
        await encryptedStream.CopyToAsync(ms, cancellationToken);

        byte[] plainBytes;
        try
        {
            plainBytes = encryptionService.Decrypt(ms.ToArray());
        }
        catch
        {
            return Result<DocumentResponse>.Failure("Document integrity check failed: decryption error.");
        }

        // Verify integrity: re-compute hash and compare
        var actualHash = hashService.Compute(plainBytes);
        if (!document.Hash.Matches(actualHash.Value))
            return Result<DocumentResponse>.Failure("Document integrity check failed: hash mismatch.");

        return Result<DocumentResponse>.Success(new DocumentResponse(
            document.Id,
            document.FileName,
            document.ContentType,
            document.FileSizeBytes,
            document.Hash.Value,
            document.Status.ToString(),
            document.UploadedAt,
            plainBytes));
    }
}
