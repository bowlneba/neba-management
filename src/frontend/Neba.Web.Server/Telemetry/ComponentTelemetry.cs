using System.Diagnostics;
using System.Diagnostics.Metrics;
using Neba.ServiceDefaults.Telemetry;

namespace Neba.Web.Server.Telemetry;

/// <summary>
/// Telemetry tracking for Blazor component errors and boundaries.
/// </summary>
internal static class ComponentTelemetry
{
    private static readonly ActivitySource s_activitySource = new("Neba.Web.Server.Components");
    private static readonly Meter s_meter = new("Neba.Web.Server.Components");

    private static readonly Counter<long> s_componentErrors = s_meter.CreateCounter<long>(
        "neba.web.component.errors",
        description: "Number of component errors caught by error boundaries");

    /// <summary>
    /// Records a component error caught by an error boundary.
    /// </summary>
    /// <param name="exception">The exception that was caught.</param>
    /// <param name="componentName">The name of the component where the error occurred.</param>
    /// <param name="routePath">The route path where the error occurred.</param>
    public static void RecordComponentError(Exception exception, string componentName, string? routePath = null)
    {
        using Activity? activity = s_activitySource.StartActivity("component.error");

        activity?.SetCodeAttributes(componentName, "Neba.Web.Server.Components");
        activity?.SetTag("component.name", componentName);

        if (!string.IsNullOrEmpty(routePath))
        {
            activity?.SetTag("route.path", routePath);
        }

        activity?.SetExceptionTags(exception);

        TagList tags = new()
        {
            { "component.name", componentName },
            { "error.type", exception.GetErrorType() }
        };

        if (!string.IsNullOrEmpty(routePath))
        {
            tags.Add("route.path", routePath);
        }

        s_componentErrors.Add(1, tags);
    }
}
