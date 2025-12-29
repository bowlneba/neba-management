using ErrorOr;
using Neba.Domain.Addresses;

namespace Neba.UnitTests.Domain.Addresses;

public sealed class AddressDistanceCalculatorTests
{
    #region DistanceInMiles Tests

    [Fact(DisplayName = "Calculates distance in miles between two addresses with coordinates")]
    public void DistanceInMiles_WithValidCoordinates_ReturnsDistance()
    {
        // Arrange - Empire State Building to Willis Tower (approximately 712.72 miles)
        ErrorOr<Coordinates> empireStateCoordinates = Coordinates.Create(40.7484, -73.9857);
        ErrorOr<Address> empireStateAddress = Address.Create(
            "20 W 34th St",
            null,
            "New York",
            UsState.NewYork,
            "10001",
            empireStateCoordinates.Value);

        ErrorOr<Coordinates> willisTowerCoordinates = Coordinates.Create(41.8818, -87.6376);
        ErrorOr<Address> willisTowerAddress = Address.Create(
            "233 S Wacker Dr",
            null,
            "Chicago",
            UsState.Illinois,
            "60606",
            willisTowerCoordinates.Value);

        // Act
        ErrorOr<decimal> result = AddressDistanceCalculator.DistanceInMiles(
            empireStateAddress.Value,
            willisTowerAddress.Value);

        // Assert - Approximately 712.72 miles with ±2 mile tolerance for 35-mile rule
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBeGreaterThan(710m);
        result.Value.ShouldBeLessThan(715m);
    }

    [Fact(DisplayName = "Calculates distance of zero for identical coordinates")]
    public void DistanceInMiles_WithIdenticalCoordinates_ReturnsZero()
    {
        // Arrange
        ErrorOr<Coordinates> coordinates = Coordinates.Create(40.7128, -74.0060);
        ErrorOr<Address> address1 = Address.Create(
            "123 Main St",
            null,
            "New York",
            UsState.NewYork,
            "10001",
            coordinates.Value);

        ErrorOr<Address> address2 = Address.Create(
            "456 Oak Ave",
            null,
            "New York",
            UsState.NewYork,
            "10002",
            coordinates.Value);

        // Act
        ErrorOr<decimal> result = AddressDistanceCalculator.DistanceInMiles(
            address1.Value,
            address2.Value);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(0m, 0.01m);
    }

    [Fact(DisplayName = "Calculates short distance accurately")]
    public void DistanceInMiles_WithShortDistance_ReturnsAccurateDistance()
    {
        // Arrange - Two points approximately 10 miles apart
        ErrorOr<Coordinates> coordinates1 = Coordinates.Create(40.7128, -74.0060); // NYC
        ErrorOr<Coordinates> coordinates2 = Coordinates.Create(40.7489, -73.9680); // Queens (~10 miles)

        ErrorOr<Address> address1 = Address.Create(
            "123 Main St",
            null,
            "New York",
            UsState.NewYork,
            "10001",
            coordinates1.Value);

        ErrorOr<Address> address2 = Address.Create(
            "456 Queens Blvd",
            null,
            "Queens",
            UsState.NewYork,
            "11377",
            coordinates2.Value);

        // Act
        ErrorOr<decimal> result = AddressDistanceCalculator.DistanceInMiles(
            address1.Value,
            address2.Value);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBeGreaterThan(2m);
        result.Value.ShouldBeLessThan(5m);
    }

    [Fact(DisplayName = "Rejects null first address")]
    public void DistanceInMiles_WithNullFirstAddress_ThrowsArgumentNullException()
    {
        // Arrange
        ErrorOr<Coordinates> coordinates = Coordinates.Create(40.7128, -74.0060);
        ErrorOr<Address> address = Address.Create(
            "123 Main St",
            null,
            "New York",
            UsState.NewYork,
            "10001",
            coordinates.Value);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            AddressDistanceCalculator.DistanceInMiles(null!, address.Value));
    }

    [Fact(DisplayName = "Rejects null second address")]
    public void DistanceInMiles_WithNullSecondAddress_ThrowsArgumentNullException()
    {
        // Arrange
        ErrorOr<Coordinates> coordinates = Coordinates.Create(40.7128, -74.0060);
        ErrorOr<Address> address = Address.Create(
            "123 Main St",
            null,
            "New York",
            UsState.NewYork,
            "10001",
            coordinates.Value);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            AddressDistanceCalculator.DistanceInMiles(address.Value, null!));
    }

    [Fact(DisplayName = "Returns error when first address has no coordinates")]
    public void DistanceInMiles_WithFirstAddressMissingCoordinates_ReturnsError()
    {
        // Arrange
        ErrorOr<Address> address1 = Address.Create(
            "123 Main St",
            null,
            "New York",
            UsState.NewYork,
            "10001");

        ErrorOr<Coordinates> coordinates2 = Coordinates.Create(34.0522, -118.2437);
        ErrorOr<Address> address2 = Address.Create(
            "456 Hollywood Blvd",
            null,
            "Los Angeles",
            UsState.California,
            "90028",
            coordinates2.Value);

        // Act
        ErrorOr<decimal> result = AddressDistanceCalculator.DistanceInMiles(
            address1.Value,
            address2.Value);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(AddressDistanceCalculatorErrors.AddressMissingCoordinates.Code);
    }

    [Fact(DisplayName = "Returns error when second address has no coordinates")]
    public void DistanceInMiles_WithSecondAddressMissingCoordinates_ReturnsError()
    {
        // Arrange
        ErrorOr<Coordinates> coordinates1 = Coordinates.Create(40.7128, -74.0060);
        ErrorOr<Address> address1 = Address.Create(
            "123 Main St",
            null,
            "New York",
            UsState.NewYork,
            "10001",
            coordinates1.Value);

        ErrorOr<Address> address2 = Address.Create(
            "456 Hollywood Blvd",
            null,
            "Los Angeles",
            UsState.California,
            "90028");

        // Act
        ErrorOr<decimal> result = AddressDistanceCalculator.DistanceInMiles(
            address1.Value,
            address2.Value);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(AddressDistanceCalculatorErrors.AddressMissingCoordinates.Code);
    }

    [Fact(DisplayName = "Returns error when both addresses have no coordinates")]
    public void DistanceInMiles_WithBothAddressesMissingCoordinates_ReturnsError()
    {
        // Arrange
        ErrorOr<Address> address1 = Address.Create(
            "123 Main St",
            null,
            "New York",
            UsState.NewYork,
            "10001");

        ErrorOr<Address> address2 = Address.Create(
            "456 Hollywood Blvd",
            null,
            "Los Angeles",
            UsState.California,
            "90028");

        // Act
        ErrorOr<decimal> result = AddressDistanceCalculator.DistanceInMiles(
            address1.Value,
            address2.Value);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(AddressDistanceCalculatorErrors.AddressMissingCoordinates.Code);
    }

    #endregion

    #region DistanceInKilometers Tests

    [Fact(DisplayName = "Calculates distance in kilometers between two addresses with coordinates")]
    public void DistanceInKilometers_WithValidCoordinates_ReturnsDistance()
    {
        // Arrange - Empire State Building to Willis Tower (approximately 1147.01 kilometers)
        ErrorOr<Coordinates> empireStateCoordinates = Coordinates.Create(40.7484, -73.9857);
        ErrorOr<Address> empireStateAddress = Address.Create(
            "20 W 34th St",
            null,
            "New York",
            UsState.NewYork,
            "10001",
            empireStateCoordinates.Value);

        ErrorOr<Coordinates> willisTowerCoordinates = Coordinates.Create(41.8818, -87.6376);
        ErrorOr<Address> willisTowerAddress = Address.Create(
            "233 S Wacker Dr",
            null,
            "Chicago",
            UsState.Illinois,
            "60606",
            willisTowerCoordinates.Value);

        // Act
        ErrorOr<decimal> result = AddressDistanceCalculator.DistanceInKilometers(
            empireStateAddress.Value,
            willisTowerAddress.Value);

        // Assert - Approximately 1147.01 kilometers with ±3 km tolerance for 35-mile rule
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBeGreaterThan(1144m);
        result.Value.ShouldBeLessThan(1150m);
    }

    [Fact(DisplayName = "Converts miles to kilometers correctly")]
    public void DistanceInKilometers_WithValidCoordinates_ConvertsFromMilesCorrectly()
    {
        // Arrange - Empire State Building to Willis Tower
        ErrorOr<Coordinates> empireStateCoordinates = Coordinates.Create(40.7484, -73.9857);
        ErrorOr<Address> empireStateAddress = Address.Create(
            "20 W 34th St",
            null,
            "New York",
            UsState.NewYork,
            "10001",
            empireStateCoordinates.Value);

        ErrorOr<Coordinates> willisTowerCoordinates = Coordinates.Create(41.8818, -87.6376);
        ErrorOr<Address> willisTowerAddress = Address.Create(
            "233 S Wacker Dr",
            null,
            "Chicago",
            UsState.Illinois,
            "60606",
            willisTowerCoordinates.Value);

        // Act
        ErrorOr<decimal> milesResult = AddressDistanceCalculator.DistanceInMiles(
            empireStateAddress.Value,
            willisTowerAddress.Value);
        ErrorOr<decimal> kilometersResult = AddressDistanceCalculator.DistanceInKilometers(
            empireStateAddress.Value,
            willisTowerAddress.Value);

        // Assert
        decimal expectedKilometers = milesResult.Value * 1.60934m;
        kilometersResult.Value.ShouldBe(expectedKilometers);
    }

    [Fact(DisplayName = "Calculates distance of zero in kilometers for identical coordinates")]
    public void DistanceInKilometers_WithIdenticalCoordinates_ReturnsZero()
    {
        // Arrange
        ErrorOr<Coordinates> coordinates = Coordinates.Create(40.7128, -74.0060);
        ErrorOr<Address> address1 = Address.Create(
            "123 Main St",
            null,
            "New York",
            UsState.NewYork,
            "10001",
            coordinates.Value);

        ErrorOr<Address> address2 = Address.Create(
            "456 Oak Ave",
            null,
            "New York",
            UsState.NewYork,
            "10002",
            coordinates.Value);

        // Act
        ErrorOr<decimal> result = AddressDistanceCalculator.DistanceInKilometers(
            address1.Value,
            address2.Value);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(0m, 0.01m);
    }

    [Fact(DisplayName = "Rejects null first address for kilometers")]
    public void DistanceInKilometers_WithNullFirstAddress_ThrowsArgumentNullException()
    {
        // Arrange
        ErrorOr<Coordinates> coordinates = Coordinates.Create(40.7128, -74.0060);
        ErrorOr<Address> address = Address.Create(
            "123 Main St",
            null,
            "New York",
            UsState.NewYork,
            "10001",
            coordinates.Value);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            AddressDistanceCalculator.DistanceInKilometers(null!, address.Value));
    }

    [Fact(DisplayName = "Rejects null second address for kilometers")]
    public void DistanceInKilometers_WithNullSecondAddress_ThrowsArgumentNullException()
    {
        // Arrange
        ErrorOr<Coordinates> coordinates = Coordinates.Create(40.7128, -74.0060);
        ErrorOr<Address> address = Address.Create(
            "123 Main St",
            null,
            "New York",
            UsState.NewYork,
            "10001",
            coordinates.Value);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            AddressDistanceCalculator.DistanceInKilometers(address.Value, null!));
    }

    [Fact(DisplayName = "Returns error when first address has no coordinates for kilometers")]
    public void DistanceInKilometers_WithFirstAddressMissingCoordinates_ReturnsError()
    {
        // Arrange
        ErrorOr<Address> address1 = Address.Create(
            "123 Main St",
            null,
            "New York",
            UsState.NewYork,
            "10001");

        ErrorOr<Coordinates> coordinates2 = Coordinates.Create(34.0522, -118.2437);
        ErrorOr<Address> address2 = Address.Create(
            "456 Hollywood Blvd",
            null,
            "Los Angeles",
            UsState.California,
            "90028",
            coordinates2.Value);

        // Act
        ErrorOr<decimal> result = AddressDistanceCalculator.DistanceInKilometers(
            address1.Value,
            address2.Value);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(AddressDistanceCalculatorErrors.AddressMissingCoordinates.Code);
    }

    [Fact(DisplayName = "Returns error when second address has no coordinates for kilometers")]
    public void DistanceInKilometers_WithSecondAddressMissingCoordinates_ReturnsError()
    {
        // Arrange
        ErrorOr<Coordinates> coordinates1 = Coordinates.Create(40.7128, -74.0060);
        ErrorOr<Address> address1 = Address.Create(
            "123 Main St",
            null,
            "New York",
            UsState.NewYork,
            "10001",
            coordinates1.Value);

        ErrorOr<Address> address2 = Address.Create(
            "456 Hollywood Blvd",
            null,
            "Los Angeles",
            UsState.California,
            "90028");

        // Act
        ErrorOr<decimal> result = AddressDistanceCalculator.DistanceInKilometers(
            address1.Value,
            address2.Value);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(AddressDistanceCalculatorErrors.AddressMissingCoordinates.Code);
    }

    [Fact(DisplayName = "Returns error when both addresses have no coordinates for kilometers")]
    public void DistanceInKilometers_WithBothAddressesMissingCoordinates_ReturnsError()
    {
        // Arrange
        ErrorOr<Address> address1 = Address.Create(
            "123 Main St",
            null,
            "New York",
            UsState.NewYork,
            "10001");

        ErrorOr<Address> address2 = Address.Create(
            "456 Hollywood Blvd",
            null,
            "Los Angeles",
            UsState.California,
            "90028");

        // Act
        ErrorOr<decimal> result = AddressDistanceCalculator.DistanceInKilometers(
            address1.Value,
            address2.Value);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(AddressDistanceCalculatorErrors.AddressMissingCoordinates.Code);
    }

    #endregion
}
