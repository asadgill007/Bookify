using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Businesses;

public sealed class SearchBusinessesQuery : PagedQuery, IRequest<Result<PaginatedList<BusinessSearchResult>>>
{
    public string? Category { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? RadiusKm { get; init; }
    public double? RatingMin { get; init; }
    public decimal? PriceMin { get; init; }
    public decimal? PriceMax { get; init; }
    public bool? IsVerified { get; init; }
}

public class BusinessSearchResult
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

public sealed class SearchBusinessesQueryHandler : IRequestHandler<SearchBusinessesQuery, Result<PaginatedList<BusinessSearchResult>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SearchBusinessesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedList<BusinessSearchResult>>> Handle(SearchBusinessesQuery request, CancellationToken cancellationToken)
    {
        Guid? categoryId = null;
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var category = await _unitOfWork.Categories.GetBySlugAsync(request.Category, cancellationToken);
            categoryId = category?.Id;
        }

        // Public customer search shows only verified (approved) businesses by default.
        // Pending/rejected businesses stay hidden until an admin approves them.
        var effectiveVerified = request.IsVerified ?? true;

        var businesses = await _unitOfWork.Businesses.SearchAsync(
            request.Search,
            categoryId,
            request.Latitude,
            request.Longitude,
            request.RadiusKm,
            request.RatingMin,
            request.PriceMin,
            request.PriceMax,
            effectiveVerified,
            request.SortBy,
            request.SortDirection,
            request.Page,
            request.PageSize,
            cancellationToken);

        var totalCount = await _unitOfWork.Businesses.SearchCountAsync(
            request.Search, categoryId, request.Latitude, request.Longitude, request.RadiusKm,
            effectiveVerified, request.PriceMin, request.PriceMax, cancellationToken);

        var items = businesses.Select(b =>
        {
            double? distanceKm = null;
            if (request.Latitude.HasValue && request.Longitude.HasValue && b.Latitude.HasValue && b.Longitude.HasValue)
            {
                distanceKm = HaversineKm(request.Latitude.Value, request.Longitude.Value, b.Latitude.Value, b.Longitude.Value);
            }

            return new BusinessSearchResult
            {
                Id = b.Id,
                Name = b.Name,
                Slug = b.Slug,
                Description = b.Description,
                Category = b.BusinessCategories.FirstOrDefault()?.Category?.Name,
                AverageRating = b.AverageRating,
                TotalReviews = b.TotalReviews,
                CoverImageUrl = b.Images.FirstOrDefault(i => i.IsCover)?.Url ?? b.CoverImageUrl,
                City = b.City,
                Country = b.Country,
                DistanceKm = distanceKm,
                IsVerified = b.IsVerified,
                IsOpenNow = true // Simplified; real logic checks current availability
            };
        }).ToList();

        return Result<PaginatedList<BusinessSearchResult>>.Success(
            new PaginatedList<BusinessSearchResult>(items, request.Page, request.PageSize, totalCount));
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371.0;
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Math.Round(r * c, 1);
    }
}
