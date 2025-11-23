using System.Reactive.Subjects;

namespace Neba.Web.Server.Services;

/// <summary>
/// Implementation of INotificationService using System.Reactive for publish-subscribe pattern.
/// Thread-safe for Blazor Server circuits.
/// </summary>
internal sealed class NotificationService : INotificationService, IDisposable
{
    private readonly Subject<NotificationPayload> _notificationSubject = new();
    private bool _disposed;

    /// <inheritdoc />
    public IObservable<NotificationPayload> Notifications => _notificationSubject;

    /// <inheritdoc />
    public void Info(string message, string? title = null, NotifyBehavior behavior = NotifyBehavior.ToastOnly)
    {
        Publish(new NotificationPayload(NotifySeverity.Info, message, title, behavior));
    }

    /// <inheritdoc />
    public void Success(string message, string? title = null, NotifyBehavior behavior = NotifyBehavior.ToastOnly)
    {
        Publish(new NotificationPayload(NotifySeverity.Success, message, title, behavior));
    }

    /// <inheritdoc />
    public void Warning(string message, string? title = null, NotifyBehavior behavior = NotifyBehavior.ToastOnly)
    {
        Publish(new NotificationPayload(NotifySeverity.Warning, message, title, behavior));
    }

    /// <inheritdoc />
    public void Error(string message, string? title = null, NotifyBehavior behavior = NotifyBehavior.ToastOnly)
    {
        Publish(new NotificationPayload(NotifySeverity.Error, message, title, behavior));
    }

    /// <inheritdoc />
    public void Normal(string message, string? title = null, NotifyBehavior behavior = NotifyBehavior.ToastOnly)
    {
        Publish(new NotificationPayload(NotifySeverity.Normal, message, title, behavior));
    }

    /// <inheritdoc />
    public void ValidationFailure(string message, NotifyBehavior? overrideBehavior = null)
    {
        var behavior = overrideBehavior ?? NotifyBehavior.AlertAndToast;

        var payload = new NotificationPayload(
            NotifySeverity.Error,
            message,
            "Validation Failed",
            behavior,
            Code: "VALIDATION_FAILURE"
        );

        Publish(payload);
    }

    /// <inheritdoc />
    public void Publish(NotificationPayload payload)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _notificationSubject.OnNext(payload);
    }

    /// <summary>
    /// Disposes the notification subject.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _notificationSubject.OnCompleted();
            _notificationSubject.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
