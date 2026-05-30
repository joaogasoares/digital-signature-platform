namespace DigitalSignature.Domain.Entities;

public sealed class Signature
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public Guid SignerId { get; private set; }
    public byte[] SignatureBytes { get; private set; }
    public string Algorithm { get; private set; }
    public string CertificateThumbprint { get; private set; }
    public DateTimeOffset SignedAt { get; private set; }

    private Signature()
    {
        SignatureBytes = [];
        Algorithm = string.Empty;
        CertificateThumbprint = string.Empty;
    }

    public static Signature Create(
        Guid documentId,
        Guid signerId,
        byte[] signatureBytes,
        string algorithm,
        string certificateThumbprint,
        DateTimeOffset signedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(algorithm);
        ArgumentException.ThrowIfNullOrWhiteSpace(certificateThumbprint);
        if (signatureBytes.Length == 0)
            throw new ArgumentException("Signature bytes cannot be empty.", nameof(signatureBytes));

        return new Signature
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            SignerId = signerId,
            SignatureBytes = signatureBytes,
            Algorithm = algorithm,
            CertificateThumbprint = certificateThumbprint,
            SignedAt = signedAt
        };
    }
}
