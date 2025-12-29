using ErrorOr;

namespace Neba.Domain.Addresses;

/// <summary>
/// Provides utilities to calculate distances between two <see cref="Address"/> instances.
/// Uses the Haversine formula to compute great-circle distances.
/// </summary>
public static class AddressDistanceCalculator
{
    /// <summary>
    /// Earth's radius in miles used for Haversine distance calculations.
    /// </summary>
    private const double EarthRadiusInMiles = 3958.8;

    /// <summary>
    /// Calculates the great-circle distance between two addresses in miles using the Haversine formula.
    /// Returns <see cref="AddressDistanceCalculatorErrors.AddressMissingCoordinates"/> if either address
    /// is missing coordinates.
    /// </summary>
    /// <param name="address1">The first address (required).</param>
    /// <param name="address2">The second address (required).</param>
    /// <returns>
    /// An <see cref="ErrorOr{Decimal}"/> containing the distance in miles, or an error if coordinates are missing.
    /// </returns>
    public static ErrorOr<decimal> DistanceInMiles(Address address1, Address address2)
    {
        ArgumentNullException.ThrowIfNull(address1);
        ArgumentNullException.ThrowIfNull(address2);

        if (address1.Coordinates is null || address2.Coordinates is null)
        {
            return AddressDistanceCalculatorErrors.AddressMissingCoordinates;
        }

        double latitude1Radians = address1.Coordinates.Latitude * (Math.PI / 180);
        double longitude1Radians = address1.Coordinates.Longitude * (Math.PI / 180);
        double latitude2Radians = address2.Coordinates.Latitude * (Math.PI / 180);
        double longitude2Radians = address2.Coordinates.Longitude * (Math.PI / 180);

        double deltaLatitude = latitude2Radians - latitude1Radians;
        double deltaLongitude = longitude2Radians - longitude1Radians;

        double haversineComponent = Math.Pow(Math.Sin(deltaLatitude / 2), 2) +
                                    (Math.Cos(latitude1Radians) * Math.Cos(latitude2Radians) *
                                    Math.Pow(Math.Sin(deltaLongitude / 2), 2));

        double centralAngle = 2 * Math.Asin(Math.Sqrt(haversineComponent));
        double distanceMiles = EarthRadiusInMiles * centralAngle;

        return (decimal)distanceMiles;
    }

    /// <summary>
    /// Calculates the great-circle distance between two addresses in kilometers.
    /// Returns the same errors as <see cref="DistanceInMiles"/> if coordinates are missing.
    /// </summary>
    /// <param name="address1">The first address (required).</param>
    /// <param name="address2">The second address (required).</param>
    /// <returns>
    /// An <see cref="ErrorOr{Decimal}"/> containing the distance in kilometers, or an error if coordinates are missing.
    /// </returns>
    public static ErrorOr<decimal> DistanceInKilometers(Address address1, Address address2)
    {
        ErrorOr<decimal> milesResult = DistanceInMiles(address1, address2);

        if (milesResult.IsError)
        {
            return milesResult.Errors;
        }

        decimal milesDistance = milesResult.Value;
        decimal kilometersDistance = milesDistance * 1.60934m;

        return kilometersDistance;
    }
}
