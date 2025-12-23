using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.SignalR;

internal static partial class SignalRDocumentRefreshNotifierLogMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "No hub group name was provided for document refresh notification. Notification will not be sent.")]
    public static partial void LogNoHubGroupName(this ILogger<SignalRDocumentRefreshNotifier> logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Hub group '{HubGroupName}' is not registered for document refresh notifications. Notification will not be sent.")]
    public static partial void LogHubGroupNotRegistered(this ILogger<SignalRDocumentRefreshNotifier> logger, string hubGroupName);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "An error occurred while sending document refresh notification to hub group '{HubGroupName}'")]
    public static partial void LogErrorSendingNotification(this ILogger<SignalRDocumentRefreshNotifier> logger, Exception ex, string hubGroupName);
}
