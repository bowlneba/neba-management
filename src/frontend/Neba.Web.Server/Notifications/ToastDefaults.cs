namespace Neba.Web.Server.Notifications;

/// <summary>
/// Default configuration values for toast notifications.
/// </summary>
internal static class ToastDefaults
{
    /// <summary>
    /// Default toast duration for mobile devices (in milliseconds).
    /// </summary>
    public const int MobileDurationMs = 3000;

    /// <summary>
    /// Default toast duration for desktop devices (in milliseconds).
    /// </summary>
    public const int DesktopDurationMs = 4000;

    /// <summary>
    /// Maximum number of toasts that can be queued.
    /// </summary>
    public const int MaxQueueSize = 10;
}
