using DigitalSignature.Domain.ValueObjects;

namespace DigitalSignature.Application.Common.Interfaces;

public interface IHashService
{
    Task<DocumentHash> ComputeAsync(Stream content, CancellationToken cancellationToken = default);
    DocumentHash Compute(byte[] content);
}
