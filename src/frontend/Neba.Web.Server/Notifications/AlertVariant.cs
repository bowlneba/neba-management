namespace Neba.Web.Server.Notifications;

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
