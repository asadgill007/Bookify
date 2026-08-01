using Bookify.Application.Commands.Admin;
using Bookify.Application.Commands.Businesses;
using Bookify.Application.Commands.Providers;
using Bookify.Application.DTOs.Businesses;
using Bookify.Application.Queries.Businesses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
public class BusinessesController : ApiController
{
    private readonly IMediator _mediator;

    public BusinessesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search businesses with filtering, sorting, and pagination.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] BusinessSearchRequest request, CancellationToken cancellationToken)
    {
        var query = new SearchBusinessesQuery
        {
            Search = request.Search,
            Category = request.Category,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            RadiusKm = request.RadiusKm,
            RatingMin = request.RatingMin,
            PriceMin = request.PriceMin,
            PriceMax = request.PriceMax,
            IsVerified = request.IsVerified,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
            return HandleResult(result);

        var pagination = new PaginationInfo
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = result.Data!.TotalCount,
            TotalPages = result.Data.TotalPages
        };

        return ApiOk(result.Data.Items, pagination);
    }

    /// <summary>
    /// Get business details by slug.
    /// Pending/rejected businesses are only visible to their owner (or admins).
    /// </summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            try { userId = GetUserId(); } catch { /* anonymous */ }
        }

        var query = new GetBusinessBySlugQuery
        {
            Slug = slug,
            UserId = userId,
            IsAdmin = User.IsInRole("Admin")
        };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get businesses owned by the current user (provider dashboard).
    /// </summary>
    [HttpGet("mine")]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var query = new GetMyBusinessesQuery { UserId = GetUserId() };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new business listing.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBusinessRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateBusinessCommand
        {
            UserId = GetUserId(),
            Name = request.Name,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            TimeZone = request.TimeZone,
            Currency = request.Currency,
            Description = request.Description,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Website = request.Website,
            CancellationPolicy = request.CancellationPolicy,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CoverImageUrl = request.CoverImageUrl,
            CategoryIds = request.CategoryIds ?? new List<Guid>()
        };

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return HandleResult(result);

        return ApiCreated(new { result.Data!.Id, result.Data.Slug }, "Business created successfully.");
    }

    /// <summary>
    /// Update business details (name, address, categories, booking type, etc.).
    /// </summary>
    [HttpPut("{businessId}")]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> Update(Guid businessId, [FromBody] UpdateBusinessRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateBusinessCommand
        {
            UserId = GetUserId(),
            BusinessId = businessId,
            Name = request.Name,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            TimeZone = request.TimeZone,
            Currency = request.Currency,
            Description = request.Description,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Website = request.Website,
            CancellationPolicy = request.CancellationPolicy,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            BookingType = request.BookingType,
            CategoryIds = request.CategoryIds ?? new List<Guid>()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Set weekly opening hours for a business.
    /// </summary>
    [HttpPut("{businessId}/hours")]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> SetHours(Guid businessId, [FromBody] SetBusinessHoursRequest request, CancellationToken cancellationToken)
    {
        var command = new SetBusinessHoursCommand
        {
            BusinessId = businessId,
            UserId = GetUserId(),
            Hours = (request.Hours ?? new List<BusinessDayHoursRequest>())
                .Select(h => new BusinessDayHours
                {
                    DayOfWeek = h.DayOfWeek,
                    OpenTime = h.OpenTime,
                    CloseTime = h.CloseTime,
                    IsClosed = h.IsClosed
                })
                .ToList()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Add gallery images (by URL) to a business. First image becomes cover.
    /// </summary>
    [HttpPost("{businessId}/images")]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> AddImages(Guid businessId, [FromBody] AddBusinessImagesRequest request, CancellationToken cancellationToken)
    {
        var command = new AddBusinessImagesCommand
        {
            BusinessId = businessId,
            UserId = GetUserId(),
            ImageUrls = request.ImageUrls ?? new List<string>()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? ApiOk(message: "Images added successfully.")
            : HandleResult(result);
    }

    /// <summary>
    /// Resubmit a rejected business for review (business owner only).
    /// </summary>
    [HttpPost("{businessId}/resubmit")]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> Resubmit(Guid businessId, CancellationToken cancellationToken)
    {
        var command = new ResubmitBusinessCommand
        {
            UserId = GetUserId(),
            BusinessId = businessId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Add a staff/provider profile under a business.
    /// </summary>
    [HttpPost("{businessId}/providers")]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> AddProvider(Guid businessId, [FromBody] AddBusinessProviderRequest request, CancellationToken cancellationToken)
    {
        var command = new AddBusinessProviderCommand
        {
            BusinessId = businessId,
            OwnerUserId = GetUserId(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Title = request.Title,
            Bio = request.Bio,
            AvatarUrl = request.AvatarUrl,
            DisplayOrder = request.DisplayOrder,
            ServiceIds = request.ServiceIds ?? new List<Guid>()
        };

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return HandleResult(result);

        return ApiCreated(new { result.Data!.ProviderId, result.Data.UserId }, "Provider added successfully.");
    }
}

public class AddBusinessImagesRequest
{
    public List<string>? ImageUrls { get; set; }
}

public class SetBusinessHoursRequest
{
    public List<BusinessDayHoursRequest>? Hours { get; set; }
}

public class BusinessDayHoursRequest
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public bool IsClosed { get; set; }
}

public class AddBusinessProviderRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public int DisplayOrder { get; set; }
    public List<Guid>? ServiceIds { get; set; }
}
