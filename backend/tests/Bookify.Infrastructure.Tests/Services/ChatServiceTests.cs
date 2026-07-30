using Bookify.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Bookify.Infrastructure.Tests.Services;

public class ChatServiceTests
{
    [Fact]
    public async Task SendMessageAsync_TestMode_ReturnsSuccessResult()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<ChatService>();
        var settings = Options.Create(new ChatSettings { UseTestMode = true });
        var service = new ChatService(logger, settings);

        // Act
        var result = await service.SendMessageAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Hello, this is a test message");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.MessageId);
        Assert.NotNull(result.SentAt);
    }

    [Fact]
    public async Task MarkAsReadAsync_TestMode_ReturnsTrue()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<ChatService>();
        var settings = Options.Create(new ChatSettings { UseTestMode = true });
        var service = new ChatService(logger, settings);

        // Act
        var result = await service.MarkAsReadAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteMessageAsync_TestMode_ReturnsTrue()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<ChatService>();
        var settings = Options.Create(new ChatSettings { UseTestMode = true });
        var service = new ChatService(logger, settings);

        // Act
        var result = await service.DeleteMessageAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.True(result);
    }
}