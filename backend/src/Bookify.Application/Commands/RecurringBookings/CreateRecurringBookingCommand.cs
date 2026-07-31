using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.RecurringBookings;

public sealed record CreateRecurringBookingCommand : IRequest<Result>
{
    public Guid CustomerId { get; init; }
    public Guid ProviderId { get; init; }
    public Guid ServiceId { get; init; }
    public Guid BusinessId { get; init; }
    public RecurrenceType RecurrenceType { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public DateTime SeriesStartDate { get; init; }
    public DateTime? SeriesEndDate { get; init; }
    public int? MaxOccurrences { get; init; }
    public int Interval { get; init; } = 1;
    public int? DayOfMonth { get; init; }
    public List<DayOfWeek> DaysOfWeek { get; init; } = new();
    public string? Notes { get; init; }
}

public sealed class CreateRecurringBookingCommandValidator : AbstractValidator<CreateRecurringBookingCommand>
{
    public CreateRecurringBookingCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.BusinessId).NotEmpty();
        RuleFor(x => x.RecurrenceType).IsInEnum();
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime).WithMessage("Start time must be before end time.");
        RuleFor(x => x.SeriesStartDate).NotEmpty().GreaterThanOrEqualTo(DateTime.UtcNow.Date);
        RuleFor(x => x.Interval).GreaterThanOrEqualTo(1);
        RuleFor(x => x.DaysOfWeek).Must(d => d.Count > 0).When(x => x.RecurrenceType == RecurrenceType.Weekly)
            .WithMessage("At least one day of week is required for weekly recurrence.");
        RuleFor(x => x.DayOfMonth).NotNull().When(x => x.RecurrenceType == RecurrenceType.Monthly)
            .WithMessage("Day of month is required for monthly recurrence.");
    }
}

public sealed class CreateRecurringBookingCommandHandler : IRequestHandler<CreateRecurringBookingCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IRecurringBookingGeneratorService _recurringGenerator;
    private readonly ILogger<CreateRecurringBookingCommandHandler> _logger;

    public CreateRecurringBookingCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IRecurringBookingGeneratorService recurringGenerator,
        ILogger<CreateRecurringBookingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _recurringGenerator = recurringGenerator;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateRecurringBookingCommand request, CancellationToken cancellationToken)
    {
        // Business validation: provider must exist and be active
        var provider = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId, cancellationToken);
        if (provider == null)
            return Result.Failure("Provider not found.", "NOT_FOUND");
        if (!provider.IsActive)
            return Result.Failure("Provider is not active.", "PROVIDER_INACTIVE");
        if (provider.BusinessId != request.BusinessId)
            return Result.Failure("Provider does not belong to the specified business.", "PROVIDER_MISMATCH");

        // Business validation: service must exist and be active
        var service = await _unitOfWork.Services.GetByIdAsync(request.ServiceId, cancellationToken);
        if (service == null)
            return Result.Failure("Service not found.", "NOT_FOUND");
        if (!service.IsActive)
            return Result.Failure("Service is not active.", "SERVICE_INACTIVE");
        if (service.BusinessId != request.BusinessId)
            return Result.Failure("Service does not belong to the specified business.", "SERVICE_MISMATCH");

        // Business validation: business must exist
        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null)
            return Result.Failure("Business not found.", "NOT_FOUND");

        // Slot validation: check for conflicts on the first few upcoming occurrences
        var checkDates = new List<DateTime>();
        var currentDate = request.SeriesStartDate;
        var checkUntil = request.SeriesEndDate ?? currentDate.AddMonths(1);
        var maxChecks = Math.Min(request.MaxOccurrences ?? 5, 10); // Check up to 10 occurrences or max

        while (currentDate <= checkUntil && checkDates.Count < maxChecks)
        {
            bool shouldInclude = request.RecurrenceType switch
            {
                Domain.Entities.RecurrenceType.Daily => true,
                Domain.Entities.RecurrenceType.Weekly => request.DaysOfWeek.Contains(currentDate.DayOfWeek),
                Domain.Entities.RecurrenceType.Monthly => request.DayOfMonth.HasValue && currentDate.Day == request.DayOfMonth.Value,
                Domain.Entities.RecurrenceType.Custom => true,
                _ => false
            };

            if (shouldInclude && currentDate >= request.SeriesStartDate)
            {
                var occurrenceStart = currentDate.Date.Add(request.StartTime.ToTimeSpan());
                var occurrenceEnd = currentDate.Date.Add(request.EndTime.ToTimeSpan());

                if (occurrenceStart > DateTime.UtcNow)
                {
                    var hasConflict = await _unitOfWork.Appointments.HasConflictAsync(
                        request.ProviderId, occurrenceStart, occurrenceEnd, null, cancellationToken);

                    if (hasConflict)
                        return Result.Failure($"A scheduling conflict was detected for {currentDate:yyyy-MM-dd}. Please choose a different time or provider.", "SLOT_CONFLICT");

                    checkDates.Add(currentDate);
                }
            }

            currentDate = request.RecurrenceType switch
            {
                Domain.Entities.RecurrenceType.Daily => currentDate.AddDays(request.Interval),
                Domain.Entities.RecurrenceType.Weekly => currentDate.AddDays(1),
                Domain.Entities.RecurrenceType.Monthly => currentDate.AddMonths(request.Interval),
                Domain.Entities.RecurrenceType.Custom => currentDate.AddDays(request.Interval),
                _ => currentDate.AddDays(1)
            };
        }

        var booking = new RecurringBooking(
            request.CustomerId,
            request.ProviderId,
            request.ServiceId,
            request.BusinessId,
            request.RecurrenceType,
            request.StartTime,
            request.EndTime,
            request.SeriesStartDate,
            request.SeriesEndDate,
            request.MaxOccurrences,
            request.Interval,
            request.DayOfMonth,
            request.DaysOfWeek,
            request.Notes);

        await _unitOfWork.RecurringBookings.AddAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Materialize the upcoming occurrences immediately so the series is
        // visible right away (the daily background job tops up later dates).
        try
        {
            await _recurringGenerator.GenerateAppointmentsForSeriesAsync(
                booking.Id,
                upTo: DateTime.UtcNow.AddDays(30),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate initial occurrences for recurring series {Id}", booking.Id);
        }

        _logger.LogInformation(
            "Recurring booking created: Type={RecurrenceType}, Provider={ProviderId}, Customer={CustomerId}, Start={SeriesStartDate}",
            request.RecurrenceType, request.ProviderId, request.CustomerId, request.SeriesStartDate);

        // Notify customer
        await _notificationService.SendNotificationAsync(
            request.CustomerId,
            Domain.Enums.NotificationType.BookingConfirmed,
            "Recurring Booking Created",
            $"Your recurring booking series has been created starting {request.SeriesStartDate:d}.",
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}

public sealed record CancelRecurringSeriesCommand : IRequest<Result>
{
    public Guid RecurringBookingId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class CancelRecurringSeriesCommandHandler : IRequestHandler<CancelRecurringSeriesCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CancelRecurringSeriesCommandHandler> _logger;

    public CancelRecurringSeriesCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<CancelRecurringSeriesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result> Handle(CancelRecurringSeriesCommand request, CancellationToken cancellationToken)
    {
        var booking = await _unitOfWork.RecurringBookings.GetByIdAsync(request.RecurringBookingId, cancellationToken);
        if (booking == null)
            return Result.Failure("Recurring booking not found.", "NOT_FOUND");

        booking.CancelSeries();

        // Cancel all future occurrences that were generated from this series.
        var futureAppointments = await _unitOfWork.Appointments.GetFutureByRecurringBookingAsync(
            request.RecurringBookingId, cancellationToken);
        foreach (var appointment in futureAppointments)
        {
            appointment.Cancel("Recurring series cancelled");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Recurring series {Id} cancelled by user {UserId} ({Count} future occurrences cancelled)",
            request.RecurringBookingId, request.UserId, futureAppointments.Count);

        await _notificationService.SendNotificationAsync(
            booking.CustomerId,
            Domain.Enums.NotificationType.BookingCancelled,
            "Recurring Series Cancelled",
            "Your recurring booking series has been cancelled.",
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}

public sealed record SkipRecurringOccurrenceCommand : IRequest<Result>
{
    public Guid RecurringBookingId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class SkipRecurringOccurrenceCommandValidator : AbstractValidator<SkipRecurringOccurrenceCommand>
{
    public SkipRecurringOccurrenceCommandValidator()
    {
        RuleFor(x => x.RecurringBookingId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class SkipRecurringOccurrenceCommandHandler : IRequestHandler<SkipRecurringOccurrenceCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SkipRecurringOccurrenceCommandHandler> _logger;

    public SkipRecurringOccurrenceCommandHandler(IUnitOfWork unitOfWork, ILogger<SkipRecurringOccurrenceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(SkipRecurringOccurrenceCommand request, CancellationToken cancellationToken)
    {
        var booking = await _unitOfWork.RecurringBookings.GetByIdAsync(request.RecurringBookingId, cancellationToken);
        if (booking == null)
            return Result.Failure("Recurring booking not found.", "NOT_FOUND");
        if (!booking.IsActive)
            return Result.Failure("Recurring series is not active.", "SERIES_INACTIVE");

        booking.IncrementOccurrencesCreated();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Skipped next occurrence for recurring series {Id} by user {UserId}",
            request.RecurringBookingId, request.UserId);

        return Result.Success();
    }
}

public sealed record UpdateRecurringScheduleCommand : IRequest<Result>
{
    public Guid RecurringBookingId { get; init; }
    public Guid UserId { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public DateTime? SeriesEndDate { get; init; }
    public int? MaxOccurrences { get; init; }
    public string? Notes { get; init; }
}

public sealed class UpdateRecurringScheduleCommandValidator : AbstractValidator<UpdateRecurringScheduleCommand>
{
    public UpdateRecurringScheduleCommandValidator()
    {
        RuleFor(x => x.RecurringBookingId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        When(x => x.StartTime.HasValue && x.EndTime.HasValue, () =>
        {
            RuleFor(x => x.StartTime!.Value)
                .LessThan(x => x.EndTime!.Value)
                .WithMessage("Start time must be before end time.");
        });
    }
}

public sealed class UpdateRecurringScheduleCommandHandler : IRequestHandler<UpdateRecurringScheduleCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateRecurringScheduleCommandHandler> _logger;

    public UpdateRecurringScheduleCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateRecurringScheduleCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateRecurringScheduleCommand request, CancellationToken cancellationToken)
    {
        var booking = await _unitOfWork.RecurringBookings.GetByIdAsync(request.RecurringBookingId, cancellationToken);
        if (booking == null)
            return Result.Failure("Recurring booking not found.", "NOT_FOUND");

        var newStart = request.StartTime ?? booking.StartTime;
        var newEnd = request.EndTime ?? booking.EndTime;

        if (newStart >= newEnd)
            return Result.Failure("Start time must be before end time.", "INVALID_TIME_RANGE");

        booking.UpdateSchedule(newStart, newEnd, request.SeriesEndDate, request.MaxOccurrences, request.Notes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recurring series {Id} schedule updated by user {UserId}", request.RecurringBookingId, request.UserId);
        return Result.Success();
    }
}
