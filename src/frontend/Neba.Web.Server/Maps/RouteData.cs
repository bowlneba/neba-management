using System.Collections.ObjectModel;
using UnitsNet;

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
            Length distance = Length.FromMeters(DistanceMeters);
            double miles = distance.Miles;
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
            int minutes = TravelTimeSeconds / 60;
            if (minutes < 60)
            {
                return $"{minutes} min";
            }

            int hours = minutes / 60;
            int remainingMinutes = minutes % 60;
            return $"{hours} hr {remainingMinutes} min";
        }
    }
}
