using DigitalSignature.Domain.ValueObjects;
using FluentAssertions;

namespace DigitalSignature.Domain.UnitTests.ValueObjects;

public class DocumentHashTests
{
    private const string ValidSha256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

    [Fact]
    public void FromSha256_valid_hex_creates_hash()
    {
        var hash = DocumentHash.FromSha256(ValidSha256);
        hash.Value.Should().Be(ValidSha256);
        hash.Algorithm.Should().Be("SHA-256");
    }

    [Fact]
    public void FromSha256_normalizes_to_lowercase()
    {
        var hash = DocumentHash.FromSha256(ValidSha256.ToUpper());
        hash.Value.Should().Be(ValidSha256);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null!)]
    public void FromSha256_null_or_empty_throws(string? value)
    {
        var act = () => DocumentHash.FromSha256(value!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromSha256_wrong_length_throws()
    {
        var act = () => DocumentHash.FromSha256("abc123");
        act.Should().Throw<ArgumentException>().WithMessage("*64 hex*");
    }

    [Fact]
    public void FromSha256_non_hex_throws()
    {
        var act = () => DocumentHash.FromSha256(new string('z', 64));
        act.Should().Throw<ArgumentException>().WithMessage("*hex*");
    }

    [Fact]
    public void Matches_same_hash_returns_true()
    {
        var hash = DocumentHash.FromSha256(ValidSha256);
        hash.Matches(ValidSha256).Should().BeTrue();
    }

    [Fact]
    public void Matches_different_hash_returns_false()
    {
        var hash = DocumentHash.FromSha256(ValidSha256);
        var other = new string('a', 64);
        hash.Matches(other).Should().BeFalse();
    }

    [Fact]
    public void Two_hashes_with_same_value_are_equal()
    {
        var a = DocumentHash.FromSha256(ValidSha256);
        var b = DocumentHash.FromSha256(ValidSha256);
        a.Should().Be(b);
    }
}
