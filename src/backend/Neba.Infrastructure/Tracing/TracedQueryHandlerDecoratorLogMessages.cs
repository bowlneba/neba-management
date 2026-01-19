using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Tracing;

internal static partial class TracedQueryHandlerDecoratorLogMessages
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Query execution failed: {QueryType} (Duration: {DurationMs}ms)")]
    public static partial void LogQueryExecutionFailed(
        this ILogger logger,
        string queryType,
        double durationMs,
        Exception exception);
}
