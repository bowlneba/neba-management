namespace Neba.Application.BackgroundJobs;

/// <summary>
/// Handler interface for background jobs
/// Handlers contain the actual execution logic and receive dependencies via DI
/// </summary>
/// <typeparam name="TJob"></typeparam>
public interface IBackgroundJobHandler<in TJob>
    where TJob : IBackgroundJob
{
    /// <summary>
    /// Handle the execution of the background job
    /// </summary>
    /// <param name="job"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteAsync(TJob job, CancellationToken cancellationToken);
}
