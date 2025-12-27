using Neba.Application.BackgroundJobs;

namespace Neba.Application.Documents;

/// <summary>
/// Background job for syncing an HTML document to cloud storage.
/// </summary>
public sealed record SyncHtmlDocumentToStorageJob
    : IBackgroundJob
{
    /// <summary>
    /// The key used to retrieve the document from the documents service.
    /// </summary>
    public required string DocumentKey { get; init; }

    /// <summary>
    /// The storage container where the document will be uploaded.
    /// </summary>
    public required string Container { get; init; }

    /// <summary>
    /// The path within the container where the document will be stored.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Optional metadata to associate with the stored document.
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = [];

    /// <summary>
    /// User or system that triggered the sync (e.g., username or "scheduled").
    /// </summary>
    public string TriggeredBy { get; init; } = "system";

    /// <summary>
    /// Document type identifier for broadcasting status updates (e.g., "bylaws-refresh").
    /// </summary>
    public string? HubGroupName { get; init; }

    /// <summary>
    /// Cache key for tracking job state.
    /// </summary>
    public string? CacheKey { get; init; }

    /// <summary>
    /// Cache key for the document content cache (to invalidate on completion).
    /// </summary>
    public string? DocumentCacheKey { get; init; }

    /// <inheritdoc />
    public string JobName
        => $"Sync Document: {DocumentKey}";
}
