using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Neba.Infrastructure.Documents;

/// <summary>
/// OpenTelemetry metrics for Google Docs service operations.
/// </summary>
internal static class GoogleDocsMetrics
{
    private static readonly Meter s_meter = new("Neba.GoogleDocs");

    private static readonly Histogram<double> s_exportDuration = s_meter.CreateHistogram<double>(
        "neba.google.docs.export.duration",
        unit: "ms",
        description: "Duration of Google Docs export operations");

    private static readonly Counter<long> s_exportSuccess = s_meter.CreateCounter<long>(
        "neba.google.docs.export.success",
        description: "Number of successful Google Docs exports");

    private static readonly Counter<long> s_exportFailure = s_meter.CreateCounter<long>(
        "neba.google.docs.export.failure",
        description: "Number of failed Google Docs exports");

    private static readonly Histogram<long> s_exportSize = s_meter.CreateHistogram<long>(
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

        s_exportSuccess.Add(1, tags);
        s_exportDuration.Record(durationMs, tags);
        s_exportSize.Record(sizeBytes, tags);
    }

    /// <summary>
    /// Records a failed document export.
    /// </summary>
    /// <param name="documentName">The name of the document that failed to export.</param>
    /// <param name="documentId">The Google Docs document ID.</param>
    public static void RecordExportFailure(string documentName, string documentId)
    {
        TagList tags = new()
        {
            { "document.name", documentName },
            { "document.id", documentId },
            { "export.format", "text/html" }
        };
        s_exportFailure.Add(1, tags);
    }
}
