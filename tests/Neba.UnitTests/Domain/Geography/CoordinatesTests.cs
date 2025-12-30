using ErrorOr;
using Neba.Domain.Geography;

namespace Neba.UnitTests.Domain.Geography;

public sealed class CoordinatesTests
{
    [Fact(DisplayName = "Create should return Coordinates for valid latitude and longitude")]
    public void Create_ShouldReturnCoordinates_WhenValidLatitudeAndLongitudeProvided()
    {
        // Arrange
        const double latitude = 45.0;
        const double longitude = -75.0;

        // Act
        ErrorOr<Coordinates> result = Coordinates.Create(latitude, longitude);

        // Assert
        result.IsError.ShouldBeFalse();

        Coordinates coordinates = result.Value;
        coordinates.Latitude.ShouldBe(latitude);
        coordinates.Longitude.ShouldBe(longitude);
    }

    [Fact(DisplayName = "Create should return error for invalid latitude")]
    public void Create_ShouldReturnError_WhenInvalidLatitudeProvided()
    {
        // Arrange
        const double invalidLatitude = 100.0; // Invalid latitude
        const double longitude = -75.0;

        // Act
        ErrorOr<Coordinates> result = Coordinates.Create(invalidLatitude, longitude);

        // Assert
        result.IsError.ShouldBeTrue();

        result.Errors.ShouldContain(CoordinatesErrors.InvalidLatitude);
    }

    [Fact(DisplayName = "Create should return error for invalid longitude")]
    public void Create_ShouldReturnError_WhenInvalidLongitudeProvided()
    {
        // Arrange
        const double latitude = 45.0;
        const double invalidLongitude = -200.0; // Invalid longitude

        // Act
        ErrorOr<Coordinates> result = Coordinates.Create(latitude, invalidLongitude);

        // Assert
        result.IsError.ShouldBeTrue();

        result.Errors.ShouldContain(CoordinatesErrors.InvalidLongitude);
    }
}
