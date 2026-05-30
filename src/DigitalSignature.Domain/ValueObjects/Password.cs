namespace DigitalSignature.Domain.ValueObjects;

public static class PasswordPolicy
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    public static void Validate(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        if (password.Length < MinLength)
            throw new ArgumentException($"Password must be at least {MinLength} characters.", nameof(password));

        if (password.Length > MaxLength)
            throw new ArgumentException($"Password must not exceed {MaxLength} characters.", nameof(password));

        if (!password.Any(char.IsUpper))
            throw new ArgumentException("Password must contain at least one uppercase letter.", nameof(password));

        if (!password.Any(char.IsLower))
            throw new ArgumentException("Password must contain at least one lowercase letter.", nameof(password));

        if (!password.Any(char.IsDigit))
            throw new ArgumentException("Password must contain at least one digit.", nameof(password));
    }
}
