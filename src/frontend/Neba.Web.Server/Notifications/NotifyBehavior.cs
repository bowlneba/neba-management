namespace Neba.Web.Server.Notifications;

/// <summary>
/// Defines how a notification should be delivered to the user.
/// </summary>
public enum NotifyBehavior
{
    /// <summary>
    /// Show only as an inline alert (persistent).
    /// </summary>
    AlertOnly,

    /// <summary>
    /// Show only as a toast popup (ephemeral).
    /// </summary>
    ToastOnly,

    /// <summary>
    /// Show both alert and toast (default for form validation failures).
    /// </summary>
    AlertAndToast,

    /// <summary>
    /// Do not show any notification.
    /// </summary>
    None
}
