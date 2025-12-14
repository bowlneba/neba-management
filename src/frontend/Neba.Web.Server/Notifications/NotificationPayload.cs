namespace Neba.Web.Server.Notifications;

/// <summary>
/// Represents a notification message with severity, content, and metadata.
/// </summary>
/// <param name="Severity">The severity level of the notification.</param>
/// <param name="Title">Optional title for the notification.</param>
/// <param name="Message">The main message content.</param>
/// <param name="Behavior">How the notification should be delivered (toast, alert, both, or none).</param>
/// <param name="Persist">If true, toast will not auto-dismiss (rare usage).</param>
/// <param name="Code">
/// Optional machine-readable notification code for logging, analytics, or programmatic handling.
/// Example use cases: error tracking systems, business intelligence, conditional UI behavior.
/// Currently not consumed by the notification system itself but available for custom handlers.
/// Example: "ERR_VALIDATION_001", "PAYMENT_SUCCESS", "SESSION_TIMEOUT"
/// </param>
/// <param name="Metadata">
/// Optional metadata for analytics, logging, or custom notification handling.
/// Currently not consumed by the notification system itself but available for:
/// - Analytics/telemetry systems
/// - Error tracking (e.g., stack traces, request IDs)
/// - Business context (e.g., entity IDs, operation types)
/// - Custom notification processors
/// </param>
public record NotificationPayload(
    NotifySeverity Severity,
    string Message,
    string? Title = null,
    NotifyBehavior Behavior = NotifyBehavior.ToastOnly,
    bool Persist = false,
    string? Code = null,
    object? Metadata = null
);
