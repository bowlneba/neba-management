using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Api.Endpoints.Website.History.Champions;
using Neba.Domain.Bowlers;
using Neba.Tests;
using Neba.Contracts.History.Champions;
using Neba.Domain.Tournaments;

namespace Neba.UnitTests.Mapping.Api;

public sealed class ChampionsMappingExtensionsTests
{
    [Fact]
    public void BowlerTitleCountDto_ToResponseModel_MapsBowlerIdCorrectly()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        BowlerTitleCountDto dto = BowlerTitleCountDtoFactory.Create(bowlerId: bowlerId);

        // Act
        GetBowlerTitleCountsResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerId.ShouldBe(bowlerId.Value);
    }

    [Fact]
    public void BowlerTitleCountDto_ToResponseModel_MapsBowlerNameCorrectly()
    {
        // Arrange
        const string bowlerName = "Jane Smith";
        BowlerTitleCountDto dto = BowlerTitleCountDtoFactory.Create(bowlerName: bowlerName);

        // Act
        GetBowlerTitleCountsResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerName.ShouldBe(bowlerName);
    }

    [Fact]
    public void BowlerTitleCountDto_ToResponseModel_MapsTitleCountCorrectly()
    {
        // Arrange
        const int titleCount = 5;
        BowlerTitleCountDto dto = BowlerTitleCountDtoFactory.Create(titleCount: titleCount);

        // Act
        GetBowlerTitleCountsResponse response = dto.ToResponseModel();

        // Assert
        response.TitleCount.ShouldBe(titleCount);
    }

    [Fact]
    public void BowlerTitlesDto_ToResponseModel_MapsBowlerIdCorrectly()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        BowlerTitlesDto dto = BowlerTitlesDtoFactory.Create(bowlerId: bowlerId);

        // Act
        GetBowlerTitlesResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerId.ShouldBe(bowlerId.Value);
    }

    [Fact]
    public void BowlerTitlesDto_ToResponseModel_MapsBowlerNameCorrectly()
    {
        // Arrange
        const string bowlerName = "Jane Smith";
        BowlerTitlesDto dto = BowlerTitlesDtoFactory.Create(bowlerName: bowlerName);

        // Act
        GetBowlerTitlesResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerName.ShouldBe(bowlerName);
    }

    [Fact]
    public void BowlerTitlesDto_ToResponseModel_MapsTitlesCorrectly()
    {
        // Arrange
        TitleDto[] titles =
        [
            TitleDtoFactory.Create(month: Month.January, year: 2020, tournamentType: TournamentType.Women),
            TitleDtoFactory.Create(month: Month.March, year: 2021, tournamentType: TournamentType.Senior)
        ];

        BowlerTitlesDto dto = BowlerTitlesDtoFactory.Create(titles: titles);

        // Act
        GetBowlerTitlesResponse response = dto.ToResponseModel();

        // Assert
        response.Titles.Count.ShouldBe(titles.Length);

        for (int i = 0; i < titles.Length; i++)
        {
            response.Titles.ElementAt(i).Month.ShouldBe(titles[i].Month);
            response.Titles.ElementAt(i).Year.ShouldBe(titles[i].Year);
            response.Titles.ElementAt(i).TournamentType.ShouldBe(titles[i].TournamentType.Name);
        }
    }
}
