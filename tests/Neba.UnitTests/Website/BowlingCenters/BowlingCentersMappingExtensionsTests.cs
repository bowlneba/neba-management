using Neba.Domain.Contact;
using Neba.Tests.Website;
using Neba.Website.Application.BowlingCenters;
using Neba.Website.Contracts.BowlingCenters;
using Neba.Website.Endpoints.BowlingCenters;

namespace Neba.UnitTests.Website.BowlingCenters;

public sealed class BowlingCentersMappingExtensionsTests
{
    [Fact(DisplayName = "Maps name from dto to response")]
    public void BowlingCenterDto_ToResponse_ShouldMapName()
    {
        // Arrange
        const string name = "Strike Zone Bowling";
        BowlingCenterDto dto = BowlingCenterDtoFactory.Create(name: name);

        // Act
        BowlingCenterResponse response = dto.ToResponse();

        // Assert
        response.Name.ShouldBe(name);
    }

    [Fact(DisplayName = "Maps street from dto to response")]
    public void BowlingCenterDto_ToResponse_ShouldMapStreet()
    {
        // Arrange
        const string street = "456 Bowling Lane";
        BowlingCenterDto dto = BowlingCenterDtoFactory.Create(street: street);

        // Act
        BowlingCenterResponse response = dto.ToResponse();

        // Assert
        response.Street.ShouldBe(street);
    }

    [Fact(DisplayName = "Maps unit from dto to response")]
    public void BowlingCenterDto_ToResponse_ShouldMapUnit()
    {
        // Arrange
        const string unit = "Suite 100";
        BowlingCenterDto dto = BowlingCenterDtoFactory.Create(unit: unit);

        // Act
        BowlingCenterResponse response = dto.ToResponse();

        // Assert
        response.Unit.ShouldBe(unit);
    }

    [Fact(DisplayName = "Maps null unit from dto to response")]
    public void BowlingCenterDto_ToResponse_WithNullUnit_ShouldMapNullUnit()
    {
        // Arrange
        BowlingCenterDto dto = BowlingCenterDtoFactory.Create(unit: null);

        // Act
        BowlingCenterResponse response = dto.ToResponse();

        // Assert
        response.Unit.ShouldBeNull();
    }

    [Fact(DisplayName = "Maps city from dto to response")]
    public void BowlingCenterDto_ToResponse_ShouldMapCity()
    {
        // Arrange
        const string city = "Huntington";
        BowlingCenterDto dto = BowlingCenterDtoFactory.Create(city: city);

        // Act
        BowlingCenterResponse response = dto.ToResponse();

        // Assert
        response.City.ShouldBe(city);
    }

    [Fact(DisplayName = "Maps state from dto to response")]
    public void BowlingCenterDto_ToResponse_ShouldMapState()
    {
        // Arrange
        UsState state = UsState.NewYork;
        BowlingCenterDto dto = BowlingCenterDtoFactory.Create(state: state);

        // Act
        BowlingCenterResponse response = dto.ToResponse();

        // Assert
        response.State.ShouldBe(state);
    }

    [Fact(DisplayName = "Maps zip code from dto to response")]
    public void BowlingCenterDto_ToResponse_ShouldMapZipCode()
    {
        // Arrange
        const string zipCode = "11743";
        BowlingCenterDto dto = BowlingCenterDtoFactory.Create(zipCode: zipCode);

        // Act
        BowlingCenterResponse response = dto.ToResponse();

        // Assert
        response.ZipCode.ShouldBe(zipCode);
    }

    [Fact(DisplayName = "Maps phone number from dto to response")]
    public void BowlingCenterDto_ToResponse_ShouldMapPhoneNumber()
    {
        // Arrange
        const string phoneNumber = "(631) 555-9876";
        BowlingCenterDto dto = BowlingCenterDtoFactory.Create(phoneNumber: phoneNumber);

        // Act
        BowlingCenterResponse response = dto.ToResponse();

        // Assert
        response.PhoneNumber.ShouldBe(phoneNumber);
    }

    [Fact(DisplayName = "Maps extension to phone extension in response")]
    public void BowlingCenterDto_ToResponse_ShouldMapExtensionToPhoneExtension()
    {
        // Arrange
        const string extension = "204";
        BowlingCenterDto dto = BowlingCenterDtoFactory.Create(extension: extension);

        // Act
        BowlingCenterResponse response = dto.ToResponse();

        // Assert
        response.PhoneExtension.ShouldBe(extension);
    }

    [Fact(DisplayName = "Maps null extension to null phone extension in response")]
    public void BowlingCenterDto_ToResponse_WithNullExtension_ShouldMapNullPhoneExtension()
    {
        // Arrange
        BowlingCenterDto dto = BowlingCenterDtoFactory.Create(extension: null);

        // Act
        BowlingCenterResponse response = dto.ToResponse();

        // Assert
        response.PhoneExtension.ShouldBeNull();
    }

    [Fact(DisplayName = "Maps latitude from dto to response")]
    public void BowlingCenterDto_ToResponse_ShouldMapLatitude()
    {
        // Arrange
        const double latitude = 40.8682;
        BowlingCenterDto dto = BowlingCenterDtoFactory.Create(latitude: latitude);

        // Act
        BowlingCenterResponse response = dto.ToResponse();

        // Assert
        response.Latitude.ShouldBe(latitude);
    }

    [Fact(DisplayName = "Maps longitude from dto to response")]
    public void BowlingCenterDto_ToResponse_ShouldMapLongitude()
    {
        // Arrange
        const double longitude = -73.4227;
        BowlingCenterDto dto = BowlingCenterDtoFactory.Create(longitude: longitude);

        // Act
        BowlingCenterResponse response = dto.ToResponse();

        // Assert
        response.Longitude.ShouldBe(longitude);
    }

    [Fact(DisplayName = "Maps isClosed from dto to response")]
    public void BowlingCenterDto_ToResponse_ShouldMapIsClosed()
    {
        // Arrange
        const bool isClosed = true;
        BowlingCenterDto dto = BowlingCenterDtoFactory.Create(isClosed: isClosed);

        // Act
        BowlingCenterResponse response = dto.ToResponse();

        // Assert
        response.IsClosed.ShouldBe(isClosed);
    }

    [Fact(DisplayName = "Maps all properties correctly from dto to response")]
    public void BowlingCenterDto_ToResponse_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        const string name = "Perfect Game Lanes";
        const string street = "789 Strike Street";
        const string unit = "Building A";
        const string city = "Melville";
        UsState state = UsState.NewYork;
        const string zipCode = "11747";
        const string phoneNumber = "(631) 555-1234";
        const string extension = "305";
        const double latitude = 40.7933;
        const double longitude = -73.4151;
        const bool isClosed = true;

        BowlingCenterDto dto = BowlingCenterDtoFactory.Create(
            name: name,
            street: street,
            unit: unit,
            city: city,
            state: state,
            zipCode: zipCode,
            phoneNumber: phoneNumber,
            extension: extension,
            latitude: latitude,
            longitude: longitude,
            isClosed: isClosed);

        // Act
        BowlingCenterResponse response = dto.ToResponse();

        // Assert
        response.Name.ShouldBe(name);
        response.Street.ShouldBe(street);
        response.Unit.ShouldBe(unit);
        response.City.ShouldBe(city);
        response.State.ShouldBe(state);
        response.ZipCode.ShouldBe(zipCode);
        response.PhoneNumber.ShouldBe(phoneNumber);
        response.PhoneExtension.ShouldBe(extension);
        response.Latitude.ShouldBe(latitude);
        response.Longitude.ShouldBe(longitude);
        response.IsClosed.ShouldBe(isClosed);
    }
}
