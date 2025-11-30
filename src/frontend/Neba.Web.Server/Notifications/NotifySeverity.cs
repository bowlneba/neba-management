namespace Neba.Web.Server.Notifications;

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
