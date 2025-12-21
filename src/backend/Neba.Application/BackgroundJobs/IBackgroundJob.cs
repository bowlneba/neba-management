namespace Neba.Application.BackgroundJobs;

#pragma warning disable CA1040 // Avoid empty interfaces

/// <summary>
/// Marker interface for background job data/payload
/// Jobs implementing this interface contain only data - no behavior
/// </summary>
public interface IBackgroundJob;
