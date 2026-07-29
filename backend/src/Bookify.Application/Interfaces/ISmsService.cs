namespace Bookify.Application.Interfaces;

/// <summary>
/// Abstraction for SMS delivery.
/// Implementation will integrate with Twilio, Vonage, or other SMS providers.
/// </summary>
public interface ISmsService
{
    Task SendVerificationSmsAsync(
        string phoneNumber,
        string code,
        CancellationToken cancellationToken = default);

    Task SendAppointmentReminderSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default);

    Task SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default);
}
