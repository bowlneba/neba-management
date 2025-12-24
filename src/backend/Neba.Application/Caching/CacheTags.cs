using Neba.Domain.Identifiers;

namespace Neba.Application.Caching;

/// <summary>
/// Centralized cache tag definitions following ADR-003 tagging strategy.
/// </summary>
/// <remarks>
/// <para>
/// Tags follow a hierarchical pattern for bulk invalidation:
/// </para>
/// <list type="bullet">
///   <item>Level 1: {context}</item>
///   <item>Level 2: {context}:{category}</item>
///   <item>Level 3: {context}:{category}:{entity-id}</item>
/// </list>
/// <para>
/// See: docs/architecture/adr-003-cache-tagging-strategy.md
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Document tags
/// var tags = CacheTags.Documents("bylaws");
/// // Result: ["website", "website:documents", "website:document:bylaws"]
///
/// // Bowler tags
/// var tags = CacheTags.Bowler(bowlerId);
/// // Result: ["website", "website:bowlers", "website:bowler:01ARZ3NDEK..."]
/// </code>
/// </example>
public static class CacheTags
{
    /// <summary>
    /// Creates a complete tag set for a document cache entry.
    /// </summary>
    /// <param name="documentKey">Document identifier (e.g., "bylaws", "tournament-rules").</param>
    /// <returns>Array of tags: [context, type, entity]</returns>
    /// <example>
    /// <code>
    /// var tags = CacheTags.Documents("bylaws");
    /// // Returns: ["website", "website:documents", "website:document:bylaws"]
    /// </code>
    /// </example>
    public static string[] Documents(string documentKey) =>
        [
            CacheKeys.WebsiteContext,
            $"{CacheKeys.WebsiteContext}:documents",
            $"{CacheKeys.WebsiteContext}:document:{documentKey}"
        ];

    /// <summary>
    /// Creates a complete tag set for a bowler cache entry.
    /// </summary>
    /// <param name="bowlerId">The unique identifier of the bowler.</param>
    /// <returns>Array of tags: [context, type, entity]</returns>
    /// <example>
    /// <code>
    /// var bowlerId = new BowlerId("01ARZ3NDEKTSV4RRFFQ69G5FAV");
    /// var tags = CacheTags.Bowler(bowlerId);
    /// // Returns: ["website", "website:bowlers", "website:bowler:01ARZ3NDEKTSV4RRFFQ69G5FAV"]
    /// </code>
    /// </example>
    public static string[] Bowler(BowlerId bowlerId) =>
        [
            CacheKeys.WebsiteContext,
            $"{CacheKeys.WebsiteContext}:bowlers",
            $"{CacheKeys.WebsiteContext}:bowler:{bowlerId}"
        ];

    /// <summary>
    /// Creates a complete tag set for all bowlers (list queries).
    /// </summary>
    /// <returns>Array of tags: [context, type]</returns>
    /// <example>
    /// <code>
    /// var tags = CacheTags.AllBowlers();
    /// // Returns: ["website", "website:bowlers"]
    /// </code>
    /// </example>
    public static string[] AllBowlers() =>
        [
            CacheKeys.WebsiteContext,
            $"{CacheKeys.WebsiteContext}:bowlers"
        ];

    /// <summary>
    /// Creates a complete tag set for a tournament cache entry.
    /// </summary>
    /// <param name="tournamentId">The unique identifier of the tournament.</param>
    /// <returns>Array of tags: [context, type, entity]</returns>
    /// <example>
    /// <code>
    /// var tournamentId = new TournamentId("01ARZ3NDEKTSV4RRFFQ69G5FAV");
    /// var tags = CacheTags.Tournament(tournamentId);
    /// // Returns: ["website", "website:tournaments", "website:tournament:01ARZ3NDEKTSV4RRFFQ69G5FAV"]
    /// </code>
    /// </example>
    public static string[] Tournament(object tournamentId) =>
        [
            CacheKeys.WebsiteContext,
            $"{CacheKeys.WebsiteContext}:tournaments",
            $"{CacheKeys.WebsiteContext}:tournament:{tournamentId}"
        ];

    /// <summary>
    /// Creates a complete tag set for all tournaments (list queries).
    /// </summary>
    /// <returns>Array of tags: [context, type]</returns>
    public static string[] AllTournaments() =>
        [
            CacheKeys.WebsiteContext,
            $"{CacheKeys.WebsiteContext}:tournaments"
        ];

    /// <summary>
    /// Creates a complete tag set for award cache entries.
    /// </summary>
    /// <param name="awardType">Award type identifier (e.g., "bowler-of-the-year", "high-average", "high-block").</param>
    /// <returns>Array of tags: [context, type, entity]</returns>
    /// <example>
    /// <code>
    /// var tags = CacheTags.Award("bowler-of-the-year");
    /// // Returns: ["website", "website:awards", "website:award:bowler-of-the-year"]
    /// </code>
    /// </example>
    public static string[] Award(string awardType) =>
        [
            CacheKeys.WebsiteContext,
            $"{CacheKeys.WebsiteContext}:awards",
            $"{CacheKeys.WebsiteContext}:award:{awardType}"
        ];

    /// <summary>
    /// Creates a complete tag set for all awards.
    /// </summary>
    /// <returns>Array of tags: [context, type]</returns>
    /// <example>
    /// <code>
    /// var tags = CacheTags.AllAwards();
    /// // Returns: ["website", "website:awards"]
    /// </code>
    /// </example>
    public static string[] AllAwards() =>
        [
            CacheKeys.WebsiteContext,
            $"{CacheKeys.WebsiteContext}:awards"
        ];

    /// <summary>
    /// Creates a complete tag set for job state cache entries.
    /// </summary>
    /// <param name="jobType">Job type identifier (e.g., "doc-sync").</param>
    /// <param name="target">Job target identifier (e.g., "bylaws", "tournament-rules").</param>
    /// <returns>Array of tags: [context, type, entity]</returns>
    /// <example>
    /// <code>
    /// var tags = CacheTags.Job("doc-sync", "bylaws");
    /// // Returns: ["website", "website:jobs", "website:job:doc-sync:bylaws"]
    /// </code>
    /// </example>
    public static string[] Job(string jobType, string target) =>
        [
            CacheKeys.WebsiteContext,
            $"{CacheKeys.WebsiteContext}:jobs",
            $"{CacheKeys.WebsiteContext}:job:{jobType}:{target}"
        ];

    /// <summary>
    /// Award type constants for standardized award tagging.
    /// </summary>
#pragma warning disable CA1034 // Nested types are intentional for namespace organization
    public static class AwardTypes
#pragma warning restore CA1034
    {
        /// <summary>
        /// Tag identifier for Bowler of the Year awards.
        /// </summary>
        public const string BowlerOfTheYear = "bowler-of-the-year";

        /// <summary>
        /// Tag identifier for High Average awards.
        /// </summary>
        public const string HighAverage = "high-average";

        /// <summary>
        /// Tag identifier for High Block (5-game) awards.
        /// </summary>
        public const string HighBlock = "high-block";
    }

    /// <summary>
    /// Job type constants for standardized job tagging.
    /// </summary>
#pragma warning disable CA1034 // Nested types are intentional for namespace organization
    public static class JobTypes
#pragma warning restore CA1034
    {
        /// <summary>
        /// Tag identifier for document synchronization jobs.
        /// </summary>
        public const string DocumentSync = "doc-sync";
    }
}

