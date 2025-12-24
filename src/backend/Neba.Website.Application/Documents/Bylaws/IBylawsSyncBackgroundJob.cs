namespace Neba.Website.Application.Documents.Bylaws;

/// <summary>
/// Defines operations for scheduling and triggering bylaws document sync jobs.
/// </summary>
internal interface IBylawsSyncBackgroundJob
{
    /// <summary>
    /// Triggers an immediate sync of the bylaws document to storage.
    /// </summary>
    /// <returns>The job ID of the enqueued sync job.</returns>
    string TriggerImmediateSync();
}
