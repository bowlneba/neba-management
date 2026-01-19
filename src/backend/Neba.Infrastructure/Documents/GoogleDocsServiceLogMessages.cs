using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Documents;

/// <summary>
/// Structured logging messages for Google Docs service operations.
/// </summary>
internal static partial class GoogleDocsServiceLogMessages
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Exporting Google Doc: {DocumentName} (ID: {DocumentId})")]
    public static partial void LogExportingDocument(
        this ILogger logger,
        string documentName,
        string documentId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Document exported successfully: {DocumentName} ({SizeBytes} bytes, {DurationMs}ms)")]
    public static partial void LogDocumentExported(
        this ILogger logger,
        string documentName,
        int sizeBytes,
        double durationMs);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to export document: {DocumentName} (ID: {DocumentId})")]
    public static partial void LogDocumentExportFailed(
        this ILogger logger,
        Exception exception,
        string documentName,
        string documentId);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = "Processing HTML for document: {DocumentName} (original size: {OriginalSize} bytes)")]
    public static partial void LogProcessingHtml(
        this ILogger logger,
        string documentName,
        int originalSize);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = "HTML processed for document: {DocumentName} (processed size: {ProcessedSize} bytes)")]
    public static partial void LogHtmlProcessed(
        this ILogger logger,
        string documentName,
        int processedSize);
}
