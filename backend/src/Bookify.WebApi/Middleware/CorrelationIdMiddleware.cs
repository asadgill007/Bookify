using Serilog.Context;

namespace Bookify.WebApi.Middleware;

/// <summary>
/// Middleware that assigns or preserves a correlation ID for every HTTP request.
/// The correlation ID is added to the response header and to the Serilog log context
/// so all log entries for a single request share the same correlation ID.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Use existing correlation ID from the request header, or generate a new one
        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out var existingId)
            && !string.IsNullOrWhiteSpace(existingId)
            ? existingId.ToString()
            : Guid.NewGuid().ToString("N");

        // Add to response headers so the caller can correlate logs
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            return Task.CompletedTask;
        });

        // Push correlation ID into the Serilog log context for structured logging
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogDebug("Request {Method} {Path} with CorrelationId {CorrelationId}",
                context.Request.Method, context.Request.Path, correlationId);

            await _next(context);
        }
    }
}
