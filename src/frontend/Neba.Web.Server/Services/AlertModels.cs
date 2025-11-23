namespace Neba.Web.Server.Services;

/// <summary>
/// Visual variant for alert rendering.
/// </summary>
public enum AlertVariant
{
    /// <summary>
    /// Solid background with full color (default).
    /// </summary>
    Filled,

    /// <summary>
    /// Border only with transparent background.
    /// </summary>
    Outlined,

    /// <summary>
    /// Compact padding for dense layouts.
    /// </summary>
    Dense
}

/// <summary>
/// Configuration options for alert appearance and behavior.
/// </summary>
public class AlertOptions
{
    /// <summary>
    /// Visual variant for the alert.
    /// </summary>
    public AlertVariant Variant { get; set; } = AlertVariant.Filled;

    /// <summary>
    /// Whether to display the severity icon.
    /// </summary>
    public bool ShowIcon { get; set; } = true;

    /// <summary>
    /// Whether the alert can be dismissed with a close button.
    /// </summary>
    public bool Dismissible { get; set; } = true;

    /// <summary>
    /// Custom handler invoked when the close icon is clicked.
    /// </summary>
    public Func<Task>? OnCloseIconClicked { get; set; }
}

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
    /// Display and behavior options for this alert.
    /// </summary>
    public required AlertOptions Options { get; set; }
}
