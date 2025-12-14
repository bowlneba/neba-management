
namespace Neba.Web.Server.Notifications;

/// <summary>
/// Service for managing and displaying alerts in the UI.
/// </summary>
/// <remarks>
/// Thread safety: This service is scoped per Blazor Server circuit and relies on
/// Blazor's single-threaded synchronization context. All state mutations occur
/// on the circuit's synchronization context. Not suitable for multi-threaded use.
/// </remarks>
public class AlertService
{
    private AlertItem? _currentAlert;

    /// <summary>
    /// The currently active alert, or null if no alert is displayed.
    /// </summary>
    public AlertItem? CurrentAlert
    {
        get => _currentAlert;
        private set
        {
            _currentAlert = value;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Event triggered when the alert state changes.
    /// </summary>
    public event EventHandler? OnChange;

    /// <summary>
    /// Shows a normal severity alert.
    /// </summary>
    /// <param name="message">The alert message.</param>
    /// <param name="title">Optional title displayed above the message.</param>
    /// <param name="configure">Optional configuration action for alert options.</param>
    /// <param name="persistAcrossNavigation">If true, alert persists across navigation. Defaults to false.</param>
    public void ShowNormal(string message, string? title = null, Action<AlertOptions>? configure = null, bool persistAcrossNavigation = false)
    {
        Show(NotifySeverity.Normal, message, title, configure, persistAcrossNavigation);
    }

    /// <summary>
    /// Shows an informational alert.
    /// </summary>
    /// <param name="message">The alert message.</param>
    /// <param name="title">Optional title displayed above the message.</param>
    /// <param name="configure">Optional configuration action for alert options.</param>
    /// <param name="persistAcrossNavigation">If true, alert persists across navigation. Defaults to false.</param>
    public void ShowInfo(string message, string? title = null, Action<AlertOptions>? configure = null, bool persistAcrossNavigation = false)
    {
        Show(NotifySeverity.Info, message, title, configure, persistAcrossNavigation);
    }

    /// <summary>
    /// Shows a success alert.
    /// </summary>
    /// <param name="message">The alert message.</param>
    /// <param name="title">Optional title displayed above the message.</param>
    /// <param name="configure">Optional configuration action for alert options.</param>
    /// <param name="persistAcrossNavigation">If true, alert persists across navigation. Defaults to false.</param>
    public void ShowSuccess(string message, string? title = null, Action<AlertOptions>? configure = null, bool persistAcrossNavigation = false)
    {
        Show(NotifySeverity.Success, message, title, configure, persistAcrossNavigation);
    }

    /// <summary>
    /// Shows a warning alert.
    /// </summary>
    /// <param name="message">The alert message.</param>
    /// <param name="title">Optional title displayed above the message.</param>
    /// <param name="configure">Optional configuration action for alert options.</param>
    /// <param name="persistAcrossNavigation">If true, alert persists across navigation. Defaults to false.</param>
    public void ShowWarning(string message, string? title = null, Action<AlertOptions>? configure = null, bool persistAcrossNavigation = false)
    {
        Show(NotifySeverity.Warning, message, title, configure, persistAcrossNavigation);
    }

    /// <summary>
    /// Shows an error alert.
    /// </summary>
    /// <param name="message">The alert message.</param>
    /// <param name="title">Optional title displayed above the message.</param>
    /// <param name="configure">Optional configuration action for alert options.</param>
    /// <param name="persistAcrossNavigation">If true, alert persists across navigation. Defaults to false.</param>
    public void ShowError(string message, string? title = null, Action<AlertOptions>? configure = null, bool persistAcrossNavigation = false)
    {
        Show(NotifySeverity.Error, message, title, configure, persistAcrossNavigation);
    }

    /// <summary>
    /// Shows a validation alert with a collection of validation messages.
    /// </summary>
    /// <param name="messages">The validation error messages to display as a bulleted list.</param>
    /// <param name="title">Optional title displayed above the messages. Defaults to "Please fix the following:"</param>
    public void ShowValidation(IReadOnlyList<string> messages, string? title = "Please fix the following:")
    {
        var options = new AlertOptions();
        CurrentAlert = new AlertItem
        {
            Severity = NotifySeverity.Error,
            Message = string.Empty,
            Title = title,
            ValidationMessages = messages,
            Options = options
        };
    }

    /// <summary>
    /// Shows a validation alert with a collection of validation messages.
    /// </summary>
    /// <param name="title">Title displayed above the messages.</param>
    /// <param name="messages">The validation error messages to display as a bulleted list.</param>
    public void ShowValidation(string title, IReadOnlyList<string> messages)
    {
        ShowValidation(messages, title);
    }

    /// <summary>
    /// Clears the currently displayed alert.
    /// </summary>
    public void Clear()
    {
        CurrentAlert = null;
    }

    /// <summary>
    /// Internal method to show an alert with the specified severity.
    /// </summary>
    private void Show(NotifySeverity severity, string message, string? title, Action<AlertOptions>? configure, bool persistAcrossNavigation = false)
    {
        var options = new AlertOptions();
        configure?.Invoke(options);

        CurrentAlert = new AlertItem
        {
            Severity = severity,
            Message = message,
            Title = title,
            PersistAcrossNavigation = persistAcrossNavigation,
            Options = options
        };
    }

    /// <summary>
    /// Notifies subscribers that the alert state has changed.
    /// </summary>
    private void NotifyStateChanged()
    {
        OnChange?.Invoke(this, EventArgs.Empty);
    }
}
