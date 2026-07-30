using Bookify.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Bookify.Infrastructure.Tests.Services;

public class VirusScanServiceTests
{
    [Fact]
    public async Task ScanAsync_TestMode_ReturnsCleanResult()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<VirusScanService>();
        var settings = Options.Create(new VirusScanSettings { UseTestMode = true });
        var service = new VirusScanService(logger, settings);
        using var stream = new MemoryStream(new byte[100]);

        // Act
        var result = await service.ScanAsync(stream, "test.pdf");

        // Assert
        Assert.True(result.IsClean);
        Assert.Null(result.ThreatDescription);
    }

    [Fact]
    public async Task ScanAsync_TestMode_ReturnsCleanForAnyFile()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<VirusScanService>();
        var settings = Options.Create(new VirusScanSettings { UseTestMode = true });
        var service = new VirusScanService(logger, settings);
        using var stream = new MemoryStream(new byte[1000]);

        // Act
        var result = await service.ScanAsync(stream, "malware.exe");

        // Assert
        Assert.True(result.IsClean);
    }
}