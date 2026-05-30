using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Application.Common.Models;
using MediatR;

namespace DigitalSignature.Application.Signatures.ValidateSignature;

internal sealed class ValidateSignatureHandler(
    IDocumentRepository documentRepository,
    ISignatureRepository signatureRepository,
    ISignatureService signatureService)
    : IRequestHandler<ValidateSignatureQuery, Result<ValidationResult>>
{
    public async Task<Result<ValidationResult>> Handle(ValidateSignatureQuery request, CancellationToken cancellationToken)
    {
        var document = await documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return Result<ValidationResult>.Failure("Document not found.");

        var signature = await signatureRepository.GetByDocumentIdAsync(request.DocumentId, cancellationToken);
        if (signature is null)
        {
            return Result<ValidationResult>.Success(new ValidationResult(
                false, request.DocumentId.ToString(), null, null, null, null,
                "Document has not been signed."));
        }

        var hashBytes = Convert.FromHexString(document.Hash.Value);
        var isValid = signatureService.Verify(hashBytes, signature.SignatureBytes);

        return Result<ValidationResult>.Success(new ValidationResult(
            isValid,
            request.DocumentId.ToString(),
            signature.SignerId.ToString(),
            signature.Algorithm,
            signature.CertificateThumbprint,
            signature.SignedAt,
            isValid ? null : "Signature verification failed. Document may have been tampered."));
    }
}
