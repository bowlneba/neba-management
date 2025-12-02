using System.Reactive.Subjects;
using Neba.Web.Server.Services;

namespace Neba.Web.Server.Notifications;

/// <summary>
/// Implementation of INotificationService using System.Reactive for publish-subscribe pattern.
/// </summary>
/// <remarks>
/// Thread safety: This service is scoped per Blazor Server circuit and relies on
/// Blazor's single-threaded synchronization context. It is safe within a circuit
/// but not designed for use in multi-threaded scenarios.
/// </remarks>
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
            Persist: false,
            Code: "VALIDATION_FAILURE"
        );

        Publish(payload);
    }

    /// <inheritdoc />
    public void Publish(NotificationPayload payload)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            _notificationSubject.OnNext(payload);
        }
        catch (ObjectDisposedException)
        {
            // Subject was disposed during publish - this is expected during shutdown.
            // When logging is introduced, log this exception at Debug level.
            // Swallow the exception to prevent crashes during cleanup.
        }
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
