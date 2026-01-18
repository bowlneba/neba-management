using System.Diagnostics.Metrics;

namespace Neba.Infrastructure.Caching;

/// <summary>
/// OpenTelemetry metrics for cache operations.
/// </summary>
internal static class CacheMetrics
{
    private static readonly Meter Meter = new("Neba.Cache");

    private static readonly Counter<long> CacheHits = Meter.CreateCounter<long>(
        "neba.cache.hits",
        description: "Number of cache hits");

    private static readonly Counter<long> CacheMisses = Meter.CreateCounter<long>(
        "neba.cache.misses",
        description: "Number of cache misses");

    private static readonly Histogram<double> CacheOperationDuration = Meter.CreateHistogram<double>(
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
        CacheHits.Add(1,
            new KeyValuePair<string, object?>("cache.key", cacheKey),
            new KeyValuePair<string, object?>("query.type", queryType),
            new KeyValuePair<string, object?>("cache.hit", true));
    }

    /// <summary>
    /// Records a cache miss.
    /// </summary>
    /// <param name="cacheKey">The cache key that was missed.</param>
    /// <param name="queryType">The type of query being cached.</param>
    public static void RecordCacheMiss(string cacheKey, string queryType)
    {
        CacheMisses.Add(1,
            new KeyValuePair<string, object?>("cache.key", cacheKey),
            new KeyValuePair<string, object?>("query.type", queryType),
            new KeyValuePair<string, object?>("cache.hit", false));
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
        CacheOperationDuration.Record(durationMs,
            new KeyValuePair<string, object?>("cache.key", cacheKey),
            new KeyValuePair<string, object?>("query.type", queryType),
            new KeyValuePair<string, object?>("cache.hit", hit));
    }
}
