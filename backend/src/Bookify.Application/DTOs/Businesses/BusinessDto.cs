using FluentValidation;

namespace Bookify.Application.DTOs.Businesses;

public class BusinessDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public string? CoverImageUrl { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double? DistanceKm { get; set; }
    public bool IsVerified { get; set; }
    public bool IsOpenNow { get; set; }
}

public class BusinessDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public AddressDto Address { get; set; } = new();
    public GeoLocationDto? GeoLocation { get; set; }
    public string? Website { get; set; }
    public bool IsVerified { get; set; }
    public string VerificationStatus { get; set; } = "Pending";
    public string? RejectionReason { get; set; }
    public string BookingType { get; set; } = "Instant";
    public string? CancellationPolicy { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string Currency { get; set; } = "USD";
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<string> Categories { get; set; } = new();
    public string? CoverImageUrl { get; set; }
    public string? LogoUrl { get; set; }
    public List<BusinessImageDto> Gallery { get; set; } = new();
    public List<ProviderSummaryDto> Providers { get; set; } = new();
    public List<ServiceSummaryDto> Services { get; set; } = new();
    public List<BusinessHoursDto> OpeningHours { get; set; } = new();
}

public class BusinessHoursDto
{
    public string DayOfWeek { get; set; } = string.Empty;
    public string OpenTime { get; set; } = "09:00";
    public string CloseTime { get; set; } = "17:00";
    public bool IsClosed { get; set; }
}

public class AddressDto
{
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class GeoLocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class BusinessImageDto
{
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsCover { get; set; }
}

public class ProviderSummaryDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? AvatarUrl { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
}

public class ServiceSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Category { get; set; }
}

public class BusinessSearchRequest
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public double? RatingMin { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public bool? IsVerified { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "desc";
}

public class CreateBusinessRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Website { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string Currency { get; set; } = "USD";
    public string? CancellationPolicy { get; set; }
    public string? CoverImageUrl { get; set; }
    public List<Guid>? CategoryIds { get; set; }
}

public class CreateBusinessRequestValidator : AbstractValidator<CreateBusinessRequest>
{
    public CreateBusinessRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TimeZone).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}

public class UpdateBusinessRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Website { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string Currency { get; set; } = "USD";
    public string? CancellationPolicy { get; set; }
    public string? BookingType { get; set; }
    public List<Guid>? CategoryIds { get; set; }
}

public class UpdateBusinessRequestValidator : AbstractValidator<UpdateBusinessRequest>
{
    public UpdateBusinessRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TimeZone).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
