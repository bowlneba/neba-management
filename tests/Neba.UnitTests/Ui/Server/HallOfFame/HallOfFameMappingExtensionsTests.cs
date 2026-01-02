using Neba.Tests.Website;
using Neba.Web.Server.HallOfFame;
using Neba.Website.Contracts.Awards;

namespace Neba.UnitTests.Ui.Server.HallOfFame;
[Trait("Category", "Unit")]
[Trait("Component", "Ui.Server.HallOfFame")]

public sealed class HallOfFameMappingExtensionsTests
{
    [Fact(DisplayName = "Maps bowler name from HallOfFameInductionResponse to view model")]
    public void HallOfFameInductionResponse_ToViewModel_ShouldMapBowlerName()
    {
        // Arrange
        const string bowlerName = "John Doe";
        HallOfFameInductionResponse response = HallOfFameInductionResponseFactory.Create(bowlerName: bowlerName);

        // Act
        HallOfFameInductionViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlerName.ShouldBe(bowlerName);
    }

    [Fact(DisplayName = "Maps induction year from HallOfFameInductionResponse to view model")]
    public void HallOfFameInductionResponse_ToViewModel_ShouldMapInductionYear()
    {
        // Arrange
        const int year = 2024;
        HallOfFameInductionResponse response = HallOfFameInductionResponseFactory.Create(year: year);

        // Act
        HallOfFameInductionViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.InductionYear.ShouldBe(year);
    }

    [Fact(DisplayName = "Maps categories from HallOfFameInductionResponse to view model")]
    public void HallOfFameInductionResponse_ToViewModel_ShouldMapCategories()
    {
        // Arrange
        IReadOnlyCollection<string> categories = ["Superior Performance", "Meritorious Service"];
        HallOfFameInductionResponse response = HallOfFameInductionResponseFactory.Create(categories: categories);

        // Act
        HallOfFameInductionViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.Categories.ShouldNotBeNull();
        viewModel.Categories.Count.ShouldBe(2);
        viewModel.Categories.ShouldContain("Superior Performance");
        viewModel.Categories.ShouldContain("Meritorious Service");
    }

    [Fact(DisplayName = "Maps single category correctly to view model")]
    public void HallOfFameInductionResponse_ToViewModel_WithSingleCategory_ShouldMapCorrectly()
    {
        // Arrange
        IReadOnlyCollection<string> categories = ["Friend of NEBA"];
        HallOfFameInductionResponse response = HallOfFameInductionResponseFactory.Create(categories: categories);

        // Act
        HallOfFameInductionViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.Categories.Count.ShouldBe(1);
        viewModel.Categories.Single().ShouldBe("Friend of NEBA");
    }

    [Fact(DisplayName = "Maps photo URL when present in response")]
    public void HallOfFameInductionResponse_ToViewModel_WithPhotoUrl_ShouldMapPhotoUrl()
    {
        // Arrange
        Uri photoUrl = new Uri("https://storage.example.com/photo.jpg");
        HallOfFameInductionResponse response = HallOfFameInductionResponseFactory.Create(photoUrl: photoUrl);

        // Act
        HallOfFameInductionViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.PhotoUrl.ShouldBe(photoUrl);
    }

    [Fact(DisplayName = "Maps null photo URL when not present in response")]
    public void HallOfFameInductionResponse_ToViewModel_WithNullPhotoUrl_ShouldMapNullPhotoUrl()
    {
        // Arrange
        HallOfFameInductionResponse response = HallOfFameInductionResponseFactory.Create(photoUrl: null);

        // Act
        HallOfFameInductionViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.PhotoUrl.ShouldBeNull();
    }

    [Fact(DisplayName = "Maps all properties from response to view model correctly")]
    public void HallOfFameInductionResponse_ToViewModel_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        const string bowlerName = "Jane Smith";
        const int year = 2020;
        Uri photoUrl = new Uri("https://storage.example.com/jane.jpg");
        IReadOnlyCollection<string> categories = ["Superior Performance", "Meritorious Service"];

        HallOfFameInductionResponse response = HallOfFameInductionResponseFactory.Create(
            bowlerName: bowlerName,
            year: year,
            photoUrl: photoUrl,
            categories: categories);

        // Act
        HallOfFameInductionViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlerName.ShouldBe(bowlerName);
        viewModel.InductionYear.ShouldBe(year);
        viewModel.PhotoUrl.ShouldBe(photoUrl);
        viewModel.Categories.Count.ShouldBe(2);
        viewModel.Categories.ShouldContain("Superior Performance");
        viewModel.Categories.ShouldContain("Meritorious Service");
    }
}
