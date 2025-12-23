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
        Message = "Completed synchronization of HTML document to storage: {DocumentLocation}")]
    public static partial void LogCompletedHtmlDocumentSync(
        this ILogger<SyncHtmlDocumentToStorageJobHandler> logger,
        string documentLocation);
}
