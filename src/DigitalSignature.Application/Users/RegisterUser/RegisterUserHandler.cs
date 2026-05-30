using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Application.Common.Models;
using DigitalSignature.Domain.Entities;
using DigitalSignature.Domain.ValueObjects;
using MediatR;

namespace DigitalSignature.Application.Users.RegisterUser;

internal sealed class RegisterUserHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        Email email;
        try
        {
            email = Email.Create(request.Email);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }

        try
        {
            PasswordPolicy.Validate(request.Password);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }

        if (await userRepository.ExistsByEmailAsync(email.Value, cancellationToken))
            return Result<Guid>.Failure($"Email '{email.Value}' is already registered.");

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(email.Value, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}
