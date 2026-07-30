using Bookify.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Email service implementation with test mode support.
/// In test mode, emails are logged but not sent.
/// In production, uses SMTP (can be configured for SendGrid, Mailgun, etc.)
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _settings;

    public EmailService(
        ILogger<EmailService> logger,
        IOptions<EmailSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task SendVerificationEmailAsync(
        string toEmail,
        string toName,
        string verificationToken,
        CancellationToken cancellationToken = default)
    {
        var subject = "Verify Your Email - Bookify";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2>Welcome to Bookify!</h2>
                <p>Hi {toName},</p>
                <p>Please verify your email address by clicking the link below:</p>
                <p><a href='{_settings.BaseUrl}/verify-email?token={verificationToken}'>Verify Email</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you didn't create an account, please ignore this email.</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, body, isHtml: true, cancellationToken: cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string toName,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        var subject = "Reset Your Password - Bookify";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2>Password Reset Request</h2>
                <p>Hi {toName},</p>
                <p>You requested to reset your password. Click the link below:</p>
                <p><a href='{_settings.BaseUrl}/reset-password?token={resetToken}'>Reset Password</a></p>
                <p>This link will expire in 1 hour.</p>
                <p>If you didn't request this, please ignore this email.</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, body, isHtml: true, cancellationToken: cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(
        string toEmail,
        string toName,
        CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to Bookify!";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2>Welcome to Bookify, {toName}!</h2>
                <p>Thank you for joining Bookify - your AI-powered appointment booking platform.</p>
                <p>Get started by exploring our services and booking your first appointment.</p>
                <p><a href='{_settings.BaseUrl}'>Browse Services</a></p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, body, isHtml: true, cancellationToken: cancellationToken);
    }

    public async Task SendAppointmentConfirmationEmailAsync(
        string toEmail,
        string toName,
        string appointmentDetails,
        CancellationToken cancellationToken = default)
    {
        var subject = "Appointment Confirmed - Bookify";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2>Appointment Confirmed!</h2>
                <p>Hi {toName},</p>
                <p>Your appointment has been confirmed. Here are the details:</p>
                <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px;'>
                    {appointmentDetails}
                </div>
                <p>We'll send you a reminder before your appointment.</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, body, isHtml: true, cancellationToken: cancellationToken);
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default)
    {
        if (_settings.UseTestMode)
        {
            _logger.LogInformation("[EMAIL TEST MODE] To: {Email}, Subject: {Subject}", toEmail, subject);
            _logger.LogDebug("[EMAIL TEST MODE] Body: {Body}", body);
            await Task.CompletedTask;
            return;
        }

        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword),
                EnableSsl = _settings.SmtpUseSsl
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(new MailAddress(toEmail));

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }
}

public class EmailSettings
{
    public bool UseTestMode { get; set; } = true;
    public string BaseUrl { get; set; } = "https://bookify.app";
    public string FromEmail { get; set; } = "noreply@bookify.app";
    public string FromName { get; set; } = "Bookify";
    
    // SMTP Settings (used when UseTestMode = false)
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool SmtpUseSsl { get; set; } = true;
}