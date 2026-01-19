using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Neba.Web.Server.Telemetry;

namespace Neba.Web.Server.Components;

/// <summary>
/// Base component with built-in telemetry tracking for lifecycle events.
/// Inherit from this to automatically track component initialization and rendering performance.
/// </summary>
public abstract class TelemeteredComponentBase : ComponentBase, IDisposable
{
    private readonly Stopwatch _stopwatch = new();
    private bool _firstRender = true;
    private bool _disposed;

    /// <summary>
    /// Gets the name of the component for telemetry tracking.
    /// Override to provide a custom name, defaults to the type name.
    /// </summary>
    protected virtual string ComponentName => GetType().Name;

    /// <summary>
    /// Gets a value indicating whether lifecycle telemetry is enabled for this component.
    /// Override to disable telemetry for specific components.
    /// </summary>
    protected virtual bool EnableLifecycleTelemetry => true;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        if (EnableLifecycleTelemetry)
        {
            _stopwatch.Restart();
        }

        await base.OnInitializedAsync();
        await OnInitializedCoreAsync();

        if (EnableLifecycleTelemetry)
        {
            _stopwatch.Stop();
            ComponentLifecycleTelemetry.RecordInitialization(
                ComponentName,
                _stopwatch.Elapsed.TotalMilliseconds,
                isAsync: true);
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (EnableLifecycleTelemetry)
        {
            _stopwatch.Restart();
        }

        await base.OnAfterRenderAsync(firstRender);
        await OnAfterRenderCoreAsync(firstRender);

        if (EnableLifecycleTelemetry)
        {
            _stopwatch.Stop();
            ComponentLifecycleTelemetry.RecordRender(
                ComponentName,
                _stopwatch.Elapsed.TotalMilliseconds,
                _firstRender);
            _firstRender = false;
        }
    }

    /// <summary>
    /// Override this method instead of OnInitializedAsync to add component-specific initialization logic.
    /// Telemetry tracking happens automatically around this method.
    /// </summary>
    protected virtual Task OnInitializedCoreAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Override this method instead of OnAfterRenderAsync to add component-specific render logic.
    /// Telemetry tracking happens automatically around this method.
    /// </summary>
    /// <param name="firstRender">Whether this is the first render.</param>
    protected virtual Task OnAfterRenderCoreAsync(bool firstRender)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases resources used by the component.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing && EnableLifecycleTelemetry)
        {
            ComponentLifecycleTelemetry.RecordDisposal(ComponentName);
        }

        _disposed = true;
    }
}
