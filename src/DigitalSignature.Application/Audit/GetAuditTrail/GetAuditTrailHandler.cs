using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Application.Common.Models;
using MediatR;

namespace DigitalSignature.Application.Audit.GetAuditTrail;

internal sealed class GetAuditTrailHandler(IAuditRepository auditRepository)
    : IRequestHandler<GetAuditTrailQuery, Result<IReadOnlyList<AuditEventDto>>>
{
    public async Task<Result<IReadOnlyList<AuditEventDto>>> Handle(
        GetAuditTrailQuery request, CancellationToken cancellationToken)
    {
        var events = request.DocumentId.HasValue
            ? await auditRepository.GetByDocumentIdAsync(request.DocumentId.Value, cancellationToken)
            : request.UserId.HasValue
                ? await auditRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken)
                : [];

        var dtos = events
            .Select(e => new AuditEventDto(e.Id, e.Action, e.UserId, e.DocumentId, e.Details, e.OccurredAt))
            .ToList();

        return Result<IReadOnlyList<AuditEventDto>>.Success(dtos);
    }
}
