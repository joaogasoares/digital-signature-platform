using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DigitalSignature.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DigitalSignature.Infrastructure.Cryptography;

// Loads a PFX certificate from config and signs document hashes with RSA-PSS.
// PFX path and password come from configuration (never hardcoded).
internal sealed class RsaPfxSignatureService : ISignatureService, IDisposable
{
    private readonly X509Certificate2 _certificate;
    private readonly RSA _privateKey;

    public RsaPfxSignatureService(IConfiguration configuration)
    {
        var pfxPath = configuration["Pfx:Path"]
            ?? throw new InvalidOperationException("Pfx:Path is not configured.");
        var pfxPassword = configuration["Pfx:Password"]
            ?? throw new InvalidOperationException("Pfx:Password is not configured.");

        var pfxBytes = File.ReadAllBytes(pfxPath);
        _certificate = X509CertificateLoader.LoadPkcs12(
            pfxBytes,
            pfxPassword,
            X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);

        _privateKey = _certificate.GetRSAPrivateKey()
            ?? throw new InvalidOperationException("Certificate does not contain an RSA private key.");
    }

    public byte[] Sign(byte[] data)
        => _privateKey.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

    public bool Verify(byte[] data, byte[] signature)
    {
        using var publicKey = _certificate.GetRSAPublicKey()
            ?? throw new InvalidOperationException("Certificate does not contain an RSA public key.");

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

        // Verify key usage allows digital signature
        foreach (var ext in _certificate.Extensions)
        {
            if (ext is X509KeyUsageExtension ku)
            {
                if (!ku.KeyUsages.HasFlag(X509KeyUsageFlags.DigitalSignature))
                {
                    validationError = "Certificate key usage does not include DigitalSignature.";
                    return false;
                }
            }
        }

        return true;
    }

    public void Dispose()
    {
        _privateKey.Dispose();
        _certificate.Dispose();
    }
}
