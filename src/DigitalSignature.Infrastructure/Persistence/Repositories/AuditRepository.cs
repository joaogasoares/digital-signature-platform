using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignature.Infrastructure.Persistence.Repositories;

internal sealed class AuditRepository(AppDbContext context) : IAuditRepository
{
    public async Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
        => await context.AuditEvents.AddAsync(auditEvent, cancellationToken);

    public async Task<IReadOnlyList<AuditEvent>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
        => await context.AuditEvents
            .Where(e => e.DocumentId == documentId)
            .OrderByDescending(e => e.OccurredAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AuditEvent>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await context.AuditEvents
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.OccurredAt)
            .ToListAsync(cancellationToken);
}
