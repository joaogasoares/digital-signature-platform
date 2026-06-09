using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DigitalSignature.Application.Common.Interfaces;

namespace DigitalSignature.Infrastructure.Cryptography;

// Shared RSA-PSS signing/verification over an X.509 certificate. Subclasses
// only supply the certificate (loaded from a PFX or generated self-signed);
// all crypto operations live here so there is a single implementation.
internal abstract class CertificateSignatureService : ISignatureService, IDisposable
{
    private readonly RSA _privateKey;

    protected CertificateSignatureService(X509Certificate2 certificate)
    {
        Certificate = certificate;
        _privateKey = certificate.GetRSAPrivateKey()
            ?? throw new InvalidOperationException("Certificate does not contain an RSA private key.");
    }

    protected X509Certificate2 Certificate { get; }

    public byte[] Sign(byte[] data)
        => _privateKey.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

    public bool Verify(byte[] data, byte[] signature)
    {
        using var publicKey = Certificate.GetRSAPublicKey()
            ?? throw new InvalidOperationException("Certificate does not contain an RSA public key.");

        return publicKey.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
    }

    public string GetThumbprint() => Certificate.Thumbprint;

    public string GetAlgorithm() => "RSA-PSS-SHA256";

    public virtual bool IsCertificateValid(out string validationError)
    {
        validationError = string.Empty;
        var now = DateTime.UtcNow;

        if (now < Certificate.NotBefore.ToUniversalTime())
        {
            validationError = "Certificate is not yet valid.";
            return false;
        }

        if (now > Certificate.NotAfter.ToUniversalTime())
        {
            validationError = "Certificate has expired.";
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        _privateKey.Dispose();
        Certificate.Dispose();
    }
}
