using Neba.Domain.Contact;
using Neba.Tests.Website;
using Neba.Web.Server.BowlingCenters;
using Neba.Website.Contracts.BowlingCenters;

namespace Neba.UnitTests.Ui.Server.BowlingCenters;
[Trait("Category", "Unit")]
[Trait("Component", "Ui.Server.BowlingCenters")]

public sealed class BowlingCenterMappingExtensionsTests
{
    [Fact(DisplayName = "Maps name from BowlingCenterResponse to view model")]
    public void BowlingCenterResponse_ToViewModel_ShouldMapName()
    {
        // Arrange
        const string name = "Strike Zone Bowling";
        BowlingCenterResponse response = BowlingCenterResponseFactory.Create(name: name);

        // Act
        BowlingCenterViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.Name.ShouldBe(name);
    }

    [Fact(DisplayName = "Maps street from BowlingCenterResponse to view model")]
    public void BowlingCenterResponse_ToViewModel_ShouldMapStreet()
    {
        // Arrange
        const string street = "456 Bowling Lane";
        BowlingCenterResponse response = BowlingCenterResponseFactory.Create(street: street);

        // Act
        BowlingCenterViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.Street.ShouldBe(street);
    }

    [Fact(DisplayName = "Maps unit from BowlingCenterResponse to view model")]
    public void BowlingCenterResponse_ToViewModel_ShouldMapUnit()
    {
        // Arrange
        const string unit = "Suite 100";
        BowlingCenterResponse response = BowlingCenterResponseFactory.Create(unit: unit);

        // Act
        BowlingCenterViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.Unit.ShouldBe(unit);
    }

    [Fact(DisplayName = "Maps null unit from BowlingCenterResponse to view model")]
    public void BowlingCenterResponse_ToViewModel_WithNullUnit_ShouldMapNullUnit()
    {
        // Arrange
        BowlingCenterResponse response = BowlingCenterResponseFactory.Create(unit: null);

        // Act
        BowlingCenterViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.Unit.ShouldBeNull();
    }

    [Fact(DisplayName = "Maps city from BowlingCenterResponse to view model")]
    public void BowlingCenterResponse_ToViewModel_ShouldMapCity()
    {
        // Arrange
        const string city = "Huntington";
        BowlingCenterResponse response = BowlingCenterResponseFactory.Create(city: city);

        // Act
        BowlingCenterViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.City.ShouldBe(city);
    }

    [Fact(DisplayName = "Maps state value from BowlingCenterResponse to view model")]
    public void BowlingCenterResponse_ToViewModel_ShouldMapStateValue()
    {
        // Arrange
        UsState state = UsState.NewYork;
        BowlingCenterResponse response = BowlingCenterResponseFactory.Create(state: state);

        // Act
        BowlingCenterViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.State.ShouldBe(state.Value);
    }

    [Fact(DisplayName = "Maps zip code from BowlingCenterResponse to view model")]
    public void BowlingCenterResponse_ToViewModel_ShouldMapZipCode()
    {
        // Arrange
        const string zipCode = "11743";
        BowlingCenterResponse response = BowlingCenterResponseFactory.Create(zipCode: zipCode);

        // Act
        BowlingCenterViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.ZipCode.ShouldBe(zipCode);
    }

    [Fact(DisplayName = "Maps phone number from BowlingCenterResponse to view model")]
    public void BowlingCenterResponse_ToViewModel_ShouldMapPhoneNumber()
    {
        // Arrange
        const string phoneNumber = "16315559876";
        BowlingCenterResponse response = BowlingCenterResponseFactory.Create(phoneNumber: phoneNumber);

        // Act
        BowlingCenterViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.PhoneNumber.ShouldBe(phoneNumber);
    }

    [Fact(DisplayName = "Maps phone extension from BowlingCenterResponse to view model")]
    public void BowlingCenterResponse_ToViewModel_ShouldMapPhoneExtension()
    {
        // Arrange
        const string extension = "204";
        BowlingCenterResponse response = BowlingCenterResponseFactory.Create(phoneExtension: extension);

        // Act
        BowlingCenterViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.PhoneExtension.ShouldBe(extension);
    }

    [Fact(DisplayName = "Maps null phone extension from BowlingCenterResponse to view model")]
    public void BowlingCenterResponse_ToViewModel_WithNullExtension_ShouldMapNullPhoneExtension()
    {
        // Arrange
        BowlingCenterResponse response = BowlingCenterResponseFactory.Create(phoneExtension: null);

        // Act
        BowlingCenterViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.PhoneExtension.ShouldBeNull();
    }

    [Fact(DisplayName = "Maps latitude from BowlingCenterResponse to view model")]
    public void BowlingCenterResponse_ToViewModel_ShouldMapLatitude()
    {
        // Arrange
        const double latitude = 40.8682;
        BowlingCenterResponse response = BowlingCenterResponseFactory.Create(latitude: latitude);

        // Act
        BowlingCenterViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.Latitude.ShouldBe(latitude);
    }

    [Fact(DisplayName = "Maps longitude from BowlingCenterResponse to view model")]
    public void BowlingCenterResponse_ToViewModel_ShouldMapLongitude()
    {
        // Arrange
        const double longitude = -73.4227;
        BowlingCenterResponse response = BowlingCenterResponseFactory.Create(longitude: longitude);

        // Act
        BowlingCenterViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.Longitude.ShouldBe(longitude);
    }

    [Fact(DisplayName = "Maps isClosed from BowlingCenterResponse to view model")]
    public void BowlingCenterResponse_ToViewModel_ShouldMapIsClosed()
    {
        // Arrange
        const bool isClosed = true;
        BowlingCenterResponse response = BowlingCenterResponseFactory.Create(isClosed: isClosed);

        // Act
        BowlingCenterViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.IsClosed.ShouldBe(isClosed);
    }

    [Fact(DisplayName = "Maps all properties correctly from BowlingCenterResponse to view model")]
    public void BowlingCenterResponse_ToViewModel_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        const string name = "Perfect Game Lanes";
        const string street = "789 Strike Street";
        const string unit = "Building A";
        const string city = "Melville";
        UsState state = UsState.Connecticut;
        const string zipCode = "11747";
        const string phoneNumber = "16315551234";
        const string extension = "305";
        const double latitude = 40.7933;
        const double longitude = -73.4151;
        const bool isClosed = true;

        BowlingCenterResponse response = BowlingCenterResponseFactory.Create(
            name: name,
            street: street,
            unit: unit,
            city: city,
            state: state,
            zipCode: zipCode,
            phoneNumber: phoneNumber,
            phoneExtension: extension,
            latitude: latitude,
            longitude: longitude,
            isClosed: isClosed);

        // Act
        BowlingCenterViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.Name.ShouldBe(name);
        viewModel.Street.ShouldBe(street);
        viewModel.Unit.ShouldBe(unit);
        viewModel.City.ShouldBe(city);
        viewModel.State.ShouldBe(state.Value);
        viewModel.ZipCode.ShouldBe(zipCode);
        viewModel.PhoneNumber.ShouldBe(phoneNumber);
        viewModel.PhoneExtension.ShouldBe(extension);
        viewModel.Latitude.ShouldBe(latitude);
        viewModel.Longitude.ShouldBe(longitude);
        viewModel.IsClosed.ShouldBe(isClosed);
    }
}
