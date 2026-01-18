using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Neba.Web.Server.Telemetry;

/// <summary>
/// Tracks navigation events in the Blazor application for user flow analysis.
/// Register as a scoped service and inject NavigationManager to start tracking.
/// </summary>
public sealed class NavigationTelemetry : IDisposable
{
    private static readonly ActivitySource s_activitySource = new("Neba.Web.Server.Navigation");
    private static readonly Meter s_meter = new("Neba.Web.Server.Navigation");

    private static readonly Counter<long> s_navigationCount = s_meter.CreateCounter<long>(
        "neba.web.server.navigation.count",
        description: "Number of navigations");

    private static readonly Histogram<double> s_navigationDuration = s_meter.CreateHistogram<double>(
        "neba.web.server.navigation.duration",
        unit: "ms",
        description: "Duration of navigation events");

    private readonly NavigationManager _navigationManager;
    private readonly Stopwatch _navigationTimer = new();
    private string? _previousLocation;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationTelemetry"/> class.
    /// </summary>
    /// <param name="navigationManager">The navigation manager to track.</param>
    public NavigationTelemetry(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        _previousLocation = _navigationManager.Uri;
        _navigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _navigationTimer.Stop();
        double durationMs = _navigationTimer.Elapsed.TotalMilliseconds;

        Uri currentUri = new(e.Location);
        Uri? previousUri = _previousLocation != null ? new Uri(_previousLocation) : null;

        string fromPath = previousUri?.AbsolutePath ?? "/";
        string toPath = currentUri.AbsolutePath;
        bool isExternal = !e.IsNavigationIntercepted;

        TagList tags = new()
        {
            { "navigation.from", fromPath },
            { "navigation.to", toPath },
            { "navigation.external", isExternal }
        };

        s_navigationCount.Add(1, tags);

        if (_previousLocation != null && durationMs > 0)
        {
            s_navigationDuration.Record(durationMs, tags);
        }

        using Activity? activity = s_activitySource.StartActivity("navigation");
        activity?.SetTag("navigation.from", fromPath);
        activity?.SetTag("navigation.to", toPath);
        activity?.SetTag("navigation.external", isExternal);
        activity?.SetTag("navigation.duration_ms", durationMs);

        _previousLocation = e.Location;
        _navigationTimer.Restart();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _navigationManager.LocationChanged -= OnLocationChanged;
        _disposed = true;
    }
}
