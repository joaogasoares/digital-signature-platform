using DigitalSignature.Domain.ValueObjects;
using FluentAssertions;

namespace DigitalSignature.Domain.UnitTests.ValueObjects;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("Abcdef1!")]
    [InlineData("StrongP4ssword")]
    public void Valid_password_does_not_throw(string password)
    {
        var act = () => PasswordPolicy.Validate(password);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null!)]
    public void Null_or_empty_throws(string? password)
    {
        var act = () => PasswordPolicy.Validate(password!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Too_short_throws()
    {
        var act = () => PasswordPolicy.Validate("Ab1");
        act.Should().Throw<ArgumentException>().WithMessage("*at least*");
    }

    [Fact]
    public void No_uppercase_throws()
    {
        var act = () => PasswordPolicy.Validate("abcdef1!");
        act.Should().Throw<ArgumentException>().WithMessage("*uppercase*");
    }

    [Fact]
    public void No_lowercase_throws()
    {
        var act = () => PasswordPolicy.Validate("ABCDEF1!");
        act.Should().Throw<ArgumentException>().WithMessage("*lowercase*");
    }

    [Fact]
    public void No_digit_throws()
    {
        var act = () => PasswordPolicy.Validate("Abcdefgh!");
        act.Should().Throw<ArgumentException>().WithMessage("*digit*");
    }
}
