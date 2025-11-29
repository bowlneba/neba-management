using Neba.Contracts.Website.Bowlers;
using Neba.Domain.Bowlers;
using Neba.Tests;
using Neba.Web.Server.History.Champions;

namespace Neba.UnitTests.Mapping.Ui;

public sealed class ChampionsMappingExtensionsTests
{
    [Fact]
    public void GetBowlerTitleCountsResponse_ToViewModel_ShouldMapBowlerId()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        GetBowlerTitleCountsResponse response = GetBowlerTitleCountsResponseFactory.Create(bowlerId: bowlerId.Value);

        // Act
        BowlerTitleCountViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlerId.ShouldBe(bowlerId.Value);
    }

    [Fact]
    public void GetBowlerTitleCountsResponse_ToViewModel_ShouldMapBowlerName()
    {
        // Arrange
        const string bowlerName = "Jane Smith";
        GetBowlerTitleCountsResponse response = GetBowlerTitleCountsResponseFactory.Create(bowlerName: bowlerName);

        // Act
        BowlerTitleCountViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlerName.ShouldBe(bowlerName);
    }

    [Fact]
    public void GetBowlerTitleCountsResponse_ToViewModel_ShouldMapTitleCount()
    {
        // Arrange
        const int titleCount = 7;
        GetBowlerTitleCountsResponse response = GetBowlerTitleCountsResponseFactory.Create(titleCount: titleCount);

        // Act
        BowlerTitleCountViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.Titles.ShouldBe(titleCount);
    }

    [Fact]
    public void GetBowlerTitlesResponse_ToViewModel_ShouldMapBowlerName()
    {
        // Arrange
        const string bowlerName = "Jane Smith";
        GetBowlerTitlesResponse response = GetBowlerTitlesResponseFactory.Create(bowlerName: bowlerName, titleCount: 0);

        // Act
        BowlerTitlesViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlerName.ShouldBe(bowlerName);
    }

    [Fact]
    public void GetBowlerTitlesResponse_ToViewModel_ShouldMapTitlesInOrder()
    {
        // Arrange
        IReadOnlyCollection<TitlesResponse> titles = new[]
        {
            TitlesResponseFactory.Create(month: Month.March, year: 2020),
            TitlesResponseFactory.Create(month: Month.January, year: 2019),
            TitlesResponseFactory.Create(month: Month.February, year: 2020)
        };

        GetBowlerTitlesResponse response = GetBowlerTitlesResponseFactory.Create(
            bowlerName: "Jane Smith",
            titles: titles);

        // Act
        BowlerTitlesViewModel viewModel = response.ToViewModel();

        // Assert
        List<TitlesViewModel> mappedTitles = [.. viewModel.Titles];
        mappedTitles.Count.ShouldBe(3);
        mappedTitles[0].TournamentDate.ShouldBe("Jan 2019");
        mappedTitles[1].TournamentDate.ShouldBe("Feb 2020");
        mappedTitles[2].TournamentDate.ShouldBe("Mar 2020");
    }
}
