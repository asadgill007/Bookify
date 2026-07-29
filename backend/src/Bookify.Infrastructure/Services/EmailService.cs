using Bookify.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Stub email service. Replace with SendGrid, SMTP, or other provider integration.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendVerificationEmailAsync(string toEmail, string toName, string verificationToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[EMAIL STUB] Verification email to {Email}: token={Token}", toEmail, verificationToken);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[EMAIL STUB] Password reset email to {Email}: token={Token}", toEmail, resetToken);
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string toEmail, string toName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[EMAIL STUB] Welcome email to {Email}", toEmail);
        return Task.CompletedTask;
    }

    public Task SendAppointmentConfirmationEmailAsync(string toEmail, string toName, string appointmentDetails, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[EMAIL STUB] Appointment confirmation to {Email}: {Details}", toEmail, appointmentDetails);
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[EMAIL STUB] Email to {Email}: {Subject}", toEmail, subject);
        return Task.CompletedTask;
    }
}
