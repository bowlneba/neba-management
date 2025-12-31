namespace Neba.Web.Server.Maps;

/// <summary>
/// Represents the current mode of the map component.
/// </summary>
public enum MapMode
{
    /// <summary>
    /// Default overview mode showing all center pins.
    /// </summary>
    Overview,

    /// <summary>
    /// Directions preview mode - center selected, awaiting user location input.
    /// </summary>
    DirectionsPreview,

    /// <summary>
    /// Directions active mode - showing route and turn-by-turn directions.
    /// </summary>
    DirectionsActive
}
