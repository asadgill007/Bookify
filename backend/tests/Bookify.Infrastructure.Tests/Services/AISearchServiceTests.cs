using Bookify.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Bookify.Infrastructure.Tests.Services;

public class AISearchServiceTests
{
    [Fact]
    public async Task InterpretQueryAsync_TestMode_ReturnsSuccessResult()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<AISearchService>();
        var settings = Options.Create(new AISearchSettings { UseTestMode = true });
        var service = new AISearchService(logger, settings);
        var request = new Bookify.Application.Interfaces.AISearchRequest
        {
            Query = "Find a dentist near me"
        };

        // Act
        var result = await service.InterpretQueryAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        var data = result.Data!;
        Assert.Equal("Find Dentist", data.Intent);
        Assert.Contains("location", data.ExtractedFilters.Keys);
        Assert.Equal("near me", data.ExtractedFilters["location"]);
        Assert.Equal(0.85m, data.Confidence);
    }

    [Fact]
    public async Task InterpretQueryAsync_TestMode_SpaQuery_ReturnsCorrectIntent()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<AISearchService>();
        var settings = Options.Create(new AISearchSettings { UseTestMode = true });
        var service = new AISearchService(logger, settings);
        var request = new Bookify.Application.Interfaces.AISearchRequest
        {
            Query = "I need a massage and spa treatment"
        };

        // Act
        var result = await service.InterpretQueryAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Find Spa", result.Data!.Intent);
    }

    [Fact]
    public async Task InterpretQueryAsync_TestMode_BookingQuery_ReturnsCorrectIntent()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<AISearchService>();
        var settings = Options.Create(new AISearchSettings { UseTestMode = true });
        var service = new AISearchService(logger, settings);
        var request = new Bookify.Application.Interfaces.AISearchRequest
        {
            Query = "Book an appointment for tomorrow"
        };

        // Act
        var result = await service.InterpretQueryAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Book Appointment", result.Data!.Intent);
        Assert.Equal("tomorrow", result.Data!.ExtractedFilters["date"]);
    }

    [Fact]
    public async Task InterpretQueryAsync_TestMode_GeneralQuery_ReturnsGeneralSearch()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<AISearchService>();
        var settings = Options.Create(new AISearchSettings { UseTestMode = true });
        var service = new AISearchService(logger, settings);
        var request = new Bookify.Application.Interfaces.AISearchRequest
        {
            Query = "Show me all businesses"
        };

        // Act
        var result = await service.InterpretQueryAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("General Search", result.Data!.Intent);
    }
}