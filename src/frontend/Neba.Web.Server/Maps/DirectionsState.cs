namespace Neba.Web.Server.Maps;

/// <summary>
/// Represents the state of the directions feature.
/// </summary>
#pragma warning disable CA1819 // Properties should not return arrays (arrays are needed for JavaScript interop)
public sealed class DirectionsState
{
    /// <summary>
    /// The current mode of the map.
    /// </summary>
    public MapMode Mode { get; set; } = MapMode.Overview;

    /// <summary>
    /// The ID of the selected bowling center (destination).
    /// </summary>
    public string? SelectedCenterId { get; set; }

    /// <summary>
    /// The name of the selected bowling center.
    /// </summary>
    public string? SelectedCenterName { get; set; }

    /// <summary>
    /// The user's starting location coordinates [longitude, latitude].
    /// </summary>
    public double[]? UserLocation { get; set; }

    /// <summary>
    /// The user's starting address (if entered manually).
    /// </summary>
    public string? UserAddress { get; set; }

    /// <summary>
    /// The destination coordinates [longitude, latitude].
    /// </summary>
    public double[]? DestinationLocation { get; set; }

    /// <summary>
    /// The calculated route data from Azure Maps.
    /// </summary>
    public RouteData? Route { get; set; }

    /// <summary>
    /// Whether the component is currently loading (geocoding, routing, etc.).
    /// </summary>
    public bool IsLoading { get; set; }

    /// <summary>
    /// Any error message to display to the user.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Resets the state back to overview mode.
    /// </summary>
    public void Reset()
    {
        Mode = MapMode.Overview;
        SelectedCenterId = null;
        SelectedCenterName = null;
        UserLocation = null;
        UserAddress = null;
        DestinationLocation = null;
        Route = null;
        IsLoading = false;
        ErrorMessage = null;
    }
}
#pragma warning restore CA1819
