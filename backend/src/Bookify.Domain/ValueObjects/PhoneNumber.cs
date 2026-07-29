using Bookify.Domain.Common;

namespace Bookify.Domain.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty.", nameof(value));

        if (value.Length > 20)
            throw new ArgumentException("Phone number cannot exceed 20 characters.", nameof(value));

        var cleaned = new string(value.Where(c => char.IsDigit(c) || c == '+' || c == '-').ToArray());

        if (cleaned.Length < 7)
            throw new ArgumentException("Phone number is too short.", nameof(value));

        return new PhoneNumber(cleaned);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
