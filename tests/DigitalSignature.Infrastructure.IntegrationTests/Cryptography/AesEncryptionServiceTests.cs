using DigitalSignature.Infrastructure.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace DigitalSignature.Infrastructure.IntegrationTests.Cryptography;

public class AesEncryptionServiceTests
{
    private static AesEncryptionService CreateService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Aes:MasterKey"] = Convert.ToBase64String(new byte[32]) // 256-bit zero key for tests
            })
            .Build();
        return new AesEncryptionService(config);
    }

    [Fact]
    public void Encrypt_then_decrypt_returns_original()
    {
        var svc = CreateService();
        var plaintext = "Hello, Digital Signature Platform!"u8.ToArray();

        var ciphertext = svc.Encrypt(plaintext);
        var decrypted = svc.Decrypt(ciphertext);

        decrypted.Should().Equal(plaintext);
    }

    [Fact]
    public void Encrypted_bytes_differ_from_plaintext()
    {
        var svc = CreateService();
        var plaintext = "sensitive document content"u8.ToArray();

        var ciphertext = svc.Encrypt(plaintext);

        ciphertext.Should().NotEqual(plaintext);
    }

    [Fact]
    public void Two_encryptions_of_same_input_produce_different_ciphertext()
    {
        var svc = CreateService();
        var plaintext = "same input"u8.ToArray();

        var ct1 = svc.Encrypt(plaintext);
        var ct2 = svc.Encrypt(plaintext);

        ct1.Should().NotEqual(ct2, "each encryption must use a unique nonce");
    }

    [Fact]
    public void Tampered_ciphertext_throws_on_decrypt()
    {
        var svc = CreateService();
        var ciphertext = svc.Encrypt("original"u8.ToArray());

        ciphertext[^1] ^= 0xFF; // flip last byte (tag region)

        var act = () => svc.Decrypt(ciphertext);
        act.Should().Throw<Exception>("AES-GCM authentication must fail on tampered data");
    }
}
