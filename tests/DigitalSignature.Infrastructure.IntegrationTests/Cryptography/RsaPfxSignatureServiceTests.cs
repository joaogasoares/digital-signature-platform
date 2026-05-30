using DigitalSignature.Infrastructure.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace DigitalSignature.Infrastructure.IntegrationTests.Cryptography;

public class RsaPfxSignatureServiceTests
{
    private static RsaPfxSignatureService CreateService()
    {
        var pfxPath = Path.Combine(
            Path.GetDirectoryName(typeof(RsaPfxSignatureServiceTests).Assembly.Location)!,
            "..", "..", "..", "..", "certs", "test-signing.pfx");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Pfx:Path"] = Path.GetFullPath(pfxPath),
                ["Pfx:Password"] = "testpassword123"
            })
            .Build();

        return new RsaPfxSignatureService(config);
    }

    [Fact]
    public void Sign_then_verify_returns_true()
    {
        using var svc = CreateService();
        var data = "document hash bytes"u8.ToArray();

        var signature = svc.Sign(data);
        var isValid = svc.Verify(data, signature);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void Verify_tampered_data_returns_false()
    {
        using var svc = CreateService();
        var data = "original hash"u8.ToArray();
        var signature = svc.Sign(data);

        var tampered = "tampered hash"u8.ToArray();
        var isValid = svc.Verify(tampered, signature);

        isValid.Should().BeFalse();
    }

    [Fact]
    public void Certificate_is_valid()
    {
        using var svc = CreateService();

        var result = svc.IsCertificateValid(out var error);

        result.Should().BeTrue(error);
    }

    [Fact]
    public void GetAlgorithm_returns_rsa_pss()
    {
        using var svc = CreateService();
        svc.GetAlgorithm().Should().Be("RSA-PSS-SHA256");
    }

    [Fact]
    public void GetThumbprint_returns_non_empty()
    {
        using var svc = CreateService();
        svc.GetThumbprint().Should().NotBeNullOrEmpty();
    }
}
