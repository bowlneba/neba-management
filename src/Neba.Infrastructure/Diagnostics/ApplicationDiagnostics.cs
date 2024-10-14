using System.Diagnostics.Metrics;

namespace Neba.Infrastructure.Diagnostics;

/// <summary>
/// Provides diagnostic metrics for the application.
/// </summary>
public static class ApplicationDiagnostics
{
    private const string _serviceName = "Neba.Api";

    /// <summary>
    /// The meter used for creating diagnostic instruments.
    /// </summary>
    public static readonly Meter Meter = new(_serviceName);

    /// <summary>
    /// Gets the counter for tracking the number of weather requests.
    /// </summary>
    public static Counter<long> WeatherRequestCounter
        => Meter.CreateCounter<long>("WeatherRequestCounter", "Number of weather requests");
}