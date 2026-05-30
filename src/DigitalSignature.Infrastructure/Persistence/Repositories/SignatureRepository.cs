using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignature.Infrastructure.Persistence.Repositories;

internal sealed class SignatureRepository(AppDbContext context) : ISignatureRepository
{
    public async Task<Signature?> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
        => await context.Signatures.SingleOrDefaultAsync(s => s.DocumentId == documentId, cancellationToken);

    public async Task AddAsync(Signature signature, CancellationToken cancellationToken = default)
        => await context.Signatures.AddAsync(signature, cancellationToken);
}
