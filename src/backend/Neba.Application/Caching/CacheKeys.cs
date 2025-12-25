namespace Neba.Application.Caching;

/// <summary>
/// Centralized cache key definitions following ADR-002 naming conventions.
/// </summary>
/// <remarks>
/// <para>
/// Cache keys follow the pattern: {context}:{type}:{identifier}[:{subtype}][:{qualifier}]
/// </para>
/// <para>
/// See: docs/architecture/adr-002-cache-key-naming-conventions.md
/// </para>
/// </remarks>
public static class CacheKeys
{
    /// <summary>
    /// Context identifier for website-bounded context.
    /// </summary>
    public const string WebsiteContext = "website";

    /// <summary>
    /// Context identifier for API bounded context.
    /// </summary>
    public const string ApiContext = "api";

    /// <summary>
    /// Context identifier for shared cross-context caching.
    /// </summary>
    public const string SharedContext = "shared";

    /// <summary>
    /// Cache type identifiers for different kinds of cached data.
    /// </summary>
#pragma warning disable CA1034 // Nested types are intentional for namespace organization
    public static class Types
#pragma warning restore CA1034
    {
        /// <summary>
        /// Type identifier for document content caching.
        /// </summary>
        public const string Document = "doc";

        /// <summary>
        /// Type identifier for query result caching.
        /// </summary>
        public const string Query = "query";

        /// <summary>
        /// Type identifier for background job state caching.
        /// </summary>
        public const string Job = "job";

        /// <summary>
        /// Type identifier for session data caching.
        /// </summary>
        public const string Session = "session";

#pragma warning disable S3218 // Intentionally shadows the nested class name for API consistency
        /// <summary>
        /// Type identifier for awards-related caching.
        /// </summary>
        public const string Awards = "awards";
#pragma warning restore S3218
    }

    /// <summary>
    /// Cache keys for document-related caching.
    /// </summary>
#pragma warning disable CA1034 // Nested types are intentional for namespace organization
#pragma warning disable CA1724 // Type name conflicts with namespace but provides better API organization
    public static class Documents
#pragma warning restore CA1724, CA1034
    {
        /// <summary>
        /// Generates a cache key for document content.
        /// </summary>
        /// <param name="documentKey">The document identifier (e.g., "bylaws", "tournament-rules").</param>
        /// <returns>Cache key in format: website:doc:{documentKey}:content</returns>
        public static string Content(string documentKey)
            => $"{WebsiteContext}:{Types.Document}:{documentKey}:content";

        /// <summary>
        /// Generates a cache key for document metadata.
        /// </summary>
        /// <param name="documentKey">The document identifier.</param>
        /// <returns>Cache key in format: website:doc:{documentKey}:metadata</returns>
        public static string Metadata(string documentKey)
            => $"{WebsiteContext}:{Types.Document}:{documentKey}:metadata";

        /// <summary>
        /// Generates a cache key for tracking document sync job state.
        /// </summary>
        /// <param name="documentKey">The document identifier.</param>
        /// <returns>Cache key in format: website:job:doc-sync:{documentKey}:current</returns>
        public static string JobState(string documentKey)
            => $"{WebsiteContext}:{Types.Job}:doc-sync:{documentKey}:current";
    }

    /// <summary>
    /// Cache keys for query result caching (ICachedQuery implementations).
    /// </summary>
#pragma warning disable CA1034 // Nested types are intentional for namespace organization
    public static class Queries
#pragma warning restore CA1034
    {
        /// <summary>
        /// Builds a cache key for a query with optional parameters.
        /// </summary>
        /// <param name="queryName">The query class name.</param>
        /// <param name="parameters">Optional query parameters to include in the key.</param>
        /// <returns>Cache key in format: website:query:{queryName}[:{param1}:{param2}...]</returns>
        public static string Build(string queryName, params object[] parameters)
        {
#pragma warning disable CA1062 // Validate arguments (params cannot be null)
            string baseKey = $"{WebsiteContext}:{Types.Query}:{queryName}";
            return parameters.Length > 0
                ? $"{baseKey}:{string.Join(':', parameters)}"
                : baseKey;
#pragma warning restore CA1062
        }
    }

    /// <summary>
    /// Cache keys for awards-related caching.
    /// </summary>
#pragma warning disable CA1034 // Nested types are intentional for namespace organization
    public static class Awards
#pragma warning restore CA1034
    {
        /// <summary>
        /// Generates a cache key for listing Bowler of the Year awards.
        /// </summary>
        /// <returns>Cache key in format: website:awards:bowler-of-the-year</returns>
        public static string BowlerOfTheYear()
            => $"{WebsiteContext}:{Types.Awards}:bowler-of-the-year";

        /// <summary>
        /// Generates a cache key for listing High Average awards.
        /// </summary>
        /// <returns>Cache key in format: website:awards:high-average</returns>
        public static string HighAverage()
            => $"{WebsiteContext}:{Types.Awards}:high-average";

        /// <summary>
        /// Generates a cache key for listing High Block (5-game) awards.
        /// </summary>
        /// <returns>Cache key in format: website:awards:high-block</returns>
        public static string HighBlock()
            => $"{WebsiteContext}:{Types.Awards}:high-block";
    }
}
