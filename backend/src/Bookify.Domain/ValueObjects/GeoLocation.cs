using Bookify.Domain.Common;

namespace Bookify.Domain.ValueObjects;

public sealed class GeoLocation : ValueObject
{
    public double Latitude { get; }
    public double Longitude { get; }

    private GeoLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static GeoLocation Create(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90.", nameof(latitude));

        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180.", nameof(longitude));

        return new GeoLocation(Math.Round(latitude, 6), Math.Round(longitude, 6));
    }

    public double DistanceTo(GeoLocation other)
    {
        var d1 = Latitude * (Math.PI / 180.0);
        var d2 = other.Latitude * (Math.PI / 180.0);
        var d3 = (Latitude - other.Latitude) * (Math.PI / 180.0);
        var d4 = (Longitude - other.Longitude) * (Math.PI / 180.0);

        var a = Math.Sin(d3 / 2) * Math.Sin(d3 / 2) +
                Math.Cos(d1) * Math.Cos(d2) *
                Math.Sin(d4 / 2) * Math.Sin(d4 / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return 6371 * c; // Distance in km
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString() => $"{Latitude}, {Longitude}";
}
