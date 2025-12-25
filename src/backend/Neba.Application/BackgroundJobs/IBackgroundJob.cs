namespace Neba.Application.BackgroundJobs;

#pragma warning disable CA1040 // Avoid empty interfaces

/// <summary>
/// Marker interface for background job data/payload.
/// Jobs implementing this interface contain only data - no behavior.
/// </summary>
public interface IBackgroundJob
{
    /// <summary>
    /// Gets the display name for this job instance to show in monitoring dashboards.
    /// </summary>
    /// <example>
    /// <code>
    /// public string JobName => $"Sync Document: {DocumentKey}";
    /// </code>
    /// </example>
    string JobName { get; }
}
