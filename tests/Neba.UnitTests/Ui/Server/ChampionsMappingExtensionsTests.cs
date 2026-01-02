using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Tests.Website;
using Neba.Web.Server.History.Champions;
using Neba.Website.Contracts.Bowlers;
using Neba.Website.Contracts.Titles;

namespace Neba.UnitTests.Ui.Server;

[Trait("Category", "Unit")]
[Trait("Component", "Ui.Server")]

public sealed class ChampionsMappingExtensionsTests
{
    [Fact(DisplayName = "Maps bowler ID from BowlerTitleSummaryResponse to view model")]
    public void BowlerTitleSummaryResponse_ToViewModel_ShouldMapBowlerId()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        TitleSummaryResponse response = TitleSummaryResponseFactory.Create(bowlerId: bowlerId, titleCount: 0);

        // Act
        BowlerTitleSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlerId.ShouldBe(bowlerId);
    }

    [Fact(DisplayName = "Maps bowler name from BowlerTitleSummaryResponse to view model")]
    public void BowlerTitleSummaryResponse_ToViewModel_ShouldMapBowlerName()
    {
        // Arrange
        const string bowlerName = "John Doe";
        TitleSummaryResponse response = TitleSummaryResponseFactory.Create(bowlerName: bowlerName, titleCount: 0);

        // Act
        BowlerTitleSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlerName.ShouldBe(bowlerName);
    }

    [Fact(DisplayName = "Maps title count from BowlerTitleSummaryResponse to view model")]
    public void BowlerTitleSummaryResponse_ToViewModel_ShouldMapTitleCount()
    {
        // Arrange
        const int titleCount = 3;
        TitleSummaryResponse response = TitleSummaryResponseFactory.Create(titleCount: titleCount);

        // Act
        BowlerTitleSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.TitleCount.ShouldBe(titleCount);
    }

    [Fact(DisplayName = "Maps bowler name from BowlerTitlesResponse to view model")]
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

    [Fact(DisplayName = "Maps titles from BowlerTitlesResponse to view model in chronological order")]
    public void BowlerTitlesResponse_ToViewModel_ShouldMapTitlesInOrder()
    {
        // Arrange
        IReadOnlyCollection<BowlerTitleResponse> titles = new[]
        {
            BowlerTitleResponseFactory.Create(month: Month.March, year: 2020),
            BowlerTitleResponseFactory.Create(month: Month.January, year: 2019),
            BowlerTitleResponseFactory.Create(month: Month.February, year: 2020)
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
