using Neba.Application.Bowlers.BowlerTitles;
using Neba.Domain.Bowlers;
using Neba.Api.Endpoints.Website.Bowlers;
using Neba.Tests;
using Neba.Domain.Tournaments;
using Neba.Contracts.Website.Bowlers;

namespace Neba.UnitTests.Mapping.Api;

public sealed class BowlersTitlesMappingExtensionsTests
{
    [Fact]
    public void BowlerTitleDto_ToResponseModel_ShouldMapBowlerId()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create(bowlerId: bowlerId);

        // Act
        BowlerTitleResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerId.ShouldBe(bowlerId.Value);
    }

    [Fact]
    public void BowlerTitleDto_ToResponseModel_ShouldMapBowlerName()
    {
        // Arrange
        const string bowlerName = "John Doe";
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create(bowlerName: bowlerName);

        // Act
        BowlerTitleResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerName.ShouldBe(bowlerName);
    }

    [Fact]
    public void BowlerTitleDto_ToResponseModel_ShouldMapTournamentMonth()
    {
        // Arrange
        Month tournamentMonth = Month.May;
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create(tournamentMonth: tournamentMonth);

        // Act
        BowlerTitleResponse response = dto.ToResponseModel();

        // Assert
        response.TournamentMonth.ShouldBe(tournamentMonth);
    }

    [Fact]
    public void BowlerTitleDto_ToResponseModel_ShouldMapTournamentYear()
    {
        // Arrange
        const int tournamentYear = 2022;
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create(tournamentYear: tournamentYear);

        // Act
        BowlerTitleResponse response = dto.ToResponseModel();

        // Assert
        response.TournamentYear.ShouldBe(tournamentYear);
    }

    [Fact]
    public void BowlerTitleDto_ToResponseModel_ShouldMapTournamentType()
    {
        // Arrange
        TournamentType tournamentType = TournamentType.Doubles;
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create(tournamentType: tournamentType);

        // Act
        BowlerTitleResponse response = dto.ToResponseModel();

        // Assert
        response.TournamentType.ShouldBe(tournamentType.Name);
    }

    [Fact]
    public void BowlerTitlesDto_ToResponseModel_MapsBowlerIdCorrectly()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        BowlerTitlesDto dto = BowlerTitlesDtoFactory.Create(bowlerId: bowlerId);

        // Act
        BowlerTitlesResponse response = dto.ToResponseModel();

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
        BowlerTitlesResponse response = dto.ToResponseModel();

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
        BowlerTitlesResponse response = dto.ToResponseModel();

        // Assert
        response.Titles.Count.ShouldBe(titles.Length);

        for (int i = 0; i < titles.Length; i++)
        {
            response.Titles.ElementAt(i).Month.ShouldBe(titles[i].Month);
            response.Titles.ElementAt(i).Year.ShouldBe(titles[i].Year);
            response.Titles.ElementAt(i).TournamentType.ShouldBe(titles[i].TournamentType.Name);
        }
    }

    [Fact]
    public void BowlerTitleSummaryDto_ToResponseModel_MapsBowlerIdCorrectly()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        BowlerTitleSummaryDto dto = BowlerTitleSummaryDtoFactory.Create(bowlerId: bowlerId);

        // Act
        BowlerTitleSummaryResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerId.ShouldBe(bowlerId.Value);
    }

    [Fact]
    public void BowlerTitleSummaryDto_ToResponseModel_MapsBowlerNameCorrectly()
    {
        // Arrange
        const string bowlerName = "Alice Johnson";
        BowlerTitleSummaryDto dto = BowlerTitleSummaryDtoFactory.Create(bowlerName: bowlerName);

        // Act
        BowlerTitleSummaryResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerName.ShouldBe(bowlerName);
    }

    [Fact]
    public void BowlerTitleSummaryDto_ToResponseModel_MapsTitleCountCorrectly()
    {
        // Arrange
        const int titleCount = 5;
        BowlerTitleSummaryDto dto = BowlerTitleSummaryDtoFactory.Create(titleCount: titleCount);

        // Act
        BowlerTitleSummaryResponse response = dto.ToResponseModel();

        // Assert
        response.TitleCount.ShouldBe(titleCount);
    }
}
