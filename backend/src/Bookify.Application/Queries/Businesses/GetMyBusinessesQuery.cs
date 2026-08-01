using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Businesses;

public sealed record GetMyBusinessesQuery : IRequest<Result<IReadOnlyList<MyBusinessDto>>>
{
    public Guid UserId { get; init; }
}

public sealed class MyBusinessDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public string VerificationStatus { get; set; } = "Pending";
    public string? RejectionReason { get; set; }
    public int TotalServices { get; set; }
    public int TotalProviders { get; set; }
    public string? CoverImageUrl { get; set; }

    /// <summary>Auto-verification checklist state for the provider dashboard.</summary>
    public bool IsChecklistComplete { get; set; }
    public IReadOnlyList<BusinessChecklistItemDto> Checklist { get; set; } =
        Array.Empty<BusinessChecklistItemDto>();
}

public sealed class BusinessChecklistItemDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
}

public sealed class GetMyBusinessesQueryHandler : IRequestHandler<GetMyBusinessesQuery, Result<IReadOnlyList<MyBusinessDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBusinessVerificationService _verificationService;

    public GetMyBusinessesQueryHandler(IUnitOfWork unitOfWork, IBusinessVerificationService verificationService)
    {
        _unitOfWork = unitOfWork;
        _verificationService = verificationService;
    }

    public async Task<Result<IReadOnlyList<MyBusinessDto>>> Handle(GetMyBusinessesQuery request, CancellationToken cancellationToken)
    {
        var businesses = await _unitOfWork.Businesses.GetByOwnerIdAsync(request.UserId, cancellationToken);

        var dtos = new List<MyBusinessDto>();
        foreach (var b in businesses)
        {
            // Evaluate the checklist live so the dashboard always reflects the
            // current state (and auto-verifies if the owner just completed it).
            var checklist = await _verificationService.EvaluateAndAutoVerifyAsync(b.Id, cancellationToken);

            // Auto-verify may have just promoted this business, so the live
            // status must come from the checklist result, not the AsNoTracking
            // entity that was loaded before evaluation.
            var liveStatus = checklist.VerificationStatus.ToString();

            dtos.Add(new MyBusinessDto
            {
                Id = b.Id,
                Name = b.Name,
                Slug = b.Slug,
                Description = b.Description,
                City = b.City,
                Country = b.Country,
                IsVerified = liveStatus == "Approved",
                VerificationStatus = liveStatus,
                RejectionReason = b.RejectionReason,
                TotalServices = b.Services.Count(s => s.IsActive),
                TotalProviders = b.Providers.Count,
                CoverImageUrl = b.Images.FirstOrDefault(i => i.IsCover)?.Url ?? b.CoverImageUrl,
                IsChecklistComplete = checklist.IsComplete,
                Checklist = checklist.Items.Select(i => new BusinessChecklistItemDto
                {
                    Key = i.Key,
                    Label = i.Label,
                    IsComplete = i.IsComplete
                }).ToList()
            });
        }

        return Result<IReadOnlyList<MyBusinessDto>>.Success(dtos);
    }
}
