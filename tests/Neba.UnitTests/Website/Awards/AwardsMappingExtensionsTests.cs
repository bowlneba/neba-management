using Neba.Domain;
using Neba.Domain.Awards;
using Neba.Tests;
using Neba.Tests.Website;
using Neba.Website.Application.Awards.HallOfFame;
using Neba.Website.Contracts.Awards;
using Neba.Website.Endpoints.Awards;

namespace Neba.UnitTests.Website.Awards;
[Trait("Category", "Unit")]
[Trait("Component", "Website.Awards")]

public sealed class AwardsMappingExtensionsTests
{
    [Fact(DisplayName = "Maps year from HallOfFameInductionDto to response model")]
    public void HallOfFameInductionDto_ToResponseModel_ShouldMapYear()
    {
        // Arrange
        const int year = 2024;
        HallOfFameInductionDto dto = HallOfFameInductionDtoFactory.Create(year: year);

        // Act
        HallOfFameInductionResponse response = dto.ToResponseModel();

        // Assert
        response.Year.ShouldBe(year);
    }

    [Fact(DisplayName = "Maps bowler name to display name in response model")]
    public void HallOfFameInductionDto_ToResponseModel_ShouldMapBowlerNameToDisplayName()
    {
        // Arrange
        Name bowlerName = NameFactory.Create(firstName: "John", lastName: "Doe");
        HallOfFameInductionDto dto = HallOfFameInductionDtoFactory.Create(bowlerName: bowlerName);

        // Act
        HallOfFameInductionResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerName.ShouldBe(bowlerName.ToDisplayName());
        response.BowlerName.ShouldBe("John Doe");
    }

    [Fact(DisplayName = "Maps photo URI from dto to response model")]
    public void HallOfFameInductionDto_ToResponseModel_ShouldMapPhotoUri()
    {
        // Arrange
        Uri photoUri = new Uri("https://storage.example.com/photos/inductee.jpg");
        StoredFile photo = StoredFileFactory.Create(container: "photos", path: "inductee.jpg");
        HallOfFameInductionDto dto = HallOfFameInductionDtoFactory.Create(photo: photo);
        dto = dto with { PhotoUri = photoUri }; // Set internal PhotoUri

        // Act
        HallOfFameInductionResponse response = dto.ToResponseModel();

        // Assert
        response.PhotoUrl.ShouldBe(photoUri);
    }

    [Fact(DisplayName = "Maps null photo URL when photo is null")]
    public void HallOfFameInductionDto_ToResponseModel_WithNullPhoto_ShouldMapNullPhotoUrl()
    {
        // Arrange
        HallOfFameInductionDto dto = HallOfFameInductionDtoFactory.Create(photo: null);

        // Act
        HallOfFameInductionResponse response = dto.ToResponseModel();

        // Assert
        response.PhotoUrl.ShouldBeNull();
    }

    [Fact(DisplayName = "Maps category enums to category names")]
    public void HallOfFameInductionDto_ToResponseModel_ShouldMapCategoriesToNames()
    {
        // Arrange
        IReadOnlyCollection<HallOfFameCategory> categories = [HallOfFameCategory.SuperiorPerformance, HallOfFameCategory.MeritoriousService];
        HallOfFameInductionDto dto = HallOfFameInductionDtoFactory.Create(categories: categories);

        // Act
        HallOfFameInductionResponse response = dto.ToResponseModel();

        // Assert
        response.Categories.ShouldNotBeNull();
        response.Categories.Count.ShouldBe(2);
        response.Categories.ShouldContain("Superior Performance");
        response.Categories.ShouldContain("Meritorious Service");
    }

    [Fact(DisplayName = "Maps single category correctly")]
    public void HallOfFameInductionDto_ToResponseModel_WithSingleCategory_ShouldMapCorrectly()
    {
        // Arrange
        IReadOnlyCollection<HallOfFameCategory> categories = [HallOfFameCategory.FriendOfNeba];
        HallOfFameInductionDto dto = HallOfFameInductionDtoFactory.Create(categories: categories);

        // Act
        HallOfFameInductionResponse response = dto.ToResponseModel();

        // Assert
        response.Categories.Count.ShouldBe(1);
        response.Categories.Single().ShouldBe("Friend of NEBA");
    }

    [Fact(DisplayName = "Maps all properties correctly from dto to response")]
    public void HallOfFameInductionDto_ToResponseModel_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        const int year = 2020;
        Name bowlerName = NameFactory.Create(firstName: "Jane", lastName: "Smith");
        StoredFile photo = StoredFileFactory.Create();
        Uri photoUri = new Uri("https://storage.example.com/photo.jpg");
        IReadOnlyCollection<HallOfFameCategory> categories = [HallOfFameCategory.SuperiorPerformance];

        HallOfFameInductionDto dto = HallOfFameInductionDtoFactory.Create(
            year: year,
            bowlerName: bowlerName,
            photo: photo,
            categories: categories);
        dto = dto with { PhotoUri = photoUri };

        // Act
        HallOfFameInductionResponse response = dto.ToResponseModel();

        // Assert
        response.Year.ShouldBe(year);
        response.BowlerName.ShouldBe("Jane Smith");
        response.PhotoUrl.ShouldBe(photoUri);
        response.Categories.Count.ShouldBe(1);
        response.Categories.Single().ShouldBe("Superior Performance");
    }
}
