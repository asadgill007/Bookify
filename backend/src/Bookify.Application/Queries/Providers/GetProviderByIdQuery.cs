using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Providers;

public sealed record GetProviderByIdQuery : IRequest<Result<ProviderDetailDto>>
{
    public Guid ProviderId { get; init; }
}

public sealed class ProviderDetailDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid BusinessId { get; init; }
    public string? Title { get; init; }
    public string? Bio { get; init; }
    public bool IsActive { get; init; }
    public int DisplayOrder { get; init; }
    public ProviderUserDto? User { get; init; }
}

public sealed class ProviderUserDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
}

public sealed class GetProviderByIdQueryHandler : IRequestHandler<GetProviderByIdQuery, Result<ProviderDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProviderByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProviderDetailDto>> Handle(GetProviderByIdQuery request, CancellationToken cancellationToken)
    {
        var provider = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId, cancellationToken);

        if (provider == null)
            return Result<ProviderDetailDto>.Failure("Provider not found.", "NOT_FOUND");

        return Result<ProviderDetailDto>.Success(new ProviderDetailDto
        {
            Id = provider.Id,
            UserId = provider.UserId,
            BusinessId = provider.BusinessId,
            Title = provider.Title,
            Bio = provider.Bio,
            IsActive = provider.IsActive,
            DisplayOrder = provider.DisplayOrder,
            User = provider.User != null ? new ProviderUserDto
            {
                FirstName = provider.User.FirstName,
                LastName = provider.User.LastName,
                AvatarUrl = provider.User.AvatarUrl
            } : null
        });
    }
}
