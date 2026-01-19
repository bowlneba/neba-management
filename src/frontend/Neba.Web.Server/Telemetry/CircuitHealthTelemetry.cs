using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Neba.Web.Server.Telemetry;

/// <summary>
/// Tracks Blazor Server SignalR circuit health and lifetime for monitoring connection quality.
/// </summary>
public sealed class CircuitHealthTelemetry : CircuitHandler
{
    private const string CircuitIdKey = "circuit.id";
    private static readonly ActivitySource s_activitySource = new("Neba.Web.Server.SignalR");
    private static readonly Meter s_meter = new("Neba.Web.Server.SignalR");

    private static readonly Counter<long> s_circuitOpened = s_meter.CreateCounter<long>(
        "neba.web.server.signalr.circuit.opened",
        description: "Number of SignalR circuits opened");

    private static readonly Counter<long> s_circuitClosed = s_meter.CreateCounter<long>(
        "neba.web.server.signalr.circuit.closed",
        description: "Number of SignalR circuits closed");

    private static readonly Histogram<double> s_circuitDuration = s_meter.CreateHistogram<double>(
        "neba.web.server.signalr.circuit.duration",
        unit: "s",
        description: "Duration of SignalR circuit lifetime");

    private static readonly Counter<long> s_connectionErrors = s_meter.CreateCounter<long>(
        "neba.web.server.signalr.connection.errors",
        description: "Number of SignalR connection errors");

    private static readonly UpDownCounter<long> s_activeCircuits = s_meter.CreateUpDownCounter<long>(
        "neba.web.server.signalr.circuits.active",
        description: "Number of currently active SignalR circuits");

    private readonly Dictionary<string, Stopwatch> _circuitTimers = [];

    /// <summary>
    /// Called when a new circuit is opened.
    /// </summary>
    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(circuit);

        string circuitId = circuit.Id;

        using Activity? activity = s_activitySource.StartActivity("circuit.opened");
        activity?.SetTag(CircuitIdKey, circuitId);

        TagList tags = new() { { CircuitIdKey, circuitId } };
        s_circuitOpened.Add(1, tags);
        s_activeCircuits.Add(1);

        _circuitTimers[circuitId] = Stopwatch.StartNew();

        await base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    /// <summary>
    /// Called when a circuit is closed.
    /// </summary>
    public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(circuit);

        string circuitId = circuit.Id;

        using Activity? activity = s_activitySource.StartActivity("circuit.closed");
        activity?.SetTag(CircuitIdKey, circuitId);

        TagList tags = new() { { CircuitIdKey, circuitId } };
        s_circuitClosed.Add(1, tags);
        s_activeCircuits.Add(-1);

        if (_circuitTimers.TryGetValue(circuitId, out Stopwatch? timer))
        {
            timer.Stop();
            double durationSeconds = timer.Elapsed.TotalSeconds;
            s_circuitDuration.Record(durationSeconds, tags);
            _circuitTimers.Remove(circuitId);

            activity?.SetTag("circuit.duration_seconds", durationSeconds);
        }

        await base.OnCircuitClosedAsync(circuit, cancellationToken);
    }

    /// <summary>
    /// Called when a connection is broken.
    /// </summary>
    public override async Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(circuit);

        string circuitId = circuit.Id;

        using Activity? activity = s_activitySource.StartActivity("circuit.connection_down");
        activity?.SetTag(CircuitIdKey, circuitId);

        TagList tags = new() { { CircuitIdKey, circuitId }, { "error.type", "connection_down" } };
        s_connectionErrors.Add(1, tags);

        await base.OnConnectionDownAsync(circuit, cancellationToken);
    }

    /// <summary>
    /// Called when a connection is restored.
    /// </summary>
    public override async Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(circuit);

        string circuitId = circuit.Id;

        using Activity? activity = s_activitySource.StartActivity("circuit.connection_up");
        activity?.SetTag(CircuitIdKey, circuitId);

        await base.OnConnectionUpAsync(circuit, cancellationToken);
    }
}
