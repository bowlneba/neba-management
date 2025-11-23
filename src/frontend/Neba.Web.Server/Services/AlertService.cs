using Microsoft.AspNetCore.Components;

namespace Neba.Web.Server.Services;

/// <summary>
/// Service for managing and displaying alerts in the UI.
/// Scoped per Blazor circuit.
/// </summary>
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
    public void ShowNormal(string message, string? title = null, Action<AlertOptions>? configure = null)
    {
        Show(NotifySeverity.Normal, message, title, configure);
    }

    /// <summary>
    /// Shows an informational alert.
    /// </summary>
    /// <param name="message">The alert message.</param>
    /// <param name="title">Optional title displayed above the message.</param>
    /// <param name="configure">Optional configuration action for alert options.</param>
    public void ShowInfo(string message, string? title = null, Action<AlertOptions>? configure = null)
    {
        Show(NotifySeverity.Info, message, title, configure);
    }

    /// <summary>
    /// Shows a success alert.
    /// </summary>
    /// <param name="message">The alert message.</param>
    /// <param name="title">Optional title displayed above the message.</param>
    /// <param name="configure">Optional configuration action for alert options.</param>
    public void ShowSuccess(string message, string? title = null, Action<AlertOptions>? configure = null)
    {
        Show(NotifySeverity.Success, message, title, configure);
    }

    /// <summary>
    /// Shows a warning alert.
    /// </summary>
    /// <param name="message">The alert message.</param>
    /// <param name="title">Optional title displayed above the message.</param>
    /// <param name="configure">Optional configuration action for alert options.</param>
    public void ShowWarning(string message, string? title = null, Action<AlertOptions>? configure = null)
    {
        Show(NotifySeverity.Warning, message, title, configure);
    }

    /// <summary>
    /// Shows an error alert.
    /// </summary>
    /// <param name="message">The alert message.</param>
    /// <param name="title">Optional title displayed above the message.</param>
    /// <param name="configure">Optional configuration action for alert options.</param>
    public void ShowError(string message, string? title = null, Action<AlertOptions>? configure = null)
    {
        Show(NotifySeverity.Error, message, title, configure);
    }

    /// <summary>
    /// Shows a validation alert with a collection of validation messages.
    /// </summary>
    /// <param name="messages">The validation error messages to display as a bulleted list.</param>
    /// <param name="title">Optional title displayed above the messages. Defaults to "Please fix the following:"</param>
    public void ShowValidation(string[] messages, string? title = "Please fix the following:")
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
    public void ShowValidation(string title, string[] messages)
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
    private void Show(NotifySeverity severity, string message, string? title, Action<AlertOptions>? configure)
    {
        var options = new AlertOptions();
        configure?.Invoke(options);

        CurrentAlert = new AlertItem
        {
            Severity = severity,
            Message = message,
            Title = title,
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
