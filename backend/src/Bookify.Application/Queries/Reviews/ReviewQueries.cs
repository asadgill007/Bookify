using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using MediatR;

namespace Bookify.Application.Queries.Reviews;

// ─── Get Business Reviews ───────────────────────────────
public sealed record GetBusinessReviewsQuery : IRequest<Result<PaginatedList<ReviewDetailDto>>>
{
    public Guid BusinessId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetBusinessReviewsQueryHandler : IRequestHandler<GetBusinessReviewsQuery, Result<PaginatedList<ReviewDetailDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBusinessReviewsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<ReviewDetailDto>>> Handle(GetBusinessReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _unitOfWork.Reviews.GetByBusinessIdAsync(request.BusinessId, request.Page, request.PageSize, cancellationToken);
        var total = await _unitOfWork.Reviews.GetBusinessReviewCountAsync(request.BusinessId, cancellationToken);

        var items = reviews.Select(MapToDto).ToList();
        return Result<PaginatedList<ReviewDetailDto>>.Success(new PaginatedList<ReviewDetailDto>(items, request.Page, request.PageSize, total));
    }

    private static ReviewDetailDto MapToDto(Domain.Entities.Review r)
    {
        return new ReviewDetailDto
        {
            Id = r.Id,
            CustomerName = r.Customer != null ? $"{r.Customer.FirstName} {r.Customer.LastName}" : "",
            CustomerAvatarUrl = r.Customer?.AvatarUrl,
            Rating = r.Rating,
            Comment = r.Comment,
            IsVerifiedPurchase = r.IsVerifiedPurchase,
            ProviderReply = r.ProviderReply,
            RepliedAt = r.RepliedAt,
            ProviderName = r.Provider?.User != null ? $"{r.Provider.User.FirstName} {r.Provider.User.LastName}" : null,
            CreatedAt = r.CreatedAt
        };
    }
}

// ─── Get Provider Reviews ───────────────────────────────
public sealed record GetProviderReviewsQuery : IRequest<Result<PaginatedList<ReviewDetailDto>>>
{
    public Guid ProviderId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetProviderReviewsQueryHandler : IRequestHandler<GetProviderReviewsQuery, Result<PaginatedList<ReviewDetailDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProviderReviewsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<ReviewDetailDto>>> Handle(GetProviderReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _unitOfWork.Reviews.GetByProviderIdAsync(request.ProviderId, request.Page, request.PageSize, cancellationToken);
        var total = await _unitOfWork.Reviews.GetProviderReviewCountAsync(request.ProviderId, cancellationToken);

        var items = reviews.Select(r => new ReviewDetailDto
        {
            Id = r.Id,
            CustomerName = r.Customer != null ? $"{r.Customer.FirstName} {r.Customer.LastName}" : "",
            CustomerAvatarUrl = r.Customer?.AvatarUrl,
            Rating = r.Rating,
            Comment = r.Comment,
            IsVerifiedPurchase = r.IsVerifiedPurchase,
            ProviderReply = r.ProviderReply,
            RepliedAt = r.RepliedAt,
            BusinessName = r.Business?.Name,
            CreatedAt = r.CreatedAt
        }).ToList();

        return Result<PaginatedList<ReviewDetailDto>>.Success(new PaginatedList<ReviewDetailDto>(items, request.Page, request.PageSize, total));
    }
}

// ─── Get Review Statistics ──────────────────────────────
public sealed record GetReviewStatisticsQuery : IRequest<Result<ReviewStatisticsResult>>
{
    public Guid BusinessId { get; init; }
}

public sealed class GetReviewStatisticsQueryHandler : IRequestHandler<GetReviewStatisticsQuery, Result<ReviewStatisticsResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReviewStatisticsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<ReviewStatisticsResult>> Handle(GetReviewStatisticsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _unitOfWork.Reviews.GetStatisticsAsync(request.BusinessId, cancellationToken);
        return Result<ReviewStatisticsResult>.Success(stats);
    }
}

// ─── Get Top Rated Providers ────────────────────────────
public sealed record GetTopRatedProvidersQuery : IRequest<Result<IReadOnlyList<TopRatedProviderDto>>>
{
    public int Count { get; init; } = 10;
}

public sealed class GetTopRatedProvidersQueryHandler : IRequestHandler<GetTopRatedProvidersQuery, Result<IReadOnlyList<TopRatedProviderDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTopRatedProvidersQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<IReadOnlyList<TopRatedProviderDto>>> Handle(GetTopRatedProvidersQuery request, CancellationToken cancellationToken)
    {
        var results = await _unitOfWork.Reviews.GetTopRatedProvidersAsync(request.Count, cancellationToken);

        var dtos = results.Select(r => new TopRatedProviderDto
        {
            ProviderId = r.ProviderId,
            ProviderName = r.ProviderName,
            BusinessName = r.BusinessName,
            AverageRating = r.AverageRating,
            TotalReviews = r.TotalReviews
        }).ToList();

        return Result<IReadOnlyList<TopRatedProviderDto>>.Success(dtos);
    }
}

// ─── Get Review Reports ─────────────────────────────────
public sealed record GetReviewReportsQuery : IRequest<Result<IReadOnlyList<ReviewReportDto>>>
{
    public ReportStatus? StatusFilter { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetReviewReportsQueryHandler : IRequestHandler<GetReviewReportsQuery, Result<IReadOnlyList<ReviewReportDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReviewReportsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<IReadOnlyList<ReviewReportDto>>> Handle(GetReviewReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await _unitOfWork.Reviews.GetReportsAsync(request.StatusFilter, request.Page, request.PageSize, cancellationToken);

        var items = reports.Select(r => new ReviewReportDto
        {
            Id = r.Id,
            ReviewId = r.ReviewId,
            Reason = r.Reason.ToString(),
            Description = r.Description,
            Status = r.Status.ToString(),
            ReportedBy = r.ReportedBy != null ? $"{r.ReportedBy.FirstName} {r.ReportedBy.LastName}" : "",
            CreatedAt = r.CreatedAt
        }).ToList();

        return Result<IReadOnlyList<ReviewReportDto>>.Success(items);
    }
}

// ─── DTOs ────────────────────────────────────────────────
public class ReviewDetailDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerAvatarUrl { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public string? ProviderReply { get; set; }
    public DateTime? RepliedAt { get; set; }
    public string? ProviderName { get; set; }
    public string? BusinessName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TopRatedProviderDto
{
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
}

public class ReviewReportDto
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
