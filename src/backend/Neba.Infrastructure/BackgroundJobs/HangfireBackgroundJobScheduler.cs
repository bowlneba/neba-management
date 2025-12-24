using System.ComponentModel;
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
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IBackgroundJobHandler<TJob> handler = scope.ServiceProvider.GetRequiredService<IBackgroundJobHandler<TJob>>();

        logger.LogJobStarted(typeof(TJob).Name);

        await handler.ExecuteAsync(job, cancellationToken);
    }

    private static string GetJobDisplayName<TJob>(TJob job) where TJob : IBackgroundJob
    {
        // Use the job's JobName property for display in the Hangfire dashboard
        return job.JobName;
    }
}
