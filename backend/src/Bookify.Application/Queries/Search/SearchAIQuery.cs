using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Search;

public sealed record SearchAIQuery : IRequest<Result<SearchAIResult>>
{
    public string Query { get; init; } = string.Empty;
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
}

public sealed class SearchAIResult
{
    public object? AiInterpretation { get; init; }
    public object? Results { get; init; }
}

public sealed class SearchAIQueryHandler : IRequestHandler<SearchAIQuery, Result<SearchAIResult>>
{
    private readonly IAISearchService _aiSearchService;
    private readonly IUnitOfWork _unitOfWork;

    public SearchAIQueryHandler(IAISearchService aiSearchService, IUnitOfWork unitOfWork)
    {
        _aiSearchService = aiSearchService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SearchAIResult>> Handle(SearchAIQuery request, CancellationToken cancellationToken)
    {
        var searchRequest = new AISearchRequest
        {
            Query = request.Query,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        var interpretation = await _aiSearchService.InterpretQueryAsync(searchRequest, cancellationToken);
        if (interpretation.IsFailure)
            return Result<SearchAIResult>.Failure(interpretation.Error!, interpretation.ErrorCode);

        // AI search only surfaces approved businesses, same as customer search.
        var businesses = await _unitOfWork.Businesses.SearchAsync(
            request.Query,
            null,
            request.Latitude,
            request.Longitude,
            10,
            null,
            null,
            null,
            true,
            "rating",
            "desc",
            1,
            20,
            cancellationToken);

        return Result<SearchAIResult>.Success(new SearchAIResult
        {
            AiInterpretation = interpretation.Data,
            Results = businesses.Select(b => new
            {
                b.Id,
                b.Name,
                b.Slug,
                b.Description,
                b.AverageRating,
                b.TotalReviews,
                b.City,
                b.Country,
                b.CoverImageUrl,
                b.IsVerified,
                b.Latitude,
                b.Longitude
            })
        });
    }
}
