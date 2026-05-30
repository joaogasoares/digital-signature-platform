using DigitalSignature.Application.Common.Models;
using MediatR;

namespace DigitalSignature.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(string Email, string Password) : IRequest<Result<Guid>>;
