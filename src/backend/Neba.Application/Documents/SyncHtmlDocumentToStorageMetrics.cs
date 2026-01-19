using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Neba.Application.Documents;

/// <summary>
/// OpenTelemetry metrics for document sync background job operations.
/// </summary>
internal static class SyncHtmlDocumentToStorageMetrics
{
    private const string DocumentKeyTagName = "document.key";

    private static readonly Meter s_meter = new("Neba.BackgroundJobs");

    private static readonly Counter<long> s_jobExecutions = s_meter.CreateCounter<long>(
        "neba.backgroundjob.sync_document.executions",
        description: "Number of document sync job executions");

    private static readonly Counter<long> s_jobSuccesses = s_meter.CreateCounter<long>(
        "neba.backgroundjob.sync_document.successes",
        description: "Number of successful document sync job executions");

    private static readonly Counter<long> s_jobFailures = s_meter.CreateCounter<long>(
        "neba.backgroundjob.sync_document.failures",
        description: "Number of failed document sync job executions");

    private static readonly Histogram<double> s_jobDuration = s_meter.CreateHistogram<double>(
        "neba.backgroundjob.sync_document.duration",
        unit: "ms",
        description: "Duration of document sync job executions");

    private static readonly Histogram<double> s_retrieveDuration = s_meter.CreateHistogram<double>(
        "neba.backgroundjob.sync_document.retrieve.duration",
        unit: "ms",
        description: "Duration of document retrieval phase");

    private static readonly Histogram<double> s_uploadDuration = s_meter.CreateHistogram<double>(
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
        TagList tags = new()
        {
            { DocumentKeyTagName, documentKey },
            { "triggered.by", triggeredBy }
        };
        s_jobExecutions.Add(1, tags);
    }

    /// <summary>
    /// Records a successful job execution.
    /// </summary>
    /// <param name="documentKey">The document key being synced.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static void RecordJobSuccess(string documentKey, double durationMs)
    {
        TagList tags = new() { { DocumentKeyTagName, documentKey } };
        s_jobSuccesses.Add(1, tags);

        TagList durationTags = new()
        {
            { DocumentKeyTagName, documentKey },
            { "result", "success" }
        };
        s_jobDuration.Record(durationMs, durationTags);
    }

    /// <summary>
    /// Records a failed job execution.
    /// </summary>
    /// <param name="documentKey">The document key being synced.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    /// <param name="errorType">Type of error that occurred.</param>
    public static void RecordJobFailure(string documentKey, double durationMs, string errorType)
    {
        TagList failureTags = new()
        {
            { DocumentKeyTagName, documentKey },
            { "error.type", errorType }
        };
        s_jobFailures.Add(1, failureTags);

        TagList durationTags = new()
        {
            { DocumentKeyTagName, documentKey },
            { "result", "failure" },
            { "error.type", errorType }
        };
        s_jobDuration.Record(durationMs, durationTags);
    }

    /// <summary>
    /// Records the duration of the document retrieval phase.
    /// </summary>
    /// <param name="documentKey">The document key being synced.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static void RecordRetrieveDuration(string documentKey, double durationMs)
    {
        TagList tags = new()
        {
            { DocumentKeyTagName, documentKey },
            { "phase", "retrieve" }
        };
        s_retrieveDuration.Record(durationMs, tags);
    }

    /// <summary>
    /// Records the duration of the document upload phase.
    /// </summary>
    /// <param name="documentKey">The document key being synced.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static void RecordUploadDuration(string documentKey, double durationMs)
    {
        TagList tags = new()
        {
            { DocumentKeyTagName, documentKey },
            { "phase", "upload" }
        };
        s_uploadDuration.Record(durationMs, tags);
    }
}
