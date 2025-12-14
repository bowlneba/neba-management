namespace Neba.Web.Server.Notifications;

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
