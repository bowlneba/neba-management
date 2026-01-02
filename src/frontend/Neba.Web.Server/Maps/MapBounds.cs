namespace Neba.Web.Server.Maps;

/// <summary>
/// Represents the geographic bounds of the visible map viewport.
/// </summary>
/// <param name="North">The northern (maximum) latitude</param>
/// <param name="South">The southern (minimum) latitude</param>
/// <param name="East">The eastern (maximum) longitude</param>
/// <param name="West">The western (minimum) longitude</param>
public sealed record MapBounds(
    double North,
    double South,
    double East,
    double West)
{
    /// <summary>
    /// Checks if the given latitude and longitude are within these bounds.
    /// </summary>
    public bool Contains(double latitude, double longitude)
    {
        return latitude >= South &&
               latitude <= North &&
               longitude >= West &&
               longitude <= East;
    }
}
