namespace Neba.Application.BackgroundJobs;

/// <summary>
/// Interface for scheduling background jobs
/// </summary>
public interface IBackgroundJobScheduler
{
    /// <summary>
    /// Enqueue a fire-and-forget job for immediate execution
    /// </summary>
    /// <param name="job"></param>
    /// <typeparam name="TJob"></typeparam>
    /// <returns></returns>
    string Enqueue<TJob>(TJob job) where TJob : IBackgroundJob;

    /// <summary>
    /// Schedule a job to be executed after a specified delay
    /// </summary>
    /// <param name="job"></param>
    /// <param name="delay"></param>
    /// <typeparam name="TJob"></typeparam>
    /// <returns></returns>
    string Schedule<TJob>(TJob job, TimeSpan delay) where TJob : IBackgroundJob;

    /// <summary>
    /// Schedule a job to be executed at a specific date and time
    /// </summary>
    /// <param name="job"></param>
    /// <param name="enqueueAt"></param>
    /// <typeparam name="TJob"></typeparam>
    /// <returns></returns>
    string Schedule<TJob>(TJob job, DateTimeOffset enqueueAt) where TJob : IBackgroundJob;

    /// <summary>
    /// Schedule a job to be executed after the completion of a parent job
    /// </summary>
    /// <param name="parentJobId"></param>
    /// <param name="job"></param>
    /// <typeparam name="TJob"></typeparam>
    /// <returns></returns>
    string ContinueWith<TJob>(string parentJobId, TJob job) where TJob : IBackgroundJob;
}
