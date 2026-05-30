using DigitalSignature.Application.Common.Models;
using MediatR;

namespace DigitalSignature.Application.Audit.GetAuditTrail;

public sealed record GetAuditTrailQuery(Guid? DocumentId, Guid? UserId) : IRequest<Result<IReadOnlyList<AuditEventDto>>>;

public sealed record AuditEventDto(
    Guid Id,
    string Action,
    Guid? UserId,
    Guid? DocumentId,
    string? Details,
    DateTimeOffset OccurredAt);
