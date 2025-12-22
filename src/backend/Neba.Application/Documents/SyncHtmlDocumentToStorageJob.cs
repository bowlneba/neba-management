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
    /// The name of the storage container where the document will be uploaded.
    /// </summary>
    public required string ContainerName { get; init; }

    /// <summary>
    /// The filename to use when storing the document in the container.
    /// </summary>
    public required string DocumentName { get; init; }
}
