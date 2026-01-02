
using ErrorOr;

namespace Neba.Domain.Geography;

/// <summary>
/// Represents a geographic coordinate with latitude and longitude.
/// </summary>
public sealed record Coordinates
{
    /// <summary>
    /// Gets the latitude component of the coordinate.
    /// Range: -90 to 90.
    /// </summary>
    public double Latitude { get; internal init; }

    /// <summary>
    /// Gets the longitude component of the coordinate.
    /// Range: -180 to 180.
    /// </summary>
    public double Longitude { get; internal init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Coordinates"/> record.
    /// </summary>
    /// <param name="latitude">The latitude value (-90 to 90).</param>
    /// <param name="longitude">The longitude value (-180 to 180).</param>
    private Coordinates(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    internal Coordinates()
    { }

    /// <summary>
    /// Creates a new <see cref="Coordinates"/> instance if the latitude and longitude are valid.
    /// </summary>
    /// <param name="latitude">The latitude value (-90 to 90).</param>
    /// <param name="longitude">The longitude value (-180 to 180).</param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> containing the <see cref="Coordinates"/> if valid, or an error if invalid.
    /// </returns>
    public static ErrorOr<Coordinates> Create(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            return CoordinatesErrors.InvalidLatitude;
        }

        if (longitude < -180 || longitude > 180)
        {
            return CoordinatesErrors.InvalidLongitude;
        }

        return new Coordinates(latitude, longitude);
    }

    /// <summary>
    /// Returns a string representation of the coordinates in "Latitude, Longitude" format.
    /// </summary>
    /// <returns>A string in the format "Latitude, Longitude".</returns>
    public override string ToString()
        => $"{Latitude}, {Longitude}";
}
