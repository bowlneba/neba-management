using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Neba.Web.Server.Telemetry;

/// <summary>
/// Tracks Blazor component lifecycle events for performance monitoring.
/// </summary>
public static class ComponentLifecycleTelemetry
{
    private const string ComponentNameTag = "component.name";

    private static readonly ActivitySource s_activitySource = new("Neba.Web.Server.ComponentLifecycle");
    private static readonly Meter s_meter = new("Neba.Web.Server.ComponentLifecycle");

    private static readonly Counter<long> s_componentInitializations = s_meter.CreateCounter<long>(
        "neba.web.server.component.initializations",
        description: "Number of component initializations");

    private static readonly Histogram<double> s_initializationDuration = s_meter.CreateHistogram<double>(
        "neba.web.server.component.initialization.duration",
        unit: "ms",
        description: "Duration of component initialization (OnInitializedAsync)");

    private static readonly Histogram<double> s_renderDuration = s_meter.CreateHistogram<double>(
        "neba.web.server.component.render.duration",
        unit: "ms",
        description: "Duration of component rendering (OnAfterRenderAsync)");

    private static readonly Counter<long> s_componentDisposals = s_meter.CreateCounter<long>(
        "neba.web.server.component.disposals",
        description: "Number of component disposals");

    /// <summary>
    /// Records component initialization timing.
    /// </summary>
    /// <param name="componentName">Name of the component being initialized.</param>
    /// <param name="durationMs">Duration of initialization in milliseconds.</param>
    /// <param name="isAsync">Whether async initialization was used.</param>
    public static void RecordInitialization(string componentName, double durationMs, bool isAsync = true)
    {
        TagList tags = new()
        {
            { ComponentNameTag, componentName },
            { "component.init.async", isAsync }
        };

        s_componentInitializations.Add(1, tags);
        s_initializationDuration.Record(durationMs, tags);
    }

    /// <summary>
    /// Records component render timing.
    /// </summary>
    /// <param name="componentName">Name of the component being rendered.</param>
    /// <param name="durationMs">Duration of rendering in milliseconds.</param>
    /// <param name="firstRender">Whether this is the first render.</param>
    public static void RecordRender(string componentName, double durationMs, bool firstRender)
    {
        TagList tags = new()
        {
            { ComponentNameTag, componentName },
            { "component.first_render", firstRender }
        };

        s_renderDuration.Record(durationMs, tags);
    }

    /// <summary>
    /// Records component disposal.
    /// </summary>
    /// <param name="componentName">Name of the component being disposed.</param>
    public static void RecordDisposal(string componentName)
    {
        TagList tags = new()
        {
            { ComponentNameTag, componentName }
        };

        s_componentDisposals.Add(1, tags);
    }

    /// <summary>
    /// Starts an activity for component lifecycle tracking.
    /// </summary>
    /// <param name="componentName">Name of the component.</param>
    /// <param name="lifecycleEvent">Lifecycle event (Initialize, Render, Dispose).</param>
    /// <returns>Activity for distributed tracing.</returns>
    public static Activity? StartActivity(string componentName, string lifecycleEvent)
    {
        Activity? activity = s_activitySource.StartActivity($"component.{lifecycleEvent}");
        activity?.SetTag(ComponentNameTag, componentName);
        activity?.SetTag("component.lifecycle.event", lifecycleEvent);
        return activity;
    }
}
