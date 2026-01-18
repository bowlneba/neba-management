using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Neba.Infrastructure.Documents;

/// <summary>
/// Telemetry tracking for Server-Sent Events (SSE) streams.
/// </summary>
internal static class SseStreamTelemetry
{
    private static readonly ActivitySource s_activitySource = new("Neba.Infrastructure.SSE");
    private static readonly Meter s_meter = new("Neba.Infrastructure.SSE");

    private static readonly Counter<long> s_connectionCount = s_meter.CreateCounter<long>(
        "neba.sse.connection.count",
        description: "Number of SSE connections opened");

    private static readonly UpDownCounter<long> s_activeConnections = s_meter.CreateUpDownCounter<long>(
        "neba.sse.connections.active",
        description: "Number of currently active SSE connections");

    private static readonly Histogram<double> s_connectionDuration = s_meter.CreateHistogram<double>(
        "neba.sse.connection.duration",
        unit: "s",
        description: "Duration of SSE connections");

    private static readonly Counter<long> s_eventsPublished = s_meter.CreateCounter<long>(
        "neba.sse.events.published",
        description: "Number of SSE events published to clients");

    private static readonly Counter<long> s_connectionErrors = s_meter.CreateCounter<long>(
        "neba.sse.connection.errors",
        description: "Number of SSE connection errors");

    /// <summary>
    /// Records a new SSE connection.
    /// </summary>
    /// <param name="streamType">Type of resource being streamed (e.g., "document", "notification", "status").</param>
    /// <returns>Stopwatch to track connection duration.</returns>
    public static Stopwatch RecordConnectionStart(string streamType)
    {
        TagList tags = new() { { "stream.type", streamType } };

        s_connectionCount.Add(1, tags);
        s_activeConnections.Add(1, tags);

        using Activity? activity = s_activitySource.StartActivity("sse.connection");
        activity?.SetTag("stream.type", streamType);

        return Stopwatch.StartNew();
    }

    /// <summary>
    /// Records SSE connection closure.
    /// </summary>
    /// <param name="streamType">Type of resource being streamed.</param>
    /// <param name="timer">Stopwatch tracking connection duration.</param>
    /// <param name="eventCount">Number of events sent during the connection.</param>
    public static void RecordConnectionEnd(string streamType, Stopwatch timer, int eventCount)
    {
        timer.Stop();
        double durationSeconds = timer.Elapsed.TotalSeconds;

        TagList tags = new()
        {
            { "stream.type", streamType },
            { "event.count", eventCount }
        };

        s_activeConnections.Add(-1, new TagList { { "stream.type", streamType } });
        s_connectionDuration.Record(durationSeconds, tags);

        using Activity? activity = s_activitySource.StartActivity("sse.connection.closed");
        activity?.SetTag("stream.type", streamType);
        activity?.SetTag("duration_seconds", durationSeconds);
        activity?.SetTag("event.count", eventCount);
    }

    /// <summary>
    /// Records an SSE event being published.
    /// </summary>
    /// <param name="streamType">Type of resource being streamed.</param>
    /// <param name="eventType">Type of event being published.</param>
    public static void RecordEventPublished(string streamType, string eventType)
    {
        TagList tags = new()
        {
            { "stream.type", streamType },
            { "event.type", eventType }
        };

        s_eventsPublished.Add(1, tags);
    }

    /// <summary>
    /// Records an SSE connection error.
    /// </summary>
    /// <param name="streamType">Type of resource being streamed.</param>
    /// <param name="errorType">Type of error encountered.</param>
    public static void RecordConnectionError(string streamType, string errorType)
    {
        TagList tags = new()
        {
            { "stream.type", streamType },
            { "error.type", errorType }
        };

        s_connectionErrors.Add(1, tags);
        s_activeConnections.Add(-1, new TagList { { "stream.type", streamType } });

        using Activity? activity = s_activitySource.StartActivity("sse.connection.error");
        activity?.SetTag("stream.type", streamType);
        activity?.SetTag("error.type", errorType);
        activity?.SetStatus(ActivityStatusCode.Error, errorType);
    }
}
