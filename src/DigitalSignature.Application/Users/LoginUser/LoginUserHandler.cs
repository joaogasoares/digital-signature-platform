using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Application.Common.Models;
using MediatR;

namespace DigitalSignature.Application.Users.LoginUser;

internal sealed class LoginUserHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator)
    : IRequestHandler<LoginUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(
            request.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<string>.Failure("Invalid credentials.");

        var token = tokenGenerator.GenerateToken(user);
        return Result<string>.Success(token);
    }
}
