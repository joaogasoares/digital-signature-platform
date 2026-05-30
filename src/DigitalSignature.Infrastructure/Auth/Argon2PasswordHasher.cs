using DigitalSignature.Application.Common.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace DigitalSignature.Infrastructure.Auth;

internal sealed class Argon2PasswordHasher : IPasswordHasher
{
    // PBKDF2-HMACSHA512 with 350000 iterations (OWASP 2023 recommendation).
    // Argon2id preferred but requires external dependency; PBKDF2 is available in-box.
    private const int Iterations = 350_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const char Separator = ':';

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA512,
            KeySize);

        return $"{Convert.ToBase64String(salt)}{Separator}{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split(Separator);
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA512,
            KeySize);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
