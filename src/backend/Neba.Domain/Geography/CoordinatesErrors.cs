using ErrorOr;

namespace Neba.Domain.Geography;

internal static class CoordinatesErrors
{
    public static readonly Error InvalidLatitude = Error.Validation(
        code: "Coordinates.InvalidLatitude",
        description: "Latitude must be between -90 and 90 degrees.");

    public static readonly Error InvalidLongitude = Error.Validation(
        code: "Coordinates.InvalidLongitude",
        description: "Longitude must be between -180 and 180 degrees.");
}
