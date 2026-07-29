using Bookify.Application.Common;
using Bookify.Application.DTOs.Users;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Users;

public sealed record GetUserProfileQuery : IRequest<Result<UserProfileDto>>
{
    public Guid UserId { get; init; }
}

public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserProfileQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result<UserProfileDto>.Failure("User not found.", "NOT_FOUND");

        return Result<UserProfileDto>.Success(new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role.ToString(),
            IsBiometricEnabled = user.IsBiometricEnabled,
            PreferredLanguage = user.PreferredLanguage,
            PreferredCurrency = user.PreferredCurrency,
            CreatedAt = user.CreatedAt
        });
    }
}
