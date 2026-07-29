using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Categories;

/// <summary>
/// Query to retrieve all active categories with their subcategories.
/// </summary>
public sealed record GetCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryDto>>>;

/// <summary>
/// DTO for a category with its subcategories.
/// </summary>
public sealed class CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? IconName { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyList<SubCategoryDto> SubCategories { get; init; } = Array.Empty<SubCategoryDto>();
}

/// <summary>
/// DTO for a subcategory.
/// </summary>
public sealed class SubCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
}

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Categories.GetActiveWithSubCategoriesAsync(cancellationToken);

        var items = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            IconName = c.IconName,
            DisplayOrder = c.DisplayOrder,
            SubCategories = c.SubCategories.Select(sc => new SubCategoryDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Slug = sc.Slug
            }).ToList()
        }).ToList();

        return Result<IReadOnlyList<CategoryDto>>.Success(items);
    }
}
