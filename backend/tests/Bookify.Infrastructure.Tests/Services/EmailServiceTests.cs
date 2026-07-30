using Bookify.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Bookify.Infrastructure.Tests.Services;

public class EmailServiceTests
{
    [Fact]
    public async Task SendEmailAsync_TestMode_DoesNotThrow()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<EmailService>();
        var settings = Options.Create(new EmailSettings 
        { 
            UseTestMode = true,
            FromEmail = "test@bookify.app",
            FromName = "Bookify Test"
        });
        var service = new EmailService(logger, settings);

        // Act & Assert
        await service.SendEmailAsync(
            toEmail: "recipient@example.com",
            subject: "Test Subject",
            body: "Test Body",
            isHtml: false);
        
        Assert.True(true);
    }

    [Fact]
    public async Task SendEmailAsync_TestMode_WithHtml_DoesNotThrow()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<EmailService>();
        var settings = Options.Create(new EmailSettings 
        { 
            UseTestMode = true,
            FromEmail = "test@bookify.app",
            FromName = "Bookify Test"
        });
        var service = new EmailService(logger, settings);

        // Act & Assert
        await service.SendEmailAsync(
            toEmail: "recipient@example.com",
            subject: "Test Subject",
            body: "<h1>Test HTML Body</h1>",
            isHtml: true);
        
        Assert.True(true);
    }
}