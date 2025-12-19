using Neba.Contracts.Website.Bowlers;
using Neba.Contracts.Website.Titles;
using Neba.Domain;
using Neba.Domain.Identifiers;
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
        TitleSummaryResponse response = TitleSummaryResponseFactory.Create(bowlerId: bowlerId, titleCount: 0);

        // Act
        BowlerTitleSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlerId.ShouldBe(bowlerId);
    }

    [Fact]
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

    [Fact]
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
