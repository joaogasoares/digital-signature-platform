using System.Security.Cryptography;
using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Domain.ValueObjects;

namespace DigitalSignature.Infrastructure.Cryptography;

internal sealed class Sha256HashService : IHashService
{
    public async Task<DocumentHash> ComputeAsync(Stream content, CancellationToken cancellationToken = default)
    {
        var hash = await SHA256.HashDataAsync(content, cancellationToken);
        return DocumentHash.FromSha256(Convert.ToHexString(hash).ToLowerInvariant());
    }

    public DocumentHash Compute(byte[] content)
    {
        var hash = SHA256.HashData(content);
        return DocumentHash.FromSha256(Convert.ToHexString(hash).ToLowerInvariant());
    }
}
