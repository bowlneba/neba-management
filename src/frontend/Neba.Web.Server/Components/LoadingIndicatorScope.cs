namespace Neba.Web.Server.Components;

/// <summary>
/// Defines the scope of the loading indicator overlay.
/// </summary>
public enum LoadingIndicatorScope
{
    /// <summary>
    /// Overlays only the page content area, leaving navigation and footer accessible.
    /// </summary>
    Page,

    /// <summary>
    /// Overlays the entire viewport including navigation and all UI elements.
    /// </summary>
    FullScreen,

    /// <summary>
    /// Overlays a specific section/container with absolute positioning.
    /// Parent container should have position: relative and min-height set.
    /// </summary>
    Section
}
