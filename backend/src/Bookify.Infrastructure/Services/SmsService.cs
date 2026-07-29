using Bookify.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Stub SMS service. Replace with Twilio, Vonage, or other provider integration.
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public Task SendVerificationSmsAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SMS STUB] Verification SMS to {Phone}: code={Code}", phoneNumber, code);
        return Task.CompletedTask;
    }

    public Task SendAppointmentReminderSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SMS STUB] Reminder SMS to {Phone}: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }

    public Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SMS STUB] SMS to {Phone}: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }
}
