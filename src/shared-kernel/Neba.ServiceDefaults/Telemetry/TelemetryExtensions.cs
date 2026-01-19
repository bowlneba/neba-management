using System.Diagnostics;

namespace Neba.ServiceDefaults.Telemetry;

/// <summary>
/// Extension methods for telemetry operations following OpenTelemetry semantic conventions.
/// </summary>
public static class TelemetryExtensions
{
    /// <summary>
    /// Gets the error type name from an exception following OpenTelemetry semantic conventions.
    /// Returns the fully qualified type name for better error classification.
    /// </summary>
    /// <param name="exception">The exception to get the type from.</param>
    /// <returns>The fully qualified type name of the exception.</returns>
    public static string GetErrorType(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.GetType().FullName ?? exception.GetType().Name;
    }

    /// <summary>
    /// Sets common error attributes on an activity following OpenTelemetry semantic conventions.
    /// </summary>
    /// <param name="activity">The activity to set tags on.</param>
    /// <param name="exception">The exception to extract information from.</param>
    /// <returns>The activity for method chaining.</returns>
    public static Activity? SetExceptionTags(this Activity? activity, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (activity is null)
        {
            return null;
        }

        activity.SetTag("error.type", exception.GetErrorType());
        activity.SetTag("exception.message", exception.Message);
        activity.SetTag("exception.stacktrace", exception.StackTrace);
        activity.SetStatus(ActivityStatusCode.Error, exception.Message);

        return activity;
    }

    /// <summary>
    /// Sets code-related attributes on an activity following OpenTelemetry semantic conventions.
    /// </summary>
    /// <param name="activity">The activity to set tags on.</param>
    /// <param name="function">The function name (e.g., method or handler name).</param>
    /// <param name="namespace">The namespace of the code.</param>
    /// <returns>The activity for method chaining.</returns>
    public static Activity? SetCodeAttributes(this Activity? activity, string function, string? @namespace = null)
    {
        if (activity is null)
        {
            return null;
        }

        activity.SetTag("code.function", function);

        if (!string.IsNullOrEmpty(@namespace))
        {
            activity.SetTag("code.namespace", @namespace);
        }

        return activity;
    }
}
