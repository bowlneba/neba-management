namespace Neba.Web.Server.Notifications;

/// <summary>
/// Represents an alert to be displayed.
/// </summary>
public class AlertItem
{
    /// <summary>
    /// Severity level of the alert.
    /// </summary>
    public NotifySeverity Severity { get; set; }

    /// <summary>
    /// Main message text.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Optional title displayed above the message.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Optional collection of validation messages displayed as a bulleted list.
    /// </summary>
    public IReadOnlyList<string>? ValidationMessages { get; set; }

    /// <summary>
    /// If true, the alert will persist across page navigation and must be manually dismissed.
    /// Defaults to false (alerts auto-clear on navigation).
    /// </summary>
    public bool PersistAcrossNavigation { get; set; }

    /// <summary>
    /// Display and behavior options for this alert.
    /// </summary>
    public required AlertOptions Options { get; set; }
}
