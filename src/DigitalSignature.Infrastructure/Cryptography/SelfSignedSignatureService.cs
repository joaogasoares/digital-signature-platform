using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace DigitalSignature.Infrastructure.Cryptography;

// Fallback signature service used when no PFX certificate is configured.
// Generates an ephemeral self-signed RSA certificate at startup so the
// signing pipeline works in development, tests, and demos without a real
// certificate on disk. Production should configure Pfx:Path/Pfx:Password to
// use RsaPfxSignatureService instead.
internal sealed class SelfSignedSignatureService : CertificateSignatureService
{
    public SelfSignedSignatureService()
        : base(CreateCertificate())
    {
    }

    private static X509Certificate2 CreateCertificate()
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
        return request.CreateSelfSigned(now.AddDays(-1), now.AddYears(1));
    }
}
