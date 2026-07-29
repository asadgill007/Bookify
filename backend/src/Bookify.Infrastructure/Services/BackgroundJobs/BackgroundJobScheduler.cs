using System.Linq.Expressions;
using Bookify.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services.BackgroundJobs;

/// <summary>
/// Simple in-process background job scheduler that runs tasks using <see cref="System.Threading.Channels"/>.
/// This is a development/staging placeholder. Replace with Hangfire or Quartz in production.
/// </summary>
public class BackgroundJobScheduler : IBackgroundJobScheduler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundJobScheduler> _logger;
    private readonly Dictionary<string, Timer> _recurringJobs = new();

    public BackgroundJobScheduler(IServiceScopeFactory scopeFactory, ILogger<BackgroundJobScheduler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Enqueue(Expression<Func<Task>> methodCall)
    {
        var jobId = Guid.NewGuid().ToString("N");
        var func = methodCall.Compile();

        _ = Task.Run(async () =>
        {
            try
            {
                await func();
                _logger.LogDebug("Job {JobId} completed successfully.", jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job {JobId} failed.", jobId);
            }
        });

        _logger.LogDebug("Enqueued job {JobId}", jobId);
        return jobId;
    }

    /// <inheritdoc />
    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay)
    {
        var jobId = Guid.NewGuid().ToString("N");
        var func = methodCall.Compile();

        var timer = new Timer(async _ =>
        {
            try
            {
                await func();
                _logger.LogDebug("Scheduled job {JobId} completed.", jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled job {JobId} failed.", jobId);
            }
        }, null, delay, Timeout.InfiniteTimeSpan);

        _logger.LogDebug("Scheduled job {JobId} with delay {Delay}", jobId, delay);
        return jobId;
    }

    /// <inheritdoc />
    public void Recurring(string jobId, Expression<Func<Task>> methodCall, string cronExpression)
    {
        var interval = CronToTimeSpan(cronExpression) ?? TimeSpan.FromHours(1);
        var func = methodCall.Compile();

        var timer = new Timer(async _ =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                await func();
                _logger.LogDebug("Recurring job {JobId} completed.", jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Recurring job {JobId} failed.", jobId);
            }
        }, null, interval, interval);

        _recurringJobs[jobId] = timer;
        _logger.LogInformation("Registered recurring job {JobId} with interval {Interval}", jobId, interval);
    }

    /// <inheritdoc />
    public void RemoveRecurring(string jobId)
    {
        if (_recurringJobs.TryGetValue(jobId, out var timer))
        {
            timer.Dispose();
            _recurringJobs.Remove(jobId);
            _logger.LogInformation("Removed recurring job {JobId}", jobId);
        }
    }

    /// <inheritdoc />
    public bool Delete(string jobId)
    {
        if (_recurringJobs.TryGetValue(jobId, out var timer))
        {
            timer.Dispose();
            return _recurringJobs.Remove(jobId);
        }
        return false;
    }

    /// <inheritdoc />
    public bool Exists(string jobId) => _recurringJobs.ContainsKey(jobId);

    /// <summary>Simple cron-to-timespan converter for common expressions.</summary>
    private static TimeSpan? CronToTimeSpan(string cron)
    {
        return cron switch
        {
            JobCron.EveryMinute => TimeSpan.FromMinutes(1),
            JobCron.EveryFiveMinutes => TimeSpan.FromMinutes(5),
            JobCron.EveryFifteenMinutes => TimeSpan.FromMinutes(15),
            JobCron.Hourly => TimeSpan.FromHours(1),
            JobCron.Daily or JobCron.DailyMidnight => TimeSpan.FromDays(1),
            JobCron.Weekly => TimeSpan.FromDays(7),
            JobCron.Monthly => TimeSpan.FromDays(30),
            _ => null
        };
    }

}
