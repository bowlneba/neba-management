using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Neba.Web.Server.Telemetry;

/// <summary>
/// Telemetry tracking for JavaScript interop and user interactions.
/// This service provides a bridge between JavaScript and .NET telemetry.
/// </summary>
public sealed class JavaScriptTelemetryBridge
{
    private static readonly ActivitySource s_activitySource = new("Neba.Web.Server.JavaScript");
    private static readonly Meter s_meter = new("Neba.Web.Server.JavaScript");

    private static readonly Counter<long> s_interactionCount = s_meter.CreateCounter<long>(
        "neba.web.server.javascript.interactions",
        description: "Number of tracked JavaScript interactions");

    private static readonly Histogram<double> s_operationDuration = s_meter.CreateHistogram<double>(
        "neba.web.server.javascript.operation.duration",
        unit: "ms",
        description: "Duration of JavaScript operations");

    /// <summary>
    /// Tracks a JavaScript event from the client.
    /// Called via JSInterop from JavaScript code.
    /// </summary>
    /// <param name="eventName">Name of the event (e.g., "map.route_calculated")</param>
    /// <param name="properties">Dictionary of event properties</param>
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    public void TrackEvent(string eventName, Dictionary<string, object>? properties = null)
    {
        using Activity? activity = s_activitySource.StartActivity($"javascript.{eventName}");

        activity?.SetTag("event.name", eventName);
        activity?.SetTag("event.source", "javascript");

        TagList tags = new() { { "event.name", eventName } };

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                activity?.SetTag($"event.{prop.Key}", prop.Value);

                // Track duration if present
                if (prop.Key == "duration_ms" && prop.Value is double durationMs)
                {
                    TagList durationTags = new()
                    {
                        { "event.name", eventName },
                        { "event.success", properties.TryGetValue("success", out object? successValue) && successValue is bool success && success }
                    };
                    s_operationDuration.Record(durationMs, durationTags);
                }
            }
        }

        s_interactionCount.Add(1, tags);
    }

    /// <summary>
    /// Tracks a JavaScript error from the client.
    /// Called via JSInterop from JavaScript code.
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="source">Source of the error (e.g., "map.route")</param>
    /// <param name="stackTrace">Optional stack trace</param>
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    public void TrackError(string errorMessage, string source, string? stackTrace = null)
    {
        using Activity? activity = s_activitySource.StartActivity("javascript.error");

        activity?.SetTag("error.message", errorMessage);
        activity?.SetTag("error.source", source);
        activity?.SetTag("event.source", "javascript");

        if (!string.IsNullOrEmpty(stackTrace))
        {
            activity?.SetTag("error.stacktrace", stackTrace);
        }

        activity?.SetStatus(ActivityStatusCode.Error, errorMessage);

        TagList tags = new()
        {
            { "error.source", source },
            { "error.type", "JavaScriptError" }
        };

        s_interactionCount.Add(1, tags);
    }
}
