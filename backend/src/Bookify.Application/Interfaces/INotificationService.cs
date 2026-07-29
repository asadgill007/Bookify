using Bookify.Domain.Enums;

namespace Bookify.Application.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        string? data = null,
        CancellationToken cancellationToken = default);

    Task SendAppointmentReminderAsync(
        Guid userId,
        Guid appointmentId,
        DateTime appointmentTime,
        CancellationToken cancellationToken = default);

    Task SendBulkNotificationAsync(
        IReadOnlyList<Guid> userIds,
        NotificationType type,
        string title,
        string body,
        CancellationToken cancellationToken = default);
}
