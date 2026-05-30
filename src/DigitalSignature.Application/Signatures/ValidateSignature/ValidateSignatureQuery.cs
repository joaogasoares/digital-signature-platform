using DigitalSignature.Application.Common.Models;
using MediatR;

namespace DigitalSignature.Application.Signatures.ValidateSignature;

public sealed record ValidateSignatureQuery(Guid DocumentId) : IRequest<Result<ValidationResult>>;

public sealed record ValidationResult(
    bool IsValid,
    string DocumentId,
    string? SignedBy,
    string? Algorithm,
    string? CertificateThumbprint,
    DateTimeOffset? SignedAt,
    string? FailureReason);
