using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Caching;

internal static partial class CachedQueryHandlerDecoratorLogMessages
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Cache hit for key '{CacheKey}'")]
    public static partial void LogCacheHit(
        this ILogger logger,
        string cacheKey);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Cache miss for key '{CacheKey}', executing query handler")]
    public static partial void LogCacheMiss(
        this ILogger logger,
        string cacheKey);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Skipping cache for key '{CacheKey}' due to error result")]
    public static partial void LogErrorResultNotCached(
        this ILogger logger,
        string cacheKey);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Unwrapping ErrorOr value for cache storage: key '{CacheKey}'")]
    public static partial void LogUnwrappingErrorOr(
        this ILogger logger,
        string cacheKey);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Rewrapping cached value into ErrorOr: key '{CacheKey}'")]
    public static partial void LogRewrappingErrorOr(
        this ILogger logger,
        string cacheKey);
}
