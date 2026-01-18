using System.Diagnostics.Metrics;

namespace Neba.Infrastructure.BackgroundJobs;

/// <summary>
/// OpenTelemetry metrics for Hangfire job execution.
/// </summary>
internal static class HangfireMetrics
{
    private static readonly Meter Meter = new("Neba.Hangfire");

    private static readonly Counter<long> JobExecutions = Meter.CreateCounter<long>(
        "neba.hangfire.job.executions",
        description: "Number of Hangfire job executions");

    private static readonly Counter<long> JobSuccesses = Meter.CreateCounter<long>(
        "neba.hangfire.job.successes",
        description: "Number of successful Hangfire job executions");

    private static readonly Counter<long> JobFailures = Meter.CreateCounter<long>(
        "neba.hangfire.job.failures",
        description: "Number of failed Hangfire job executions");

    private static readonly Histogram<double> JobDuration = Meter.CreateHistogram<double>(
        "neba.hangfire.job.duration",
        unit: "ms",
        description: "Duration of Hangfire job executions");

    /// <summary>
    /// Records the start of a job execution.
    /// </summary>
    /// <param name="jobType">The type of job being executed.</param>
    public static void RecordJobStart(string jobType)
    {
        JobExecutions.Add(1,
            new KeyValuePair<string, object?>("job.type", jobType));
    }

    /// <summary>
    /// Records a successful job execution.
    /// </summary>
    /// <param name="jobType">The type of job that succeeded.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static void RecordJobSuccess(string jobType, double durationMs)
    {
        JobSuccesses.Add(1,
            new KeyValuePair<string, object?>("job.type", jobType));

        JobDuration.Record(durationMs,
            new KeyValuePair<string, object?>("job.type", jobType),
            new KeyValuePair<string, object?>("result", "success"));
    }

    /// <summary>
    /// Records a failed job execution.
    /// </summary>
    /// <param name="jobType">The type of job that failed.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    /// <param name="errorType">Type of error that occurred.</param>
    public static void RecordJobFailure(string jobType, double durationMs, string errorType)
    {
        JobFailures.Add(1,
            new KeyValuePair<string, object?>("job.type", jobType),
            new KeyValuePair<string, object?>("error.type", errorType));

        JobDuration.Record(durationMs,
            new KeyValuePair<string, object?>("job.type", jobType),
            new KeyValuePair<string, object?>("result", "failure"),
            new KeyValuePair<string, object?>("error.type", errorType));
    }
}
