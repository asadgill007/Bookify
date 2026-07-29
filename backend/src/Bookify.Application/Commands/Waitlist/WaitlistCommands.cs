using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Waitlist;

public sealed record JoinWaitlistCommand : IRequest<Result<WaitlistJoinResult>>
{
    public Guid CustomerId { get; init; }
    public Guid BusinessId { get; init; }
    public Guid ProviderId { get; init; }
    public Guid ServiceId { get; init; }
    public DateOnly AppointmentDate { get; init; }
    public TimeOnly? PreferredStartTime { get; init; }
    public TimeOnly? PreferredEndTime { get; init; }
    public string? Notes { get; init; }
}

public sealed class JoinWaitlistCommandValidator : AbstractValidator<JoinWaitlistCommand>
{
    public JoinWaitlistCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.BusinessId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.AppointmentDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Appointment date cannot be in the past.");
        When(x => x.PreferredStartTime.HasValue && x.PreferredEndTime.HasValue, () =>
        {
            RuleFor(x => x.PreferredStartTime!.Value)
                .LessThan(x => x.PreferredEndTime!.Value)
                .WithMessage("Preferred start time must be before end time.");
        });
    }
}

public class WaitlistJoinResult
{
    public Guid EntryId { get; set; }
    public int Position { get; set; }
}

public sealed class JoinWaitlistCommandHandler : IRequestHandler<JoinWaitlistCommand, Result<WaitlistJoinResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<JoinWaitlistCommandHandler> _logger;

    public JoinWaitlistCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<JoinWaitlistCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<WaitlistJoinResult>> Handle(JoinWaitlistCommand request, CancellationToken cancellationToken)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null) return Result<WaitlistJoinResult>.Failure("Business not found.", "NOT_FOUND");

        var provider = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId, cancellationToken);
        if (provider == null) return Result<WaitlistJoinResult>.Failure("Provider not found.", "NOT_FOUND");
        if (provider.BusinessId != request.BusinessId)
            return Result<WaitlistJoinResult>.Failure("Provider does not belong to this business.", "PROVIDER_MISMATCH");

        var service = await _unitOfWork.Services.GetByIdAsync(request.ServiceId, cancellationToken);
        if (service == null) return Result<WaitlistJoinResult>.Failure("Service not found.", "NOT_FOUND");
        if (service.BusinessId != request.BusinessId)
            return Result<WaitlistJoinResult>.Failure("Service does not belong to this business.", "SERVICE_MISMATCH");

        var isDuplicate = await _unitOfWork.Waitlist.HasDuplicateAsync(
            request.CustomerId, request.ProviderId, request.AppointmentDate, cancellationToken);
        if (isDuplicate)
            return Result<WaitlistJoinResult>.Failure("You are already on the waitlist for this provider and date.", "DUPLICATE_ENTRY");

        var entry = new WaitlistEntry(
            request.BusinessId, request.ProviderId, request.ServiceId, request.CustomerId,
            request.AppointmentDate, request.PreferredStartTime, request.PreferredEndTime, request.Notes);

        await _unitOfWork.Waitlist.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var position = await _unitOfWork.Waitlist.GetPositionAsync(entry.Id, cancellationToken);

        _logger.LogInformation(
            "Customer {CustomerId} joined waitlist for provider {ProviderId} on {Date} (position {Position})",
            request.CustomerId, request.ProviderId, request.AppointmentDate, position);

        return Result<WaitlistJoinResult>.Success(new WaitlistJoinResult { EntryId = entry.Id, Position = position });
    }
}

public sealed record LeaveWaitlistCommand : IRequest<Result>
{
    public Guid EntryId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class LeaveWaitlistCommandHandler : IRequestHandler<LeaveWaitlistCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LeaveWaitlistCommandHandler> _logger;

    public LeaveWaitlistCommandHandler(IUnitOfWork unitOfWork, ILogger<LeaveWaitlistCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(LeaveWaitlistCommand request, CancellationToken cancellationToken)
    {
        var entry = await _unitOfWork.Waitlist.GetByIdAsync(request.EntryId, cancellationToken);
        if (entry == null) return Result.Failure("Waitlist entry not found.", "NOT_FOUND");
        if (entry.CustomerId != request.UserId) return Result.Failure("Access denied.", "FORBIDDEN");
        if (entry.Status != WaitlistStatus.Waiting) return Result.Failure("Entry is no longer active.", "INVALID_STATUS");

        entry.Cancel();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer {UserId} left waitlist entry {EntryId}", request.UserId, request.EntryId);
        return Result.Success();
    }
}

public sealed record CancelWaitlistCommand : IRequest<Result>
{
    public Guid EntryId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class CancelWaitlistCommandValidator : AbstractValidator<CancelWaitlistCommand>
{
    public CancelWaitlistCommandValidator()
    {
        RuleFor(x => x.EntryId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class CancelWaitlistCommandHandler : IRequestHandler<CancelWaitlistCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelWaitlistCommandHandler> _logger;

    public CancelWaitlistCommandHandler(IUnitOfWork unitOfWork, ILogger<CancelWaitlistCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(CancelWaitlistCommand request, CancellationToken cancellationToken)
    {
        var entry = await _unitOfWork.Waitlist.GetByIdAsync(request.EntryId, cancellationToken);
        if (entry == null) return Result.Failure("Waitlist entry not found.", "NOT_FOUND");

        if (entry.Status != WaitlistStatus.Waiting && entry.Status != WaitlistStatus.Notified)
            return Result.Failure("Entry is no longer active.", "INVALID_STATUS");

        entry.Cancel();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Waitlist entry {EntryId} cancelled by admin {UserId}", request.EntryId, request.UserId);
        return Result.Success();
    }
}

public sealed record PromoteWaitlistCommand : IRequest<Result>
{
    public Guid EntryId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class PromoteWaitlistCommandHandler : IRequestHandler<PromoteWaitlistCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PromoteWaitlistCommandHandler> _logger;

    public PromoteWaitlistCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<PromoteWaitlistCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result> Handle(PromoteWaitlistCommand request, CancellationToken cancellationToken)
    {
        var entry = await _unitOfWork.Waitlist.GetByIdAsync(request.EntryId, cancellationToken);
        if (entry == null) return Result.Failure("Waitlist entry not found.", "NOT_FOUND");

        entry.Promote();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.SendNotificationAsync(
            entry.CustomerId,
            Domain.Enums.NotificationType.BookingConfirmed,
            "You've Been Promoted!",
            "You're next in line. An appointment slot is available for you.",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Waitlist entry {EntryId} promoted by {UserId}", request.EntryId, request.UserId);
        return Result.Success();
    }
}

public sealed record UpdateWaitlistPriorityCommand : IRequest<Result>
{
    public Guid EntryId { get; init; }
    public Guid UserId { get; init; }
    public int NewPriority { get; init; }
}

public sealed class UpdateWaitlistPriorityCommandHandler : IRequestHandler<UpdateWaitlistPriorityCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateWaitlistPriorityCommandHandler> _logger;

    public UpdateWaitlistPriorityCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateWaitlistPriorityCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateWaitlistPriorityCommand request, CancellationToken cancellationToken)
    {
        var entry = await _unitOfWork.Waitlist.GetByIdAsync(request.EntryId, cancellationToken);
        if (entry == null) return Result.Failure("Waitlist entry not found.", "NOT_FOUND");

        entry.UpdatePriority(request.NewPriority);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Waitlist entry {EntryId} priority updated to {Priority}", request.EntryId, request.NewPriority);
        return Result.Success();
    }
}
