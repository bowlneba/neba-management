using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Documents;

/// <summary>
/// Log messages for document refresh notifications.
/// </summary>
public static partial class DocumentRefreshLogMessages
{
    /// <summary>
    /// Logs that a status update was written to the channel for a document type.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="documentType">The document type (e.g., "bylaws").</param>
    /// <param name="status">The status that was written.</param>
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Wrote status update to channel for {DocumentType}: {Status}"
    )]
    public static partial void LogNotifiedChannel(this ILogger logger, string documentType, string status);
}
