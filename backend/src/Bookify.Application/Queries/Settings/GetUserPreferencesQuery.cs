using Bookify.Application.Common;
using Bookify.Application.DTOs.Settings;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Settings;

public sealed record GetUserPreferencesQuery : IRequest<Result<UserPreferencesDto>>
{
    public Guid UserId { get; init; }
}

public sealed class GetUserPreferencesQueryHandler : IRequestHandler<GetUserPreferencesQuery, Result<UserPreferencesDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserPreferencesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserPreferencesDto>> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result<UserPreferencesDto>.Failure("User not found.", "NOT_FOUND");

        return Result<UserPreferencesDto>.Success(new UserPreferencesDto
        {
            Language = user.PreferredLanguage,
            Currency = user.PreferredCurrency,
            IsDarkMode = user.Preference?.IsDarkMode ?? false,
            IsAmoledMode = user.Preference?.IsAmoledMode ?? false,
            NotificationsEnabled = user.Preference?.NotificationsEnabled ?? true,
            MarketingEmails = user.Preference?.MarketingEmails ?? false
        });
    }
}
