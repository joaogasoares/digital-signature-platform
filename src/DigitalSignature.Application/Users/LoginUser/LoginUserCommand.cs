using DigitalSignature.Application.Common.Models;
using MediatR;

namespace DigitalSignature.Application.Users.LoginUser;

public sealed record LoginUserCommand(string Email, string Password) : IRequest<Result<string>>;
