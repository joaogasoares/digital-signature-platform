namespace DigitalSignature.Domain.ValueObjects;

public sealed record DocumentHash
{
    public string Value { get; }
    public string Algorithm { get; }

    private DocumentHash(string value, string algorithm)
    {
        Value = value;
        Algorithm = algorithm;
    }

    public static DocumentHash FromSha256(string hexValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hexValue);

        if (hexValue.Length != 64)
            throw new ArgumentException("SHA-256 hash must be 64 hex characters.", nameof(hexValue));

        if (!hexValue.All(c => Uri.IsHexDigit(c)))
            throw new ArgumentException("Hash must contain only hex characters.", nameof(hexValue));

        return new DocumentHash(hexValue.ToLowerInvariant(), "SHA-256");
    }

    public bool Matches(string otherHex) =>
        string.Equals(Value, otherHex?.ToLowerInvariant(), StringComparison.Ordinal);

    public override string ToString() => $"{Algorithm}:{Value}";
}
