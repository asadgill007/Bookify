using Bookify.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Bookify.Infrastructure.Tests.Services;

public class PushNotificationServiceTests
{
    [Fact]
    public async Task SendPushNotificationAsync_TestMode_DoesNotThrow()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<PushNotificationService>();
        var settings = Options.Create(new PushNotificationSettings { UseTestMode = true });
        var service = new PushNotificationService(logger, settings);

        // Act & Assert
        await service.SendPushNotificationAsync(
            "test-device-token",
            "Test Title",
            "Test Body",
            new Dictionary<string, string> { { "key", "value" } });
        
        // If we get here without exception, test passes
        Assert.True(true);
    }

    [Fact]
    public async Task SendPushNotificationToMultipleAsync_TestMode_DoesNotThrow()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<PushNotificationService>();
        var settings = Options.Create(new PushNotificationSettings { UseTestMode = true });
        var service = new PushNotificationService(logger, settings);
        var tokens = new List<string> { "token1", "token2", "token3" };

        // Act & Assert
        await service.SendPushNotificationToMultipleAsync(
            tokens,
            "Test Title",
            "Test Body");
        
        Assert.True(true);
    }

    [Fact]
    public async Task SendTopicNotificationAsync_TestMode_DoesNotThrow()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<PushNotificationService>();
        var settings = Options.Create(new PushNotificationSettings { UseTestMode = true });
        var service = new PushNotificationService(logger, settings);

        // Act & Assert
        await service.SendTopicNotificationAsync(
            "all-users",
            "Test Title",
            "Test Body");
        
        Assert.True(true);
    }
}