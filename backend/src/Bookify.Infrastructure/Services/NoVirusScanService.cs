using Bookify.Application.Interfaces;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Default passthrough virus scan service that reports all files as clean.
/// Replace with a real implementation (e.g., ClamAV, Windows Defender API) in production.
/// </summary>
public class NoVirusScanService : IVirusScanService
{
    /// <inheritdoc />
    public Task<VirusScanResult> ScanAsync(Stream fileContent, string fileName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VirusScanResult.NotAvailable());
    }
}
