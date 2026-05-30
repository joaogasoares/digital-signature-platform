using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Application.Users.LoginUser;
using DigitalSignature.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DigitalSignature.Application.UnitTests.Users;

public class LoginUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();

    private LoginUserHandler CreateHandler() =>
        new(_userRepository, _passwordHasher, _tokenGenerator);

    private static User CreateTestUser() =>
        User.Create("user@example.com", "hashed_password");

    [Fact]
    public async Task Handle_valid_credentials_returns_token()
    {
        var user = CreateTestUser();
        _userRepository.GetByEmailAsync("user@example.com").Returns(user);
        _passwordHasher.Verify("StrongP4ss", "hashed_password").Returns(true);
        _tokenGenerator.GenerateToken(user).Returns("jwt.token.value");

        var result = await CreateHandler().Handle(
            new LoginUserCommand("user@example.com", "StrongP4ss"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("jwt.token.value");
    }

    [Fact]
    public async Task Handle_unknown_email_returns_failure()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        var result = await CreateHandler().Handle(
            new LoginUserCommand("unknown@example.com", "StrongP4ss"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Handle_wrong_password_returns_failure()
    {
        var user = CreateTestUser();
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(user);
        _passwordHasher.Verify("WrongPass", "hashed_password").Returns(false);

        var result = await CreateHandler().Handle(
            new LoginUserCommand("user@example.com", "WrongPass"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Handle_wrong_password_never_reveals_reason()
    {
        var user = CreateTestUser();
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var unknownEmail = await CreateHandler().Handle(
            new LoginUserCommand("no@example.com", "pass"), CancellationToken.None);

        var wrongPass = await CreateHandler().Handle(
            new LoginUserCommand("user@example.com", "wrong"), CancellationToken.None);

        unknownEmail.Error.Should().Be(wrongPass.Error,
            "error message must be identical to prevent user enumeration");
    }
}
