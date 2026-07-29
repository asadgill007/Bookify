using Bookify.Domain.Common;

namespace Bookify.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty.", nameof(value));

        if (value.Length > 256)
            throw new ArgumentException("Email cannot exceed 256 characters.", nameof(value));

        if (!value.Contains('@') || !value.Contains('.'))
            throw new ArgumentException("Email is not in a valid format.", nameof(value));

        return new Email(value.Trim().ToLowerInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
