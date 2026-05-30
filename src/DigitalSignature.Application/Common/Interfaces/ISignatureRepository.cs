using DigitalSignature.Domain.Entities;

namespace DigitalSignature.Application.Common.Interfaces;

public interface ISignatureRepository
{
    Task<Signature?> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task AddAsync(Signature signature, CancellationToken cancellationToken = default);
}
