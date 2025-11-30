using Neba.Application.Bowlers.BowlerTitles;
using Neba.Contracts.Website.Bowlers;
using Neba.Domain.Bowlers;
using Neba.Tests;
using Neba.Web.Server.History.Champions;

namespace Neba.UnitTests.Mapping.Ui;

public sealed class ChampionsMappingExtensionsTests
{
    [Fact]
    public void BowlerTitleSummaryResponse_ToViewModel_ShouldMapBowlerId()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        BowlerTitleSummaryResponse response = BowlerTitleSummaryResponseFactory.Create(bowlerId: bowlerId.Value, titleCount: 0);

        // Act
        BowlerTitleSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlerId.ShouldBe(bowlerId.Value);
    }

    [Fact]
    public void BowlerTitleSummaryResponse_ToViewModel_ShouldMapBowlerName()
    {
        // Arrange
        const string bowlerName = "John Doe";
        BowlerTitleSummaryResponse response = BowlerTitleSummaryResponseFactory.Create(bowlerName: bowlerName, titleCount: 0);

        // Act
        BowlerTitleSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlerName.ShouldBe(bowlerName);
    }

    [Fact]
    public void BowlerTitleSummaryResponse_ToViewModel_ShouldMapTitleCount()
    {
        // Arrange
        const int titleCount = 3;
        BowlerTitleSummaryResponse response = BowlerTitleSummaryResponseFactory.Create(titleCount: titleCount);

        // Act
        BowlerTitleSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.TitleCount.ShouldBe(titleCount);
    }

    [Fact]
    public void BowlerTitlesResponse_ToViewModel_ShouldMapBowlerName()
    {
        // Arrange
        const string bowlerName = "Jane Smith";
        BowlerTitlesResponse response = BowlerTitlesResponseFactory.Create(bowlerName: bowlerName, titleCount: 0);

        // Act
        BowlerTitlesViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlerName.ShouldBe(bowlerName);
    }

    [Fact]
    public void BowlerTitlesResponse_ToViewModel_ShouldMapTitlesInOrder()
    {
        // Arrange
        IReadOnlyCollection<TitleResponse> titles = new[]
        {
            TitlesResponseFactory.Create(month: Month.March, year: 2020),
            TitlesResponseFactory.Create(month: Month.January, year: 2019),
            TitlesResponseFactory.Create(month: Month.February, year: 2020)
        };

        BowlerTitlesResponse response = BowlerTitlesResponseFactory.Create(
            bowlerName: "Jane Smith",
            titles: titles);

        // Act
        BowlerTitlesViewModel viewModel = response.ToViewModel();

        // Assert
        List<TitleViewModel> mappedTitles = [.. viewModel.Titles];
        mappedTitles.Count.ShouldBe(3);
        mappedTitles[0].TournamentDate.ShouldBe("Jan 2019");
        mappedTitles[1].TournamentDate.ShouldBe("Feb 2020");
        mappedTitles[2].TournamentDate.ShouldBe("Mar 2020");
    }
}
