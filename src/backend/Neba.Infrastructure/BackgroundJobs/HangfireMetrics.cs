using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Neba.Infrastructure.BackgroundJobs;

/// <summary>
/// OpenTelemetry metrics for Hangfire job execution.
/// </summary>
internal static class HangfireMetrics
{
    private const string JobTypeTagName = "job.type";

    private static readonly Meter s_meter = new("Neba.Hangfire");

    private static readonly Counter<long> s_jobExecutions = s_meter.CreateCounter<long>(
        "neba.hangfire.job.executions",
        description: "Number of Hangfire job executions");

    private static readonly Counter<long> s_jobSuccesses = s_meter.CreateCounter<long>(
        "neba.hangfire.job.successes",
        description: "Number of successful Hangfire job executions");

    private static readonly Counter<long> s_jobFailures = s_meter.CreateCounter<long>(
        "neba.hangfire.job.failures",
        description: "Number of failed Hangfire job executions");

    private static readonly Histogram<double> s_jobDuration = s_meter.CreateHistogram<double>(
        "neba.hangfire.job.duration",
        unit: "ms",
        description: "Duration of Hangfire job executions");

    /// <summary>
    /// Records the start of a job execution.
    /// </summary>
    /// <param name="jobType">The type of job being executed.</param>
    public static void RecordJobStart(string jobType)
    {
        TagList tags = new() { { JobTypeTagName, jobType } };
        s_jobExecutions.Add(1, tags);
    }

    /// <summary>
    /// Records a successful job execution.
    /// </summary>
    /// <param name="jobType">The type of job that succeeded.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static void RecordJobSuccess(string jobType, double durationMs)
    {
        TagList tags = new() { { JobTypeTagName, jobType } };
        s_jobSuccesses.Add(1, tags);

        TagList durationTags = new()
        {
            { JobTypeTagName, jobType },
            { "result", "success" }
        };
        s_jobDuration.Record(durationMs, durationTags);
    }

    /// <summary>
    /// Records a failed job execution.
    /// </summary>
    /// <param name="jobType">The type of job that failed.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    /// <param name="errorType">Type of error that occurred.</param>
    public static void RecordJobFailure(string jobType, double durationMs, string errorType)
    {
        TagList failureTags = new()
        {
            { JobTypeTagName, jobType },
            { "error.type", errorType }
        };
        s_jobFailures.Add(1, failureTags);

        TagList durationTags = new()
        {
            { JobTypeTagName, jobType },
            { "result", "failure" },
            { "error.type", errorType }
        };
        s_jobDuration.Record(durationMs, durationTags);
    }
}
