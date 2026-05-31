using System.Text.RegularExpressions;

namespace DigitalSignature.Domain.ValueObjects;

public sealed record Email
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var trimmed = value.Trim().ToLowerInvariant();

        if (trimmed.Length > 320)
            throw new ArgumentException("Email must not exceed 320 characters.", nameof(value));

        if (!EmailRegex.IsMatch(trimmed))
            throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));

        return new Email(trimmed);
    }

    public override string ToString() => Value;
}
