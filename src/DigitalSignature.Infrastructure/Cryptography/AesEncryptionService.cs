using System.Security.Cryptography;
using DigitalSignature.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DigitalSignature.Infrastructure.Cryptography;

// Envelope format: [12-byte nonce][16-byte tag][ciphertext]
internal sealed class AesEncryptionService : IEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _key;

    public AesEncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Aes:MasterKey"]
            ?? throw new InvalidOperationException("Aes:MasterKey is not configured.");

        _key = Convert.FromBase64String(keyBase64);

        if (_key.Length != 32)
            throw new InvalidOperationException("Aes:MasterKey must be 32 bytes (256 bits) base64-encoded.");
    }

    public byte[] Encrypt(byte[] plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var tag = new byte[TagSize];
        var ciphertext = new byte[plaintext.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        // Envelope: nonce + tag + ciphertext
        var result = new byte[NonceSize + TagSize + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(ciphertext, 0, result, NonceSize + TagSize, ciphertext.Length);
        return result;
    }

    public byte[] Decrypt(byte[] envelope)
    {
        if (envelope.Length < NonceSize + TagSize)
            throw new ArgumentException("Envelope too short.", nameof(envelope));

        var nonce = envelope[..NonceSize];
        var tag = envelope[NonceSize..(NonceSize + TagSize)];
        var ciphertext = envelope[(NonceSize + TagSize)..];
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return plaintext;
    }
}
