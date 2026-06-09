using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DigitalSignature.Application.Common.Interfaces;

namespace DigitalSignature.Infrastructure.Cryptography;

// Fallback signature service used when no PFX certificate is configured.
// Generates an ephemeral self-signed RSA certificate at startup so the
// signing pipeline works in development, tests, and demos without a real
// certificate on disk. Production should configure Pfx:Path/Pfx:Password to
// use RsaPfxSignatureService instead.
internal sealed class SelfSignedSignatureService : ISignatureService, IDisposable
{
    private readonly X509Certificate2 _certificate;
    private readonly RSA _privateKey;

    public SelfSignedSignatureService()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=DigitalSignature Self-Signed",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pss);

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

        var now = DateTimeOffset.UtcNow;
        _certificate = request.CreateSelfSigned(now.AddDays(-1), now.AddYears(1));

        _privateKey = _certificate.GetRSAPrivateKey()
            ?? throw new InvalidOperationException("Self-signed certificate has no RSA private key.");
    }

    public byte[] Sign(byte[] data)
        => _privateKey.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

    public bool Verify(byte[] data, byte[] signature)
    {
        using var publicKey = _certificate.GetRSAPublicKey()
            ?? throw new InvalidOperationException("Self-signed certificate has no RSA public key.");

        return publicKey.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
    }

    public string GetThumbprint() => _certificate.Thumbprint;

    public string GetAlgorithm() => "RSA-PSS-SHA256";

    public bool IsCertificateValid(out string validationError)
    {
        validationError = string.Empty;
        var now = DateTime.UtcNow;

        if (now < _certificate.NotBefore.ToUniversalTime())
        {
            validationError = "Certificate is not yet valid.";
            return false;
        }

        if (now > _certificate.NotAfter.ToUniversalTime())
        {
            validationError = "Certificate has expired.";
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        _privateKey.Dispose();
        _certificate.Dispose();
    }
}
