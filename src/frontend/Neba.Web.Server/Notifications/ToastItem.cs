namespace Neba.Web.Server.Notifications;

/// <summary>
/// Represents a toast notification item with a unique identifier.
/// </summary>
internal sealed class ToastItem
{
    /// <summary>
    /// Unique identifier for this toast.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The notification payload to display.
    /// </summary>
    public NotificationPayload Payload { get; init; } = null!;
}
