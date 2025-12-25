using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Caching;

internal static partial class CachedQueryHandlerDecoratorLogMessages
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Cache miss for key '{CacheKey}', executing query handler")]
    public static partial void LogCacheMiss(
        this ILogger logger,
        string cacheKey);
}
