using System.Reflection;

namespace Neba.Neba.Api.Endpoints;

/// <summary>
/// Provides a marker for the assembly containing the endpoints.
/// </summary>
public static class AssemblyMarker
{
    /// <summary>
    /// Gets the assembly that contains the endpoint declarations.
    /// </summary>
    public static Assembly Assembly
        => typeof(WeatherForecast).Assembly;
}