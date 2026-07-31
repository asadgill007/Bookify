using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Businesses;

public sealed record BusinessDayHours
{
    public DayOfWeek DayOfWeek { get; init; }
    public TimeOnly OpenTime { get; init; }
    public TimeOnly CloseTime { get; init; }
    public bool IsClosed { get; init; }
}

public sealed record SetBusinessHoursCommand : IRequest<Result>
{
    public Guid BusinessId { get; init; }
    public Guid UserId { get; init; }
    public IReadOnlyList<BusinessDayHours> Hours { get; init; } = Array.Empty<BusinessDayHours>();
}

public sealed class SetBusinessHoursCommandValidator : AbstractValidator<SetBusinessHoursCommand>
{
    public SetBusinessHoursCommandValidator()
    {
        RuleFor(x => x.BusinessId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Hours).NotNull();
        RuleFor(x => x.Hours)
            .Must(hours => hours.Select(h => h.DayOfWeek).Distinct().Count() == hours.Count)
            .WithMessage("Each day of the week can only be specified once.");

        RuleForEach(x => x.Hours)
            .Must(h => h.IsClosed || h.OpenTime < h.CloseTime)
            .WithMessage("Opening time must be before closing time.");
    }
}

public sealed class SetBusinessHoursCommandHandler : IRequestHandler<SetBusinessHoursCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<SetBusinessHoursCommandHandler> _logger;

    public SetBusinessHoursCommandHandler(
        IUnitOfWork unitOfWork,
        IPermissionService permissionService,
        ILogger<SetBusinessHoursCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<Result> Handle(SetBusinessHoursCommand request, CancellationToken cancellationToken)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null)
            return Result.Failure("Business not found.", "NOT_FOUND");

        if (!await _permissionService.CanManageBusinessAsync(request.UserId, business.Id, cancellationToken))
            return Result.Failure("You do not have permission to set hours for this business.", "FORBIDDEN");

        var entries = request.Hours
            .Select(h => new Domain.Entities.BusinessHours(
                business.Id,
                h.DayOfWeek,
                h.OpenTime,
                h.CloseTime,
                h.IsClosed))
            .ToList();

        await _unitOfWork.BusinessHours.ReplaceForBusinessAsync(business.Id, entries, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Business hours set for business {BusinessId} by {UserId} ({Count} days)",
            business.Id, request.UserId, entries.Count);

        return Result.Success();
    }
}
