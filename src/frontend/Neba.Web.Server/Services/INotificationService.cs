namespace Neba.Web.Server.Services;

/// <summary>
/// Service for managing and publishing notifications (alerts and toasts) throughout the application.
/// </summary>
internal interface INotificationService
{
    /// <summary>
    /// Show an informational notification (blue theme).
    /// </summary>
    /// <param name="message">The notification message.</param>
    /// <param name="title">Optional title for the notification.</param>
    void Info(string message, string? title = null);

    /// <summary>
    /// Show a success notification (green theme).
    /// </summary>
    /// <param name="message">The notification message.</param>
    /// <param name="title">Optional title for the notification.</param>
    void Success(string message, string? title = null);

    /// <summary>
    /// Show a warning notification (goldenrod/orange theme).
    /// </summary>
    /// <param name="message">The notification message.</param>
    /// <param name="title">Optional title for the notification.</param>
    void Warning(string message, string? title = null);

    /// <summary>
    /// Show an error notification (red theme).
    /// </summary>
    /// <param name="message">The notification message.</param>
    /// <param name="title">Optional title for the notification.</param>
    void Error(string message, string? title = null);

    /// <summary>
    /// Show a normal notification (gray theme).
    /// </summary>
    /// <param name="message">The notification message.</param>
    /// <param name="title">Optional title for the notification.</param>
    void Normal(string message, string? title = null);

    /// <summary>
    /// Show a validation failure notification. Default behavior is AlertAndToast.
    /// </summary>
    /// <param name="message">The validation failure message.</param>
    /// <param name="overrideBehavior">Optional override for the default AlertAndToast behavior.</param>
    void ValidationFailure(string message, NotifyBehavior? overrideBehavior = null);

    /// <summary>
    /// Publish a notification with full control over the payload.
    /// </summary>
    /// <param name="payload">The notification payload to publish.</param>
    void Publish(NotificationPayload payload);

    /// <summary>
    /// Observable stream of all published notifications.
    /// UI components subscribe to this to receive and display notifications.
    /// </summary>
    IObservable<NotificationPayload> Notifications { get; }
}
