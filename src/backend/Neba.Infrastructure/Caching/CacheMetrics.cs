using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Neba.Infrastructure.Caching;

/// <summary>
/// OpenTelemetry metrics for cache operations.
/// </summary>
internal static class CacheMetrics
{
    private static readonly Meter s_meter = new("Neba.Cache");

    private static readonly Counter<long> s_cacheHits = s_meter.CreateCounter<long>(
        "neba.cache.hits",
        description: "Number of cache hits");

    private static readonly Counter<long> s_cacheMisses = s_meter.CreateCounter<long>(
        "neba.cache.misses",
        description: "Number of cache misses");

    private static readonly Histogram<double> s_cacheOperationDuration = s_meter.CreateHistogram<double>(
        "neba.cache.operation.duration",
        unit: "ms",
        description: "Duration of cache operations");

    /// <summary>
    /// Records a cache hit.
    /// </summary>
    /// <param name="cacheKey">The cache key that was hit.</param>
    /// <param name="queryType">The type of query being cached.</param>
    public static void RecordCacheHit(string cacheKey, string queryType)
    {
        TagList tags = new()
        {
            { "cache.key", cacheKey },
            { "query.type", queryType },
            { "cache.hit", true }
        };
        s_cacheHits.Add(1, tags);
    }

    /// <summary>
    /// Records a cache miss.
    /// </summary>
    /// <param name="cacheKey">The cache key that was missed.</param>
    /// <param name="queryType">The type of query being cached.</param>
    public static void RecordCacheMiss(string cacheKey, string queryType)
    {
        TagList tags = new()
        {
            { "cache.key", cacheKey },
            { "query.type", queryType },
            { "cache.hit", false }
        };
        s_cacheMisses.Add(1, tags);
    }

    /// <summary>
    /// Records the duration of a cache operation.
    /// </summary>
    /// <param name="durationMs">Duration in milliseconds.</param>
    /// <param name="cacheKey">The cache key involved.</param>
    /// <param name="queryType">The type of query being cached.</param>
    /// <param name="hit">Whether the operation was a cache hit.</param>
    public static void RecordOperationDuration(double durationMs, string cacheKey, string queryType, bool hit)
    {
        TagList tags = new()
        {
            { "cache.key", cacheKey },
            { "query.type", queryType },
            { "cache.hit", hit }
        };
        s_cacheOperationDuration.Record(durationMs, tags);
    }
}
