using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.BackgroundJobs;

internal static partial class BackgroundJobLogMessages
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Enqueuing job of type {JobType}"
    )]
    public static partial void LogEnqueuingJob(this ILogger<HangfireBackgroundJobScheduler> logger, string jobType);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Scheduling job of type {JobType} after delay of {Delay}"
    )]
    public static partial void LogSchedulingJob(this ILogger<HangfireBackgroundJobScheduler> logger, string jobType, TimeSpan delay);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Scheduling job of type {JobType} at {EnqueueAt}"
    )]
    public static partial void LogSchedulingJob(this ILogger<HangfireBackgroundJobScheduler> logger, string jobType, DateTimeOffset enqueueAt);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Adding or updating recurring job {RecurringJobId} of type {JobType} with cron expression {CronExpression}"
    )]
    public static partial void LogAddingOrUpdatingRecurringJob(this ILogger<HangfireBackgroundJobScheduler> logger,
        string recurringJobId, string jobType, string cronExpression);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Recurring job with id {RecurringJobId} has been removed"
    )]
    public static partial void LogRecurringJobRemoved(this ILogger<HangfireBackgroundJobScheduler> logger, string recurringJobId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Continuing with job of type {JobType} after parent job ID {ParentJobId}"
    )]
    public static partial void LogContinuingWithJob(this ILogger<HangfireBackgroundJobScheduler> logger,
        string parentJobId, string jobType);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Job with ID {JobId} has been deleted"
    )]
    public static partial void LogJobDeleted(this ILogger<HangfireBackgroundJobScheduler> logger, string jobId);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Job of type {JobType} started execution"
    )]
    public static partial void LogJobStarted(this ILogger<HangfireBackgroundJobScheduler> logger, string jobType);
}
