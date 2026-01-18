using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Database;

/// <summary>
/// Structured logging messages for slow query detection.
/// </summary>
internal static partial class SlowQueryInterceptorLogMessages
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Slow query detected: {CommandText} took {DurationMs}ms (threshold: {ThresholdMs}ms)")]
    public static partial void LogSlowQueryDetected(
        this ILogger logger,
        string commandText,
        double durationMs,
        double thresholdMs);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Query executed: {CommandType} took {DurationMs}ms")]
    public static partial void LogQueryExecuted(
        this ILogger logger,
        string commandType,
        double durationMs);
}
