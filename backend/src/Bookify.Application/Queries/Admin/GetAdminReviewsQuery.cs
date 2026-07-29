using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Admin;

public sealed record GetAdminReviewsQuery : IRequest<Result<PaginatedList<AdminReviewDto>>>
{
    public bool? Published { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetAdminReviewsQueryHandler : IRequestHandler<GetAdminReviewsQuery, Result<PaginatedList<AdminReviewDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminReviewsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<AdminReviewDto>>> Handle(GetAdminReviewsQuery request, CancellationToken cancellationToken)
    {
        var total = await _unitOfWork.Reviews.GetCountAsync(request.Published, cancellationToken);
        var items = await _unitOfWork.Reviews.GetFilteredAsync(
            request.Published, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(r => new AdminReviewDto
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            IsPublished = r.IsPublished,
            IsVerifiedPurchase = r.IsVerifiedPurchase,
            CreatedAt = r.CreatedAt,
            CustomerName = r.Customer != null ? $"{r.Customer.FirstName} {r.Customer.LastName}" : "",
            BusinessName = r.Business?.Name ?? ""
        }).ToList();

        return Result<PaginatedList<AdminReviewDto>>.Success(
            new PaginatedList<AdminReviewDto>(dtos, request.Page, request.PageSize, total));
    }
}

public class AdminReviewDto
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsPublished { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
