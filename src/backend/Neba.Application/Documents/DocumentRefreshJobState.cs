namespace Neba.Application.Documents;
/// <summary>
/// Represents the current state of a document refresh job including identifiers,
/// document type, status, timing and any error information. Used to report
/// progress and diagnose failures for background refresh operations.
/// </summary>
public sealed record DocumentRefreshJobState
{
    /// <summary>
    /// The identifier of the background job performing the document refresh.
    /// </summary>
    public string? JobId { get; init; }

    /// <summary>
    /// The type or category of the document being refreshed (for example, "bylaws").
    /// </summary>
    public required string DocumentType { get; init; }

    /// <summary>
    /// The current <see cref="RefreshStatus"/> of the job.
    /// </summary>
    public required RefreshStatus Status { get; init; }

    /// <summary>
    /// Timestamp when the job started.
    /// </summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>
    /// Optional error message describing a failure when <see cref="Status"/> is <see cref="RefreshStatus.Failed"/>.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Identifier of the user or system that triggered the refresh.
    /// </summary>
    public required string TriggeredBy { get; init; }
}
