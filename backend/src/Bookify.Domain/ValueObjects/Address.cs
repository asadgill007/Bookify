using Bookify.Domain.Common;

namespace Bookify.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string Line1 { get; }
    public string? Line2 { get; }
    public string City { get; }
    public string? State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private Address(
        string line1,
        string? line2,
        string city,
        string? state,
        string postalCode,
        string country)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(
        string line1,
        string? line2,
        string city,
        string? state,
        string postalCode,
        string country)
    {
        if (string.IsNullOrWhiteSpace(line1))
            throw new ArgumentException("Address line 1 cannot be empty.", nameof(line1));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be empty.", nameof(postalCode));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));

        if (line1.Length > 200)
            throw new ArgumentException("Address line 1 cannot exceed 200 characters.", nameof(line1));
        if (city.Length > 100)
            throw new ArgumentException("City cannot exceed 100 characters.", nameof(city));
        if (postalCode.Length > 20)
            throw new ArgumentException("Postal code cannot exceed 20 characters.", nameof(postalCode));
        if (country.Length > 100)
            throw new ArgumentException("Country cannot exceed 100 characters.", nameof(country));

        return new Address(line1, line2?.Trim(), city.Trim(), state?.Trim(), postalCode.Trim(), country.Trim());
    }

    public override string ToString()
    {
        var parts = new List<string> { Line1 };
        if (!string.IsNullOrEmpty(Line2)) parts.Add(Line2);
        parts.Add(City);
        if (!string.IsNullOrEmpty(State)) parts.Add(State);
        parts.Add(PostalCode);
        parts.Add(Country);
        return string.Join(", ", parts);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }
}
