using Neba.Application.Caching;

namespace Neba.Website.Application.Documents.Bylaws;

/// <summary>
/// Constants used for storing and retrieving the association's bylaws document.
/// </summary>
public static class BylawsConstants
{
    /// <summary>
    /// The container where bylaws documents are stored.
    /// </summary>
    public const string Container = "documents";

    /// <summary>
    /// The logical name of the bylaws document resource.
    /// </summary>
    public const string DocumentKey = "bylaws";

    /// <summary>
    /// The path within the container where the bylaws HTML file is stored.
    /// </summary>
    public const string Path = "bylaws.html";

    /// <summary>
    /// Cache key for the bylaws document content.
    /// Follows ADR-002 naming convention: website:doc:bylaws:content
    /// </summary>
    public static string ContentCacheKey => CacheKeys.Documents.Content(DocumentKey);

    /// <summary>
    /// Cache key for tracking bylaws document sync job state.
    /// Follows ADR-002 naming convention: website:job:doc-sync:bylaws:current
    /// </summary>
    public static string JobStateCacheKey => CacheKeys.Documents.JobState(DocumentKey);
}
