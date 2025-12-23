using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Documents;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Logger message definitions for document refresh SSE operations.
/// </summary>
public static partial class DocumentRefreshSseLogMessages
{
    // DocumentRefreshChannelManager logging
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Creating new SSE channel for document type: {DocumentType}"
    )]
    public static partial void LogCreatingChannel(this ILogger<DocumentRefreshChannelManager> logger, string documentType);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Wrote status update to channel for {DocumentType}: {Status}"
    )]
    public static partial void LogWroteStatusUpdate(this ILogger<DocumentRefreshChannelManager> logger, string documentType, string status);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "No active channel for {DocumentType}, status update dropped: {Status}"
    )]
    public static partial void LogNoActiveChannel(this ILogger<DocumentRefreshChannelManager> logger, string documentType, string status);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "No listeners remaining for {DocumentType}, scheduling cleanup"
    )]
    public static partial void LogSchedulingCleanup(this ILogger<DocumentRefreshChannelManager> logger, string documentType);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Removed channel for {DocumentType} (no listeners)"
    )]
    public static partial void LogRemovedChannel(this ILogger<DocumentRefreshChannelManager> logger, string documentType);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Channel for {DocumentType} exceeded maximum lifetime (5 min), removing"
    )]
    public static partial void LogChannelMaxLifetimeExceeded(this ILogger<DocumentRefreshChannelManager> logger, string documentType);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Channel for {DocumentType} exceeded idle timeout (30s), removing"
    )]
    public static partial void LogChannelIdleTimeoutExceeded(this ILogger<DocumentRefreshChannelManager> logger, string documentType);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Error monitoring channel timeout for {DocumentType}"
    )]
    public static partial void LogChannelMonitoringError(this ILogger<DocumentRefreshChannelManager> logger, Exception ex, string documentType);

    // DocumentRefreshSseEndpoints logging
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "SSE client connected for document type: {DocumentType}"
    )]
    public static partial void LogClientConnected(this ILogger logger, string documentType);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "SSE client disconnected for document type: {DocumentType}"
    )]
    public static partial void LogClientDisconnected(this ILogger logger, Exception? ex, string documentType);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Error streaming SSE for document type: {DocumentType}"
    )]
    public static partial void LogStreamError(this ILogger logger, Exception ex, string documentType);

    // SseDocumentRefreshNotifier logging
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Notified SSE channel for {DocumentType} with status {Status}"
    )]
    public static partial void LogNotifiedChannel(this ILogger logger, string documentType, string status);
}
