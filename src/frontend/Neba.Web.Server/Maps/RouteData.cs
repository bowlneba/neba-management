using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Neba.Web.Server.Maps;

/// <summary>
/// Represents route data returned from Azure Maps Routing API.
/// </summary>
public sealed class RouteData
{
    /// <summary>
    /// Total distance in meters.
    /// </summary>
    public double DistanceMeters { get; set; }

    /// <summary>
    /// Total travel time in seconds.
    /// </summary>
    public int TravelTimeSeconds { get; set; }

    /// <summary>
    /// Turn-by-turn directions.
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only (needed for JSON deserialization)
    public Collection<RouteInstruction> Instructions { get; set; } = [];
#pragma warning restore CA2227

    /// <summary>
    /// GeoJSON representation of the route line (for drawing on map).
    /// </summary>
    public string? RouteGeoJson { get; set; }

    /// <summary>
    /// Gets the distance formatted for display.
    /// </summary>
    public string FormattedDistance
    {
        get
        {
            var miles = DistanceMeters * 0.000621371; // Convert meters to miles
            return $"{miles:F1} mi";
        }
    }

    /// <summary>
    /// Gets the travel time formatted for display.
    /// </summary>
    public string FormattedTravelTime
    {
        get
        {
            var minutes = TravelTimeSeconds / 60;
            if (minutes < 60)
            {
                return $"{minutes} min";
            }

            var hours = minutes / 60;
            var remainingMinutes = minutes % 60;
            return $"{hours} hr {remainingMinutes} min";
        }
    }
}
