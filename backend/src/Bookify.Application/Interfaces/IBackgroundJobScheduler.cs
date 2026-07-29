using System.Linq.Expressions;

namespace Bookify.Application.Interfaces;

/// <summary>
/// Abstraction for scheduling background jobs (Hangfire/Quartz ready).
/// Implementations handle the actual scheduling mechanism.
/// </summary>
public interface IBackgroundJobScheduler
{
    /// <summary>Enqueue a fire-and-forget job.</summary>
    string Enqueue(Expression<Func<Task>> methodCall);

    /// <summary>Schedule a job to run at a specific time.</summary>
    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);

    /// <summary>Schedule a recurring job with a cron expression.</summary>
    void Recurring(string jobId, Expression<Func<Task>> methodCall, string cronExpression);

    /// <summary>Remove a recurring job.</summary>
    void RemoveRecurring(string jobId);

    /// <summary>Delete an enqueued job by its ID.</summary>
    bool Delete(string jobId);

    /// <summary>Check if a job with the given ID exists.</summary>
    bool Exists(string jobId);
}

/// <summary>
/// Standard cron expression helpers for common scheduling intervals.
/// </summary>
public static class JobCron
{
    public const string EveryMinute = "* * * * *";
    public const string EveryFiveMinutes = "*/5 * * * *";
    public const string EveryFifteenMinutes = "*/15 * * * *";
    public const string Hourly = "0 * * * *";
    public const string Daily = "0 0 * * *";
    public const string DailyMidnight = "0 0 * * *";
    public const string DailyMorning = "0 6 * * *";
    public const string DailyEvening = "0 18 * * *";
    public const string Weekly = "0 0 * * 0";
    public const string Monthly = "0 0 1 * *";
}
