using System.ComponentModel;
using System.Diagnostics;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neba.Application.BackgroundJobs;

namespace Neba.Infrastructure.BackgroundJobs;

internal sealed class HangfireBackgroundJobScheduler(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<HangfireBackgroundJobScheduler> logger)
        : IBackgroundJobScheduler
{
    private static readonly ActivitySource ActivitySource = new("Neba.Hangfire");

    public string Enqueue<TJob>(TJob job) where TJob : IBackgroundJob
    {
        logger.LogEnqueuingJob(typeof(TJob).Name);

        string jobName = GetJobDisplayName(job);
        return BackgroundJob.Enqueue<HangfireBackgroundJobScheduler>(
            x => x.ExecuteJobAsync(job, jobName, CancellationToken.None));
    }

    public string Schedule<TJob>(TJob job, TimeSpan delay) where TJob : IBackgroundJob
    {
        logger.LogSchedulingJob(typeof(TJob).Name, delay);

        string jobName = GetJobDisplayName(job);
        return BackgroundJob.Schedule<HangfireBackgroundJobScheduler>(
            x => x.ExecuteJobAsync(job, jobName, CancellationToken.None),
            delay);
    }

    public string Schedule<TJob>(TJob job, DateTimeOffset enqueueAt) where TJob : IBackgroundJob
    {
        logger.LogSchedulingJob(typeof(TJob).Name, enqueueAt);

        string jobName = GetJobDisplayName(job);
        return BackgroundJob.Schedule<HangfireBackgroundJobScheduler>(
            x => x.ExecuteJobAsync(job, jobName, CancellationToken.None),
            enqueueAt);
    }

    public void AddOrUpdateRecurring<TJob>(string recurringJobId, TJob job, string cronExpression) where TJob : IBackgroundJob
    {
        logger.LogAddingOrUpdatingRecurringJob(
            recurringJobId, typeof(TJob).Name, cronExpression);

        string jobName = GetJobDisplayName(job);
        RecurringJob.AddOrUpdate<HangfireBackgroundJobScheduler>(
            recurringJobId,
            x => x.ExecuteJobAsync(job, jobName, CancellationToken.None),
            cronExpression);
    }

    public void RemoveRecurring(string recurringJobId)
    {
        logger.LogRecurringJobRemoved(recurringJobId);

        RecurringJob.RemoveIfExists(recurringJobId);
    }

    public string ContinueWith<TJob>(string parentJobId, TJob job) where TJob : IBackgroundJob
    {
        logger.LogContinuingWithJob(parentJobId, typeof(TJob).Name);

        string jobName = GetJobDisplayName(job);
        return BackgroundJob.ContinueJobWith<HangfireBackgroundJobScheduler>(
            parentJobId,
            x => x.ExecuteJobAsync(job, jobName, CancellationToken.None));
    }

    public bool Delete(string jobId)
    {
        logger.LogJobDeleted(jobId);

        return BackgroundJob.Delete(jobId);
    }

    [DisplayName("{1}")]
    public async Task ExecuteJobAsync<TJob>(TJob job, string displayName, CancellationToken cancellationToken) where TJob : IBackgroundJob
    {
        string jobType = typeof(TJob).Name;

        using Activity? activity = ActivitySource.StartActivity($"hangfire.execute_job.{jobType}");

        if (activity is not null)
        {
            activity.SetTag("job.type", jobType);
            activity.SetTag("job.display_name", displayName);
        }

        long startTimestamp = Stopwatch.GetTimestamp();
        HangfireMetrics.RecordJobStart(jobType);

        try
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            IBackgroundJobHandler<TJob> handler = scope.ServiceProvider.GetRequiredService<IBackgroundJobHandler<TJob>>();

            logger.LogJobStarted(jobType);

            await handler.ExecuteAsync(job, cancellationToken);

            double durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
            HangfireMetrics.RecordJobSuccess(jobType, durationMs);

            activity?.SetTag("job.duration_ms", durationMs);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            double durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
            HangfireMetrics.RecordJobFailure(jobType, durationMs, ex.GetType().Name);

            activity?.SetTag("job.duration_ms", durationMs);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            throw;
        }
    }

    private static string GetJobDisplayName<TJob>(TJob job) where TJob : IBackgroundJob
    {
        // Use the job's JobName property for display in the Hangfire dashboard
        return job.JobName;
    }
}
