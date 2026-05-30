namespace DigitalSignature.Application.Common.Interfaces;

public interface ISignatureService
{
    byte[] Sign(byte[] data);
    bool Verify(byte[] data, byte[] signature);
    string GetThumbprint();
    string GetAlgorithm();
    bool IsCertificateValid(out string validationError);
}
