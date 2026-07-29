namespace Bookify.Application.Interfaces;

/// <summary>
/// Abstraction for email delivery. 
/// Implementation will integrate with SendGrid, SMTP, or other providers.
/// </summary>
public interface IEmailService
{
    Task SendVerificationEmailAsync(
        string toEmail,
        string toName,
        string verificationToken,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(
        string toEmail,
        string toName,
        string resetToken,
        CancellationToken cancellationToken = default);

    Task SendWelcomeEmailAsync(
        string toEmail,
        string toName,
        CancellationToken cancellationToken = default);

    Task SendAppointmentConfirmationEmailAsync(
        string toEmail,
        string toName,
        string appointmentDetails,
        CancellationToken cancellationToken = default);

    Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default);
}
