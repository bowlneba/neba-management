namespace Neba.Web.Server.Maps;

/// <summary>
/// Represents a location to be displayed on the NEBA map component.
/// </summary>
/// <param name="Id">Unique identifier for this location</param>
/// <param name="Title">Primary title/name of the location</param>
/// <param name="Description">Detailed description or address information</param>
/// <param name="Latitude">Geographic latitude coordinate</param>
/// <param name="Longitude">Geographic longitude coordinate</param>
/// <param name="Metadata">Optional additional data to attach to this location</param>
public sealed record NebaMapLocation(
    string Id,
    string Title,
    string Description,
    double Latitude,
    double Longitude,
    Dictionary<string, object>? Metadata = null
);
