using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Settings;

public sealed record UpdateUserPreferencesCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public string Language { get; init; } = "en";
    public string Currency { get; init; } = "USD";
    public List<string>? Interests { get; init; }
    public bool IsDarkMode { get; init; }
    public bool IsAmoledMode { get; init; }
    public bool NotificationsEnabled { get; init; } = true;
    public bool MarketingEmails { get; init; }
}

public sealed class UpdateUserPreferencesCommandValidator : AbstractValidator<UpdateUserPreferencesCommand>
{
    public UpdateUserPreferencesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Language).NotEmpty().Length(2, 10);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}

public sealed class UpdateUserPreferencesCommandHandler : IRequestHandler<UpdateUserPreferencesCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserPreferencesCommandHandler> _logger;

    public UpdateUserPreferencesCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateUserPreferencesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result.Failure("User not found.", "NOT_FOUND");

        user.UpdatePreferences(request.Language, request.Currency);

        var interests = request.Interests != null ? string.Join(",", request.Interests) : null;
        var existingPref = await _unitOfWork.UserPreferences.GetByUserIdAsync(request.UserId, cancellationToken);

        if (existingPref != null)
        {
            existingPref.Update(
                request.Language,
                request.Currency,
                interests,
                request.IsDarkMode,
                request.IsAmoledMode,
                request.NotificationsEnabled,
                request.MarketingEmails);
        }
        else
        {
            var pref = new Domain.Entities.UserPreference(request.UserId);
            pref.Update(
                request.Language,
                request.Currency,
                interests,
                request.IsDarkMode,
                request.IsAmoledMode,
                request.NotificationsEnabled,
                request.MarketingEmails);

            await _unitOfWork.UserPreferences.AddAsync(pref, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} updated preferences", request.UserId);
        return Result.Success();
    }
}
