namespace DigitalSignature.Application.Common.Interfaces;

public interface IEncryptionService
{
    byte[] Encrypt(byte[] plaintext);
    byte[] Decrypt(byte[] ciphertext);
}
