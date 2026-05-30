using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Application.Users.RegisterUser;
using FluentAssertions;
using NSubstitute;

namespace DigitalSignature.Application.UnitTests.Users;

public class RegisterUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private RegisterUserHandler CreateHandler() =>
        new(_userRepository, _passwordHasher, _unitOfWork);

    [Fact]
    public async Task Handle_valid_command_returns_success_with_user_id()
    {
        _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(false);
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed_password");

        var command = new RegisterUserCommand("user@example.com", "StrongP4ss");
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_duplicate_email_returns_failure()
    {
        _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(true);

        var command = new RegisterUserCommand("existing@example.com", "StrongP4ss");
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already registered");
    }

    [Theory]
    [InlineData("notanemail", "StrongP4ss")]
    [InlineData("user@example.com", "weak")]
    public async Task Handle_invalid_input_returns_failure(string email, string password)
    {
        var command = new RegisterUserCommand(email, password);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_success_never_stores_plain_password()
    {
        _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(false);
        _passwordHasher.Hash("StrongP4ss").Returns("bcrypt_hash_value");

        var command = new RegisterUserCommand("user@example.com", "StrongP4ss");
        await CreateHandler().Handle(command, CancellationToken.None);

        await _userRepository.Received(1).AddAsync(
            Arg.Is<Domain.Entities.User>(u => u.PasswordHash == "bcrypt_hash_value"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_success_calls_save_changes()
    {
        _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(false);
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hash");

        var command = new RegisterUserCommand("user@example.com", "StrongP4ss");
        await CreateHandler().Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
