using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Favorites;

/// <summary>
/// List the current user's favorite businesses with search-card summaries.
/// </summary>
public sealed record GetFavoritesQuery : IRequest<Result<IReadOnlyList<FavoriteBusinessDto>>>
{
    public Guid UserId { get; init; }
}

public sealed class FavoriteBusinessDto
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
    public bool IsVerified { get; set; }
    public DateTime FavoritedAt { get; set; }
}

/// <summary>Ids of all businesses favorited by the user (for heart state).</summary>
public sealed record GetFavoriteIdsQuery : IRequest<Result<IReadOnlyList<Guid>>>
{
    public Guid UserId { get; init; }
}

public sealed class GetFavoritesQueryHandler : IRequestHandler<GetFavoritesQuery, Result<IReadOnlyList<FavoriteBusinessDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFavoritesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<IReadOnlyList<FavoriteBusinessDto>>> Handle(GetFavoritesQuery request, CancellationToken cancellationToken)
    {
        var favorites = await _unitOfWork.Favorites.GetByUserAsync(request.UserId, cancellationToken);

        var dtos = favorites.Select(f => new FavoriteBusinessDto
        {
            Id = f.Business.Id,
            Name = f.Business.Name,
            Slug = f.Business.Slug,
            Description = f.Business.Description,
            Category = f.Business.BusinessCategories.FirstOrDefault()?.Category?.Name,
            AverageRating = f.Business.AverageRating,
            TotalReviews = f.Business.TotalReviews,
            CoverImageUrl = f.Business.Images.FirstOrDefault(i => i.IsCover)?.Url ?? f.Business.CoverImageUrl,
            City = f.Business.City,
            Country = f.Business.Country,
            IsVerified = f.Business.IsVerified,
            FavoritedAt = f.CreatedAt
        }).ToList();

        return Result<IReadOnlyList<FavoriteBusinessDto>>.Success(dtos);
    }
}

public sealed class GetFavoriteIdsQueryHandler : IRequestHandler<GetFavoriteIdsQuery, Result<IReadOnlyList<Guid>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFavoriteIdsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<IReadOnlyList<Guid>>> Handle(GetFavoriteIdsQuery request, CancellationToken cancellationToken)
    {
        var ids = await _unitOfWork.Favorites.GetBusinessIdsAsync(request.UserId, cancellationToken);
        return Result<IReadOnlyList<Guid>>.Success(ids);
    }
}
