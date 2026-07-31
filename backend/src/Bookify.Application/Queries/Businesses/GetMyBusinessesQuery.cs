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
}

public sealed class GetMyBusinessesQueryHandler : IRequestHandler<GetMyBusinessesQuery, Result<IReadOnlyList<MyBusinessDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyBusinessesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<IReadOnlyList<MyBusinessDto>>> Handle(GetMyBusinessesQuery request, CancellationToken cancellationToken)
    {
        var businesses = await _unitOfWork.Businesses.GetByOwnerIdAsync(request.UserId, cancellationToken);

        var dtos = businesses.Select(b => new MyBusinessDto
        {
            Id = b.Id,
            Name = b.Name,
            Slug = b.Slug,
            Description = b.Description,
            City = b.City,
            Country = b.Country,
            IsVerified = b.IsVerified,
            VerificationStatus = b.VerificationStatus.ToString(),
            RejectionReason = b.RejectionReason,
            TotalServices = b.Services.Count(s => s.IsActive),
            TotalProviders = b.Providers.Count,
            CoverImageUrl = b.Images.FirstOrDefault(i => i.IsCover)?.Url ?? b.CoverImageUrl
        }).ToList();

        return Result<IReadOnlyList<MyBusinessDto>>.Success(dtos);
    }
}
