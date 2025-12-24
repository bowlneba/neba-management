using Neba.Application.BackgroundJobs;

namespace Neba.IntegrationTests.Infrastructure;

/// <summary>
/// No-op implementation of <see cref="IBackgroundJobScheduler"/> for integration tests.
/// Allows background job classes to be constructed without actually scheduling jobs.
/// </summary>
#pragma warning disable CA1812 // Class is instantiated via dependency injection
internal sealed class NoOpBackgroundJobScheduler : IBackgroundJobScheduler
#pragma warning restore CA1812
{
    public string Enqueue<TJob>(TJob job) where TJob : IBackgroundJob
    {
        // Return a fake job ID - no actual job is enqueued
        return Guid.NewGuid().ToString();
    }

    public string Schedule<TJob>(TJob job, TimeSpan delay) where TJob : IBackgroundJob
    {
        // Return a fake job ID - no actual job is scheduled
        return Guid.NewGuid().ToString();
    }

    public string Schedule<TJob>(TJob job, DateTimeOffset enqueueAt) where TJob : IBackgroundJob
    {
        // Return a fake job ID - no actual job is scheduled
        return Guid.NewGuid().ToString();
    }

    public void AddOrUpdateRecurring<TJob>(string recurringJobId, TJob job, string cronExpression) where TJob : IBackgroundJob
    {
        // Do nothing - no recurring job is registered
    }

    public void RemoveRecurring(string recurringJobId)
    {
        // Do nothing - no recurring job to remove
    }

    public string ContinueWith<TJob>(string parentJobId, TJob job) where TJob : IBackgroundJob
    {
        // Return a fake job ID - no actual job is scheduled
        return Guid.NewGuid().ToString();
    }

    public bool Delete(string jobId)
    {
        // Return true - pretend the job was deleted
        return true;
    }
}
