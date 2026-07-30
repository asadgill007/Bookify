using Bookify.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// SMS service implementation with test mode support.
/// In test mode, SMS messages are logged but not sent.
/// In production, uses Twilio, Vonage, or other SMS providers.
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly SmsSettings _settings;

    public SmsService(
        ILogger<SmsService> logger,
        IOptions<SmsSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task SendVerificationSmsAsync(
        string phoneNumber,
        string code,
        CancellationToken cancellationToken = default)
    {
        var message = $"Your Bookify verification code is: {code}. Valid for 10 minutes.";
        await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task SendAppointmentReminderSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        var smsMessage = $"Bookify Reminder: {message}";
        await SendSmsAsync(phoneNumber, smsMessage, cancellationToken);
    }

    public async Task SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (_settings.UseTestMode)
        {
            _logger.LogInformation("[SMS TEST MODE] To: {Phone}, Message: {Message}", phoneNumber, message);
            await Task.CompletedTask;
            return;
        }

        try
        {
            // Twilio implementation (replace with your preferred provider)
            // This is a placeholder - actual implementation would use Twilio SDK
            _logger.LogInformation("[SMS PRODUCTION] Would send to {Phone}: {Message}", phoneNumber, message);
            
            // TODO: Integrate with actual SMS provider (Twilio, Vonage, etc.)
            // Example with Twilio:
            // var client = new TwilioRestClient(_settings.AccountSid, _settings.AuthToken);
            // await client.SendMessageAsync(_settings.FromNumber, phoneNumber, message);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Phone}", phoneNumber);
            throw;
        }
    }
}

public class SmsSettings
{
    public bool UseTestMode { get; set; } = true;
    
    // Twilio Settings (used when UseTestMode = false)
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
    
    // Alternative: Vonage/Nexmo settings
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}