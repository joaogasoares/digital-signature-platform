using DigitalSignature.Domain.ValueObjects;
using FluentAssertions;

namespace DigitalSignature.Domain.UnitTests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("user+tag@sub.domain.com")]
    public void Create_valid_email_succeeds(string raw)
    {
        var email = Email.Create(raw);
        email.Value.Should().Be(raw.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null!)]
    public void Create_null_or_empty_throws(string? raw)
    {
        var act = () => Email.Create(raw!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@tld")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@email.com")]
    public void Create_invalid_format_throws(string raw)
    {
        var act = () => Email.Create(raw);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_email_exceeding_320_chars_throws()
    {
        var raw = new string('a', 315) + "@b.com";
        var act = () => Email.Create(raw);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Two_emails_with_same_value_are_equal()
    {
        var a = Email.Create("test@example.com");
        var b = Email.Create("TEST@EXAMPLE.COM");
        a.Should().Be(b);
    }
}
