using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Tracing;

internal static partial class TracedCommandHandlerDecoratorLogMessages
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Command execution returned errors: {CommandType} (Duration: {DurationMs}ms, ErrorCount: {ErrorCount})")]
    public static partial void LogCommandExecutionReturnedErrors(
        this ILogger logger,
        string commandType,
        double durationMs,
        int errorCount);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Command execution failed: {CommandType} (Duration: {DurationMs}ms)")]
    public static partial void LogCommandExecutionFailed(
        this ILogger logger,
        string commandType,
        double durationMs,
        Exception exception);
}
