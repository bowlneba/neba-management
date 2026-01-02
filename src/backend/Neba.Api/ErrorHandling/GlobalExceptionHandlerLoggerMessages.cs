namespace Neba.Api.ErrorHandling;

#pragma warning disable S1118 // Utility classes should not have public constructors

internal static partial class GlobalExceptionHandlerLoggerMessages
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Error,
        Message = "An unhandled exception occurred.")]
    public static partial void LogException(
        this ILogger<GlobalExceptionHandler> logger,
        Exception exception);
}
