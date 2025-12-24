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

        return BackgroundJob.Enqueue(() => ExecuteJobAsync(job, CancellationToken.None));
    }

    public string Schedule<TJob>(TJob job, TimeSpan delay) where TJob : IBackgroundJob
    {
        logger.LogSchedulingJob(typeof(TJob).Name, delay);

        return BackgroundJob.Schedule(() => ExecuteJobAsync(job, CancellationToken.None), delay);
    }

    public string Schedule<TJob>(TJob job, DateTimeOffset enqueueAt) where TJob : IBackgroundJob
    {
        logger.LogSchedulingJob(typeof(TJob).Name, enqueueAt);

        return BackgroundJob.Schedule(() => ExecuteJobAsync(job, CancellationToken.None), enqueueAt);
    }

    public void AddOrUpdateRecurring<TJob>(string recurringJobId, TJob job, string cronExpression) where TJob : IBackgroundJob
    {
        logger.LogAddingOrUpdatingRecurringJob(
            recurringJobId, typeof(TJob).Name, cronExpression);

        RecurringJob.AddOrUpdate(
            recurringJobId,
            () => ExecuteJobAsync(job, CancellationToken.None),
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

        return BackgroundJob.ContinueJobWith(parentJobId, () => ExecuteJobAsync(job, CancellationToken.None));
    }

    public bool Delete(string jobId)
    {
        logger.LogJobDeleted(jobId);

        return BackgroundJob.Delete(jobId);
    }

    public async Task ExecuteJobAsync<TJob>(TJob job, CancellationToken cancellationToken) where TJob : IBackgroundJob
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IBackgroundJobHandler<TJob> handler = scope.ServiceProvider.GetRequiredService<IBackgroundJobHandler<TJob>>();

        logger.LogJobStarted(typeof(TJob).Name);

        await handler.ExecuteAsync(job, cancellationToken);
    }
}
