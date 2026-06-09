using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace DigitalSignature.Infrastructure.Cryptography;

// Loads a PFX certificate from config and signs document hashes with RSA-PSS.
// PFX path and password come from configuration (never hardcoded).
internal sealed class RsaPfxSignatureService : CertificateSignatureService
{
    public RsaPfxSignatureService(IConfiguration configuration)
        : base(LoadCertificate(configuration))
    {
    }

    private static X509Certificate2 LoadCertificate(IConfiguration configuration)
    {
        var pfxPath = configuration["Pfx:Path"]
            ?? throw new InvalidOperationException("Pfx:Path is not configured.");
        var pfxPassword = configuration["Pfx:Password"]
            ?? throw new InvalidOperationException("Pfx:Password is not configured.");

        var pfxBytes = File.ReadAllBytes(pfxPath);
        return X509CertificateLoader.LoadPkcs12(
            pfxBytes,
            pfxPassword,
            X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
    }

    public override bool IsCertificateValid(out string validationError)
    {
        if (!base.IsCertificateValid(out validationError))
            return false;

        // Verify key usage allows digital signature
        foreach (var ext in Certificate.Extensions)
        {
            if (ext is X509KeyUsageExtension ku
                && !ku.KeyUsages.HasFlag(X509KeyUsageFlags.DigitalSignature))
            {
                validationError = "Certificate key usage does not include DigitalSignature.";
                return false;
            }
        }

        return true;
    }
}
