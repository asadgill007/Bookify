using Bookify.Application.Common;
using Bookify.Application.DTOs.Businesses;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Businesses;

public sealed record GetBusinessBySlugQuery : IRequest<Result<BusinessDetailDto>>
{
    public string Slug { get; init; } = string.Empty;
}

public sealed class GetBusinessBySlugQueryHandler : IRequestHandler<GetBusinessBySlugQuery, Result<BusinessDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBusinessBySlugQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BusinessDetailDto>> Handle(GetBusinessBySlugQuery request, CancellationToken cancellationToken)
    {
        var business = await _unitOfWork.Businesses.GetBySlugAsync(request.Slug, cancellationToken);
        if (business == null)
            return Result<BusinessDetailDto>.Failure("Business not found.", "NOT_FOUND");

        var dto = new BusinessDetailDto
        {
            Id = business.Id,
            Name = business.Name,
            Slug = business.Slug,
            Description = business.Description,
            Email = business.Email,
            PhoneNumber = business.PhoneNumber,
            Address = new AddressDto
            {
                Line1 = business.AddressLine1,
                Line2 = business.AddressLine2,
                City = business.City,
                State = business.State,
                PostalCode = business.PostalCode,
                Country = business.Country
            },
            GeoLocation = business.Latitude.HasValue && business.Longitude.HasValue
                ? new GeoLocationDto { Latitude = business.Latitude.Value, Longitude = business.Longitude.Value }
                : null,
            Website = business.Website,
            IsVerified = business.IsVerified,
            BookingType = business.BookingType.ToString(),
            CancellationPolicy = business.CancellationPolicy,
            TimeZone = business.TimeZone,
            Currency = business.Currency,
            AverageRating = business.AverageRating,
            TotalReviews = business.TotalReviews,
            Categories = business.BusinessCategories.Select(bc => bc.Category?.Name ?? string.Empty).ToList(),
            CoverImageUrl = business.CoverImageUrl,
            LogoUrl = business.LogoUrl,
            Gallery = business.Images.Select(i => new BusinessImageDto
            {
                Url = i.Url,
                AltText = i.AltText,
                IsCover = i.IsCover
            }).ToList(),
            Providers = business.Providers.Select(p => new ProviderSummaryDto
            {
                Id = p.Id,
                FirstName = p.User?.FirstName ?? "",
                LastName = p.User?.LastName ?? "",
                Title = p.Title,
                AvatarUrl = p.User?.AvatarUrl
            }).ToList(),
            Services = business.Services.Where(s => s.IsActive).Select(s => new ServiceSummaryDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                DurationMinutes = s.DurationMinutes,
                Price = s.PriceAmount,
                Currency = s.PriceCurrency,
                Category = s.Category
            }).ToList()
        };

        return Result<BusinessDetailDto>.Success(dto);
    }
}
