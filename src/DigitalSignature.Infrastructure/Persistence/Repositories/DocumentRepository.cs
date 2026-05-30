using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignature.Infrastructure.Persistence.Repositories;

internal sealed class DocumentRepository(AppDbContext context) : IDocumentRepository
{
    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Documents.SingleOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Document>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
        => await context.Documents
            .Where(d => d.OwnerId == ownerId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
        => await context.Documents.AddAsync(document, cancellationToken);
}
