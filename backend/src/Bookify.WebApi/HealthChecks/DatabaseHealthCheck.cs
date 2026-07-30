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
            // InMemory provider doesn't support ExecuteSqlRaw, so use CanConnect instead
            if (_dbContext.Database.IsInMemory())
            {
                // Verify the in-memory database is accessible
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                return canConnect
                    ? HealthCheckResult.Healthy("In-memory database is reachable.")
                    : HealthCheckResult.Unhealthy("In-memory database is unreachable.");
            }

            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            return HealthCheckResult.Healthy("Database is reachable.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database is unreachable.", ex);
        }
    }
}
