using Bookify.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Stub implementation of <see cref="IOpenTelemetryService"/> that reports
/// OpenTelemetry as disabled. Replace with a real implementation when
/// OpenTelemetry NuGet packages are installed and configured.
/// 
/// Register in DependencyInjection.cs:
/// services.AddSingleton&lt;IOpenTelemetryService, OpenTelemetryService&gt;();
/// </summary>
public sealed class OpenTelemetryService : IOpenTelemetryService
{
    private readonly ILogger<OpenTelemetryService> _logger;

    public OpenTelemetryService(ILogger<OpenTelemetryService> logger)
    {
        _logger = logger;
        _logger.LogInformation("OpenTelemetry is not configured. " +
            "To enable, install OpenTelemetry packages and register in Program.cs");
    }

    /// <summary>Returns false until OpenTelemetry packages are added.</summary>
    public bool IsEnabled => false;

    /// <summary>Returns null until OpenTelemetry is active.</summary>
    public string? CurrentTraceId => null;
}
