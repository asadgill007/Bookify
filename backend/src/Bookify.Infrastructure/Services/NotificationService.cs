using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IUnitOfWork unitOfWork, ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SendNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        string? data = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification(userId, type, title, body, data);
        await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Notification sent to user {UserId}: {Title}",
            userId, title);
    }

    public async Task SendAppointmentReminderAsync(
        Guid userId,
        Guid appointmentId,
        DateTime appointmentTime,
        CancellationToken cancellationToken = default)
    {
        var hoursUntilAppointment = (appointmentTime - DateTime.UtcNow).TotalHours;
        var title = hoursUntilAppointment <= 1
            ? "Appointment in 1 hour!"
            : "Upcoming Appointment Reminder";

        var body = hoursUntilAppointment <= 1
            ? $"Your appointment is coming up soon! Don't forget."
            : $"You have an appointment in {Math.Round(hoursUntilAppointment)} hours.";

        await SendNotificationAsync(
            userId,
            NotificationType.AppointmentReminder,
            title,
            body,
            System.Text.Json.JsonSerializer.Serialize(new { appointmentId }),
            cancellationToken);
    }

    public async Task SendBulkNotificationAsync(
        IReadOnlyList<Guid> userIds,
        NotificationType type,
        string title,
        string body,
        CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds)
        {
            await SendNotificationAsync(userId, type, title, body, null, cancellationToken);
        }
    }
}
