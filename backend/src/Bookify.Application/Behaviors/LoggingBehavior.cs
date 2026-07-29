using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Processing {RequestName} at {DateTime}", requestName, DateTime.UtcNow);

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "Completed {RequestName} in {ElapsedMs}ms at {DateTime}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                DateTime.UtcNow);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Failed {RequestName} after {ElapsedMs}ms at {DateTime}. Error: {ErrorMessage}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                DateTime.UtcNow,
                ex.Message);
            throw;
        }
    }
}
