using Bookify.Domain.Common;
using Bookify.Domain.DomainEvents;
using Bookify.Domain.Enums;
using Bookify.Domain.ValueObjects;

namespace Bookify.Domain.Entities;

public sealed class Business : BaseEntity
{
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string AddressLine1 { get; private set; } = null!;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = null!;
    public string? State { get; private set; }
    public string PostalCode { get; private set; } = null!;
    public string Country { get; private set; } = null!;
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public string? Website { get; private set; }
    public bool IsVerified { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public Guid? ReviewedBy { get; private set; }
    public bool IsActive { get; private set; }
    public BookingType BookingType { get; private set; }
    public string? CancellationPolicy { get; private set; }
    public string TimeZone { get; private set; } = null!;
    public string Currency { get; private set; } = null!;
    public double AverageRating { get; private set; }
    public int TotalReviews { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public string? LogoUrl { get; private set; }

    public User Owner { get; private set; } = null!;
    public ICollection<Provider> Providers { get; private set; } = new List<Provider>();
    public ICollection<Service> Services { get; private set; } = new List<Service>();
    public ICollection<Review> Reviews { get; private set; } = new List<Review>();
    public ICollection<BusinessImage> Images { get; private set; } = new List<BusinessImage>();
    public ICollection<BusinessCategory> BusinessCategories { get; private set; } = new List<BusinessCategory>();
    public ICollection<BusinessHours> BusinessHours { get; private set; } = new List<BusinessHours>();
    public ICollection<FavoriteBusiness> Favorites { get; private set; } = new List<FavoriteBusiness>();

    private Business() { }

    public Business(
        Guid ownerId,
        string name,
        string slug,
        string addressLine1,
        string city,
        string postalCode,
        string country,
        string timeZone,
        string currency = "USD")
    {
        OwnerId = ownerId;
        SetName(name, slug);
        AddressLine1 = addressLine1;
        City = city;
        PostalCode = postalCode;
        Country = country;
        TimeZone = timeZone;
        Currency = currency;
        IsActive = true;
        BookingType = BookingType.Instant;
        VerificationStatus = VerificationStatus.Pending;
    }

    public void SetName(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Business name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Business slug cannot be empty.", nameof(slug));

        Name = name.Trim();
        Slug = slug.ToLowerInvariant().Trim();
        Touch();
    }

    public void UpdateDetails(
        string? description,
        string? email,
        string? phoneNumber,
        string? website,
        string? cancellationPolicy)
    {
        Description = description?.Trim();
        Email = email?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        Website = website?.Trim();
        CancellationPolicy = cancellationPolicy?.Trim();
        Touch();
    }

    public void UpdateAddress(
        string addressLine1,
        string? addressLine2,
        string city,
        string? state,
        string postalCode,
        string country)
    {
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2?.Trim();
        City = city;
        State = state?.Trim();
        PostalCode = postalCode;
        Country = country;
        Touch();
    }

    public void SetGeoLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        Touch();
    }

    public void SetImages(string? coverImageUrl, string? logoUrl)
    {
        CoverImageUrl = coverImageUrl;
        LogoUrl = logoUrl;
        Touch();
    }

    public void SetBookingType(BookingType type)
    {
        BookingType = type;
        Touch();
    }

    public void Verify(Guid? adminUserId = null)
    {
        IsVerified = true;
        VerificationStatus = VerificationStatus.Approved;
        RejectionReason = null;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = adminUserId;
        AddDomainEvent(new BusinessVerifiedEvent(Id, OwnerId, DateTime.UtcNow));
        Touch();
    }

    public void Reject(string? reason, Guid? adminUserId = null)
    {
        IsVerified = false;
        VerificationStatus = VerificationStatus.Rejected;
        RejectionReason = reason?.Trim();
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = adminUserId;
        Touch();
    }

    public void ResubmitForReview()
    {
        if (VerificationStatus != VerificationStatus.Rejected)
            throw new InvalidOperationException("Only rejected businesses can be resubmitted for review.");

        VerificationStatus = VerificationStatus.Pending;
        RejectionReason = null;
        ReviewedAt = null;
        ReviewedBy = null;
        Touch();
    }

    public void ToggleActive(bool active)
    {
        IsActive = active;
        Touch();
    }

    public void UpdateRating(double newAverageRating, int totalReviews)
    {
        AverageRating = newAverageRating;
        TotalReviews = totalReviews;
        Touch();
    }

    public Address GetAddress()
    {
        return Address.Create(AddressLine1, AddressLine2, City, State, PostalCode, Country);
    }

    public GeoLocation? GetGeoLocation()
    {
        if (Latitude.HasValue && Longitude.HasValue)
            return GeoLocation.Create(Latitude.Value, Longitude.Value);
        return null;
    }
}
