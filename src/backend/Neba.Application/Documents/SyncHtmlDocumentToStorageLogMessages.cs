using Microsoft.Extensions.Logging;

namespace Neba.Application.Documents;

internal static partial class HtmlDocumentLogMessages
{
    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = "Starting synchronization of HTML document to storage.")]
    public static partial void LogStartingHtmlDocumentSync(this ILogger<SyncHtmlDocumentToStorageJobHandler> logger);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Completed synchronization of HTML document to storage: {Name}")]
    public static partial void LogCompletedHtmlDocumentSync(
        this ILogger<SyncHtmlDocumentToStorageJobHandler> logger,
        string name);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Error occurred during HTML document synchronization to storage.")]
    public static partial void LogErrorDuringHtmlDocumentSync(
        this ILogger<SyncHtmlDocumentToStorageJobHandler> logger,
        Exception ex);
}
