using Bookify.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Bookify.Infrastructure.Tests.Services;

public class SmsServiceTests
{
    [Fact]
    public async Task SendSmsAsync_TestMode_DoesNotThrow()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<SmsService>();
        var settings = Options.Create(new SmsSettings 
        { 
            UseTestMode = true,
            FromNumber = "+1234567890"
        });
        var service = new SmsService(logger, settings);

        // Act & Assert
        await service.SendSmsAsync(
            phoneNumber: "+0987654321",
            message: "Test SMS message");
        
        Assert.True(true);
    }

    [Fact]
    public async Task SendSmsAsync_TestMode_WithSpecialCharacters_DoesNotThrow()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<SmsService>();
        var settings = Options.Create(new SmsSettings 
        { 
            UseTestMode = true,
            FromNumber = "+1234567890"
        });
        var service = new SmsService(logger, settings);

        // Act & Assert
        await service.SendSmsAsync(
            phoneNumber: "+0987654321",
            message: "Test with émojis 🎉 and spëcial chars");
        
        Assert.True(true);
    }
}