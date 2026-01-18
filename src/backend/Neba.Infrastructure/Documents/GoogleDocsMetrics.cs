using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Neba.Infrastructure.Documents;

/// <summary>
/// OpenTelemetry metrics for Google Docs service operations.
/// </summary>
internal static class GoogleDocsMetrics
{
    private static readonly Meter Meter = new("Neba.GoogleDocs");

    private static readonly Histogram<double> ExportDuration = Meter.CreateHistogram<double>(
        "neba.google.docs.export.duration",
        unit: "ms",
        description: "Duration of Google Docs export operations");

    private static readonly Counter<long> ExportSuccess = Meter.CreateCounter<long>(
        "neba.google.docs.export.success",
        description: "Number of successful Google Docs exports");

    private static readonly Counter<long> ExportFailure = Meter.CreateCounter<long>(
        "neba.google.docs.export.failure",
        description: "Number of failed Google Docs exports");

    private static readonly Histogram<long> ExportSize = Meter.CreateHistogram<long>(
        "neba.google.docs.export.size",
        unit: "bytes",
        description: "Size of exported Google Docs HTML");

    /// <summary>
    /// Records a successful document export.
    /// </summary>
    /// <param name="documentName">The name of the exported document.</param>
    /// <param name="documentId">The Google Docs document ID.</param>
    /// <param name="durationMs">Duration of the export operation in milliseconds.</param>
    /// <param name="sizeBytes">Size of the exported HTML in bytes.</param>
    public static void RecordExportSuccess(string documentName, string documentId, double durationMs, long sizeBytes)
    {
        TagList tags = new()
        {
            { "document.name", documentName },
            { "document.id", documentId },
            { "export.format", "text/html" }
        };

        ExportSuccess.Add(1, tags);
        ExportDuration.Record(durationMs, tags);
        ExportSize.Record(sizeBytes, tags);
    }

    /// <summary>
    /// Records a failed document export.
    /// </summary>
    /// <param name="documentName">The name of the document that failed to export.</param>
    /// <param name="documentId">The Google Docs document ID.</param>
    public static void RecordExportFailure(string documentName, string documentId)
    {
        ExportFailure.Add(1,
            new KeyValuePair<string, object?>("document.name", documentName),
            new KeyValuePair<string, object?>("document.id", documentId),
            new KeyValuePair<string, object?>("export.format", "text/html"));
    }
}
