using DigitalSignature.Domain.Entities;

namespace DigitalSignature.Application.Common.Interfaces;

public interface IAuditRepository
{
    Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEvent>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEvent>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
