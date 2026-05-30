using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Application.Common.Models;
using DigitalSignature.Domain.Entities;
using MediatR;

namespace DigitalSignature.Application.Signatures.SignDocument;

internal sealed class SignDocumentHandler(
    IDocumentRepository documentRepository,
    ISignatureRepository signatureRepository,
    IAuditRepository auditRepository,
    ISignatureService signatureService,
    IUnitOfWork unitOfWork,
    IClock clock)
    : IRequestHandler<SignDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SignDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return Result<Guid>.Failure("Document not found.");

        if (document.OwnerId != request.SignerId)
            return Result<Guid>.Failure("Access denied.");

        var existing = await signatureRepository.GetByDocumentIdAsync(request.DocumentId, cancellationToken);
        if (existing is not null)
            return Result<Guid>.Failure("Document is already signed.");

        if (!signatureService.IsCertificateValid(out var certError))
            return Result<Guid>.Failure($"Certificate is not valid: {certError}");

        var hashBytes = Convert.FromHexString(document.Hash.Value);
        var signatureBytes = signatureService.Sign(hashBytes);

        var signature = Signature.Create(
            document.Id,
            request.SignerId,
            signatureBytes,
            signatureService.GetAlgorithm(),
            signatureService.GetThumbprint(),
            clock.UtcNow);

        document.MarkAsSigned();

        await signatureRepository.AddAsync(signature, cancellationToken);

        await auditRepository.AddAsync(
            AuditEvent.Create("DocumentSigned", request.SignerId, document.Id,
                $"Algorithm={signatureService.GetAlgorithm()}", clock.UtcNow),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(signature.Id);
    }
}
