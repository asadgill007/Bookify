namespace Bookify.Application.Interfaces;

/// <summary>
/// Abstraction for OpenTelemetry integration.
/// Implementations will provide distributed tracing, metrics, and logging export
/// to configured backends (Application Insights, Prometheus, Grafana, etc.).
/// 
/// To activate OpenTelemetry:
/// 1. Install NuGet packages:
///    - OpenTelemetry.Extensions.Hosting
///    - OpenTelemetry.Instrumentation.AspNetCore
///    - OpenTelemetry.Instrumentation.EntityFrameworkCore
///    - OpenTelemetry.Exporter.Console (dev) / OpenTelemetry.Exporter.OpenTelemetryProtocol (prod)
/// 2. Configure in Program.cs:
///    builder.Services.AddOpenTelemetry()
///        .WithTracing(tracerProvider => tracerProvider
///            .AddAspNetCoreInstrumentation()
///            .AddEntityFrameworkCoreInstrumentation()
///            .AddConsoleExporter())
///        .WithMetrics(meterProvider => meterProvider
///            .AddAspNetCoreInstrumentation()
///            .AddConsoleExporter());
/// 3. Implement this interface to expose OpenTelemetry status/health.
/// </summary>
public interface IOpenTelemetryService
{
    /// <summary>Returns whether OpenTelemetry is currently active and exporting.</summary>
    bool IsEnabled { get; }

    /// <summary>Gets the current active trace ID, if any.</summary>
    string? CurrentTraceId { get; }
}
