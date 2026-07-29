using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Admin;

public sealed record GetAdminBusinessesQuery : IRequest<Result<PaginatedList<AdminBusinessDto>>>
{
    public bool? Verified { get; init; }
    public bool? Active { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetAdminBusinessesQueryHandler : IRequestHandler<GetAdminBusinessesQuery, Result<PaginatedList<AdminBusinessDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminBusinessesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<AdminBusinessDto>>> Handle(GetAdminBusinessesQuery request, CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.Businesses.GetFilteredAsync(
            request.Verified, request.Active, request.Page, request.PageSize, cancellationToken);
        var total = await _unitOfWork.Businesses.GetFilteredCountAsync(
            request.Verified, request.Active, cancellationToken);

        var dtos = items.Select(b => new AdminBusinessDto
        {
            Id = b.Id,
            Name = b.Name,
            Slug = b.Slug,
            Email = b.Email,
            PhoneNumber = b.PhoneNumber,
            City = b.City,
            Country = b.Country,
            IsVerified = b.IsVerified,
            IsActive = b.IsActive,
            AverageRating = b.AverageRating,
            TotalReviews = b.TotalReviews,
            CreatedAt = b.CreatedAt,
            IsDeleted = b.IsDeleted
        }).ToList();

        return Result<PaginatedList<AdminBusinessDto>>.Success(
            new PaginatedList<AdminBusinessDto>(dtos, request.Page, request.PageSize, total));
    }
}

public class AdminBusinessDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
