using System.Diagnostics.Metrics;

namespace Neba.Application.Documents;

/// <summary>
/// OpenTelemetry metrics for document sync background job operations.
/// </summary>
internal static class SyncHtmlDocumentToStorageMetrics
{
    private static readonly Meter Meter = new("Neba.BackgroundJobs");

    private static readonly Counter<long> JobExecutions = Meter.CreateCounter<long>(
        "neba.backgroundjob.sync_document.executions",
        description: "Number of document sync job executions");

    private static readonly Counter<long> JobSuccesses = Meter.CreateCounter<long>(
        "neba.backgroundjob.sync_document.successes",
        description: "Number of successful document sync job executions");

    private static readonly Counter<long> JobFailures = Meter.CreateCounter<long>(
        "neba.backgroundjob.sync_document.failures",
        description: "Number of failed document sync job executions");

    private static readonly Histogram<double> JobDuration = Meter.CreateHistogram<double>(
        "neba.backgroundjob.sync_document.duration",
        unit: "ms",
        description: "Duration of document sync job executions");

    private static readonly Histogram<double> RetrieveDuration = Meter.CreateHistogram<double>(
        "neba.backgroundjob.sync_document.retrieve.duration",
        unit: "ms",
        description: "Duration of document retrieval phase");

    private static readonly Histogram<double> UploadDuration = Meter.CreateHistogram<double>(
        "neba.backgroundjob.sync_document.upload.duration",
        unit: "ms",
        description: "Duration of document upload phase");

    /// <summary>
    /// Records the start of a job execution.
    /// </summary>
    /// <param name="documentKey">The document key being synced.</param>
    /// <param name="triggeredBy">Who triggered the sync.</param>
    public static void RecordJobStart(string documentKey, string triggeredBy)
    {
        JobExecutions.Add(1,
            new KeyValuePair<string, object?>("document.key", documentKey),
            new KeyValuePair<string, object?>("triggered.by", triggeredBy));
    }

    /// <summary>
    /// Records a successful job execution.
    /// </summary>
    /// <param name="documentKey">The document key being synced.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static void RecordJobSuccess(string documentKey, double durationMs)
    {
        JobSuccesses.Add(1,
            new KeyValuePair<string, object?>("document.key", documentKey));

        JobDuration.Record(durationMs,
            new KeyValuePair<string, object?>("document.key", documentKey),
            new KeyValuePair<string, object?>("result", "success"));
    }

    /// <summary>
    /// Records a failed job execution.
    /// </summary>
    /// <param name="documentKey">The document key being synced.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    /// <param name="errorType">Type of error that occurred.</param>
    public static void RecordJobFailure(string documentKey, double durationMs, string errorType)
    {
        JobFailures.Add(1,
            new KeyValuePair<string, object?>("document.key", documentKey),
            new KeyValuePair<string, object?>("error.type", errorType));

        JobDuration.Record(durationMs,
            new KeyValuePair<string, object?>("document.key", documentKey),
            new KeyValuePair<string, object?>("result", "failure"),
            new KeyValuePair<string, object?>("error.type", errorType));
    }

    /// <summary>
    /// Records the duration of the document retrieval phase.
    /// </summary>
    /// <param name="documentKey">The document key being synced.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static void RecordRetrieveDuration(string documentKey, double durationMs)
    {
        RetrieveDuration.Record(durationMs,
            new KeyValuePair<string, object?>("document.key", documentKey),
            new KeyValuePair<string, object?>("phase", "retrieve"));
    }

    /// <summary>
    /// Records the duration of the document upload phase.
    /// </summary>
    /// <param name="documentKey">The document key being synced.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static void RecordUploadDuration(string documentKey, double durationMs)
    {
        UploadDuration.Record(durationMs,
            new KeyValuePair<string, object?>("document.key", documentKey),
            new KeyValuePair<string, object?>("phase", "upload"));
    }
}
