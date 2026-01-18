using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Neba.Web.Server.Telemetry;

/// <summary>
/// Tracks loading state and performance metrics for UI operations.
/// </summary>
public static class LoadingStateTelemetry
{
    private static readonly Meter s_meter = new("Neba.Web.Server.LoadingState");

    private static readonly Histogram<double> s_dataLoadDuration = s_meter.CreateHistogram<double>(
        "neba.web.server.data.load.duration",
        unit: "ms",
        description: "Duration of data loading operations");

    private static readonly Histogram<double> s_renderTimeDuration = s_meter.CreateHistogram<double>(
        "neba.web.server.render.time",
        unit: "ms",
        description: "Time to first render for components");

    private static readonly Counter<long> s_loadingStates = s_meter.CreateCounter<long>(
        "neba.web.server.loading.states",
        description: "Number of loading state transitions");

    /// <summary>
    /// Records data loading duration.
    /// </summary>
    /// <param name="operationName">Name of the data loading operation.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    /// <param name="success">Whether the operation succeeded.</param>
    /// <param name="itemCount">Optional number of items loaded.</param>
    public static void RecordDataLoad(string operationName, double durationMs, bool success, int? itemCount = null)
    {
        TagList tags = new()
        {
            { "operation.name", operationName },
            { "operation.success", success }
        };

        if (itemCount.HasValue)
        {
            tags.Add("operation.item_count", itemCount.Value);
        }

        s_dataLoadDuration.Record(durationMs, tags);
        s_loadingStates.Add(1, new TagList { { "state", "loaded" }, { "operation.name", operationName } });
    }

    /// <summary>
    /// Records time to first render.
    /// </summary>
    /// <param name="componentName">Name of the component.</param>
    /// <param name="durationMs">Time from component creation to first render.</param>
    public static void RecordTimeToFirstRender(string componentName, double durationMs)
    {
        TagList tags = new()
        {
            { "component.name", componentName }
        };

        s_renderTimeDuration.Record(durationMs, tags);
    }

    /// <summary>
    /// Records a loading state transition.
    /// </summary>
    /// <param name="operationName">Name of the operation.</param>
    /// <param name="state">Loading state (loading, loaded, error).</param>
    public static void RecordLoadingState(string operationName, string state)
    {
        TagList tags = new()
        {
            { "operation.name", operationName },
            { "state", state }
        };

        s_loadingStates.Add(1, tags);
    }

    /// <summary>
    /// Creates a timer for tracking loading duration.
    /// </summary>
    /// <param name="operationName">Name of the operation.</param>
    /// <returns>A stopwatch that should be stopped when loading completes.</returns>
    public static Stopwatch StartLoadingTimer(string operationName)
    {
        RecordLoadingState(operationName, "loading");
        return Stopwatch.StartNew();
    }

    /// <summary>
    /// Stops a loading timer and records the duration.
    /// </summary>
    /// <param name="operationName">Name of the operation.</param>
    /// <param name="stopwatch">The stopwatch to stop.</param>
    /// <param name="success">Whether the operation succeeded.</param>
    /// <param name="itemCount">Optional number of items loaded.</param>
    public static void StopLoadingTimer(string operationName, Stopwatch stopwatch, bool success, int? itemCount = null)
    {
        ArgumentNullException.ThrowIfNull(stopwatch);
        stopwatch.Stop();
        RecordDataLoad(operationName, stopwatch.Elapsed.TotalMilliseconds, success, itemCount);
    }
}
