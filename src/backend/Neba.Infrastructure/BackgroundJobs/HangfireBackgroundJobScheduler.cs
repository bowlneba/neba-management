using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Neba.Application.BackgroundJobs;

namespace Neba.Infrastructure.BackgroundJobs;

internal sealed class HangfireBackgroundJobScheduler
    : IBackgroundJobScheduler
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public HangfireBackgroundJobScheduler(
        IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public string Enqueue<TJob>(TJob job) where TJob : IBackgroundJob
    {
        return BackgroundJob.Enqueue(() => ExecuteJobAsync(job, CancellationToken.None));
    }

    public string Schedule<TJob>(TJob job, TimeSpan delay) where TJob : IBackgroundJob
    {
        return BackgroundJob.Schedule(() => ExecuteJobAsync(job, CancellationToken.None), delay);
    }

    public string Schedule<TJob>(TJob job, DateTimeOffset enqueueAt) where TJob : IBackgroundJob
    {
        return BackgroundJob.Schedule(() => ExecuteJobAsync(job, CancellationToken.None), enqueueAt);
    }

    public void AddOrUpdateRecurring<TJob>(string recurringJobId, TJob job, string cronExpression) where TJob : IBackgroundJob
    {
        RecurringJob.AddOrUpdate(
            recurringJobId,
            () => ExecuteJobAsync(job, CancellationToken.None),
            cronExpression);
    }

    public void RemoveRecurring(string recurringJobId)
    {
        RecurringJob.RemoveIfExists(recurringJobId);
    }

    public string ContinueWith<TJob>(string parentJobId, TJob job) where TJob : IBackgroundJob
    {
        return BackgroundJob.ContinueJobWith(parentJobId, () => ExecuteJobAsync(job, CancellationToken.None));
    }

    public bool Delete(string jobId)
    {
        return BackgroundJob.Delete(jobId);
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [30, 60, 120])]
    private async Task ExecuteJobAsync<TJob>(TJob job, CancellationToken cancellationToken) where TJob : IBackgroundJob
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IBackgroundJobHandler<TJob> handler = scope.ServiceProvider.GetRequiredService<IBackgroundJobHandler<TJob>>();

        await handler.ExecuteAsync(job, cancellationToken);
    }
}
