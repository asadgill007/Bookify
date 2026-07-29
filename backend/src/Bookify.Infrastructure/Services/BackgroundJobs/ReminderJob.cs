using Bookify.Application.Interfaces;
using Bookify.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services.BackgroundJobs;

/// <summary>
/// Processes appointment reminder notifications for upcoming appointments.
/// Runs periodically via <see cref="IBackgroundJobScheduler"/>.
/// </summary>
public class ReminderJob : IReminderJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ReminderJob> _logger;

    public ReminderJob(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<ReminderJob> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ProcessAppointmentRemindersAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var inOneHour = now.AddHours(1);
        var in24Hours = now.AddHours(24);

        // Find appointments starting in approximately 1 hour
        var soonAppointments = await GetAppointmentsInRangeAsync(now.AddMinutes(45), inOneHour, cancellationToken);
        foreach (var appointment in soonAppointments)
        {
            await _notificationService.SendNotificationAsync(
                appointment.CustomerId,
                NotificationType.AppointmentReminder,
                "Appointment in 1 Hour",
                $"Your appointment ({appointment.BookingReference}) is in 1 hour.",
                cancellationToken: cancellationToken);

            _logger.LogInformation("Sent 1-hour reminder for appointment {AppointmentId}", appointment.Id);
        }

        // Find appointments in approximately 24 hours
        var nextDayAppointments = await GetAppointmentsInRangeAsync(now.AddHours(23), in24Hours, cancellationToken);
        foreach (var appointment in nextDayAppointments)
        {
            await _notificationService.SendNotificationAsync(
                appointment.CustomerId,
                NotificationType.AppointmentReminder,
                "Appointment Tomorrow",
                $"Reminder: You have an appointment ({appointment.BookingReference}) tomorrow.",
                cancellationToken: cancellationToken);

            _logger.LogInformation("Sent 24-hour reminder for appointment {AppointmentId}", appointment.Id);
        }

        _logger.LogInformation(
            "Processed reminders: {SoonCount} one-hour, {NextDayCount} twenty-four-hour",
            soonAppointments.Count, nextDayAppointments.Count);
    }

    private async Task<IReadOnlyList<Domain.Entities.Appointment>> GetAppointmentsInRangeAsync(
        DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Appointments.GetByStatusDateRangeAsync(
            AppointmentStatus.Confirmed, from, to, cancellationToken);
    }
}
