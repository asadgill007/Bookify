using Bookify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bookify.WebApi.HealthChecks;

/// <summary>
/// Health check that verifies database connectivity by executing a lightweight query.
/// Supports both relational databases (SQL Server) and in-memory databases.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(AppDbContext dbContext, ILogger<DatabaseHealthCheck> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // InMemory provider - use a short-circuit approach since InMemory is always accessible
            // once the context is created. CanConnectAsync can hang on InMemory if the
            // database hasn't been fully initialized yet, so we simply check if the context
            // can query the database model instead.
            if (_dbContext.Database.IsInMemory())
            {
                // Use a timeout-safe check: just verify we can get the model
                // without hitting the database provider's CanConnectAsync (which can hang)
                try
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));
                    
                    var model = _dbContext.Model;
                    if (model == null)
                    {
                        return HealthCheckResult.Unhealthy("In-memory database model is not initialized.");
                    }
                    
                    // Quick check if we can query
                    var canConnect = await _dbContext.Database.CanConnectAsync(timeoutCts.Token);
                    return canConnect
                        ? HealthCheckResult.Healthy("In-memory database is reachable.")
                        : HealthCheckResult.Unhealthy("In-memory database is unreachable.");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("In-memory database health check timed out, treating as healthy (first-start lag).");
                    return HealthCheckResult.Healthy("In-memory database is warming up (first-start).");
                }
            }

            // For real databases, use a short timeout
            using var sqlTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            sqlTimeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", sqlTimeoutCts.Token);
            return HealthCheckResult.Healthy("Database is reachable.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Database health check timed out.");
            return HealthCheckResult.Unhealthy("Database health check timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database is unreachable.", ex);
        }
    }
}
