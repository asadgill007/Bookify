using Bookify.Application.Commands.Businesses;
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
    /// </summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var query = new GetBusinessBySlugQuery { Slug = slug };
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
            City = request.City,
            PostalCode = request.PostalCode,
            Country = request.Country,
            TimeZone = request.TimeZone,
            Currency = request.Currency,
            Description = request.Description,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Website = request.Website,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return HandleResult(result);

        return ApiCreated(new { result.Data!.Id, result.Data.Slug }, "Business created successfully.");
    }
}
