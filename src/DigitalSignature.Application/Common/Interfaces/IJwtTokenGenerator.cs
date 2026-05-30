using DigitalSignature.Domain.Entities;

namespace DigitalSignature.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
