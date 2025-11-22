namespace Neba.Web.Server.Services;

/// <summary>
/// Defines the severity level for a notification.
/// </summary>
public enum NotifySeverity
{
    /// <summary>
    /// Normal notification (gray theme).
    /// </summary>
    Normal,

    /// <summary>
    /// Informational notification (blue theme).
    /// </summary>
    Info,

    /// <summary>
    /// Success notification (green theme).
    /// </summary>
    Success,

    /// <summary>
    /// Warning notification (goldenrod/orange theme).
    /// </summary>
    Warning,

    /// <summary>
    /// Error notification (red theme).
    /// </summary>
    Error
}

/// <summary>
/// Defines how a notification should be delivered to the user.
/// </summary>
public enum NotifyBehavior
{
    /// <summary>
    /// Show only as an inline alert (persistent).
    /// </summary>
    AlertOnly,

    /// <summary>
    /// Show only as a toast popup (ephemeral).
    /// </summary>
    ToastOnly,

    /// <summary>
    /// Show both alert and toast (default for form validation failures).
    /// </summary>
    AlertAndToast,

    /// <summary>
    /// Do not show any notification.
    /// </summary>
    None
}

/// <summary>
/// Defines the position where toast notifications appear on the screen.
/// </summary>
public enum ToastPosition
{
    /// <summary>
    /// Top-right corner (default).
    /// </summary>
    TopRight,

    /// <summary>
    /// Top-center of the screen.
    /// </summary>
    TopCenter,

    /// <summary>
    /// Bottom-right corner.
    /// </summary>
    BottomRight,

    /// <summary>
    /// Bottom-center of the screen.
    /// </summary>
    BottomCenter
}

/// <summary>
/// Represents a notification message with severity, content, and metadata.
/// </summary>
/// <param name="Severity">The severity level of the notification.</param>
/// <param name="Title">Optional title for the notification.</param>
/// <param name="Message">The main message content.</param>
/// <param name="Persist">If true, toast will not auto-dismiss (rare usage).</param>
/// <param name="Code">Optional machine-readable error/notification code (e.g., "ERR_VALIDATION_001").</param>
/// <param name="Metadata">Optional metadata for analytics or custom handling.</param>
public record NotificationPayload(
    NotifySeverity Severity,
    string Message,
    string? Title = null,
    bool Persist = false,
    string? Code = null,
    object? Metadata = null
);
