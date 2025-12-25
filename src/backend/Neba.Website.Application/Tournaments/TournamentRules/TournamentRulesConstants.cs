using Neba.Application.Caching;

namespace Neba.Website.Application.Tournaments.TournamentRules;

/// <summary>
/// Constants used for storing and retrieving the tournament rules document.
/// </summary>
public static class TournamentRulesConstants
{
    /// <summary>
    /// The name of the container (or blob/container) where tournament rules documents are stored.
    /// </summary>
    public const string ContainerName = "tournaments";

    /// <summary>
    /// The logical name of the tournament rules document resource.
    /// </summary>
    public const string DocumentKey = "tournament-rules";

    /// <summary>
    /// The filename used when the tournament rules are persisted as an HTML file.
    /// </summary>
    public const string FileName = "tournament-rules.html";

    /// <summary>
    /// Cache key for the tournament rules document content.
    /// Follows ADR-002 naming convention: website:doc:tournament-rules:content
    /// </summary>
    public static string ContentCacheKey => CacheKeys.Documents.Content(DocumentKey);

    /// <summary>
    /// Cache key for tracking tournament rules document sync job state.
    /// Follows ADR-002 naming convention: website:job:doc-sync:tournament-rules:current
    /// </summary>
    public static string JobStateCacheKey => CacheKeys.Documents.JobState(DocumentKey);
}
