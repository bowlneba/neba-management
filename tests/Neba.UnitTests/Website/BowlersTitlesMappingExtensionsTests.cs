using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Tests.Website;
using Neba.Website.Application.Bowlers.BowlerTitles;
using Neba.Website.Application.Tournaments;
using Neba.Website.Contracts.Bowlers;
using Neba.Website.Contracts.Titles;
using Neba.Website.Domain.Tournaments;
using Neba.Website.Endpoints.Bowlers;

namespace Neba.UnitTests.Website;

public sealed class BowlersTitlesMappingExtensionsTests
{
    [Fact(DisplayName = "Maps bowler ID from BowlerTitleDto to response model")]
    public void BowlerTitleDto_ToResponseModel_ShouldMapBowlerId()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create(bowlerId: bowlerId);

        // Act
        TitleResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerId.ShouldBe(bowlerId);
    }

    [Fact(DisplayName = "Maps bowler name from BowlerTitleDto to response model")]
    public void BowlerTitleDto_ToResponseModel_ShouldMapBowlerName()
    {
        // Arrange
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create();

        // Act
        TitleResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerName.ShouldBe(dto.BowlerName.ToDisplayName());
    }

    [Fact(DisplayName = "Maps tournament month from BowlerTitleDto to response model")]
    public void BowlerTitleDto_ToResponseModel_ShouldMapTournamentMonth()
    {
        // Arrange
        Month tournamentMonth = Month.May;
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create(tournamentMonth: tournamentMonth);

        // Act
        TitleResponse response = dto.ToResponseModel();

        // Assert
        response.TournamentMonth.ShouldBe(tournamentMonth);
    }

    [Fact(DisplayName = "Maps tournament year from BowlerTitleDto to response model")]
    public void BowlerTitleDto_ToResponseModel_ShouldMapTournamentYear()
    {
        // Arrange
        const int tournamentYear = 2022;
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create(tournamentYear: tournamentYear);

        // Act
        TitleResponse response = dto.ToResponseModel();

        // Assert
        response.TournamentYear.ShouldBe(tournamentYear);
    }

    [Fact(DisplayName = "Maps tournament type from BowlerTitleDto to response model")]
    public void BowlerTitleDto_ToResponseModel_ShouldMapTournamentType()
    {
        // Arrange
        TournamentType tournamentType = TournamentType.Doubles;
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create(tournamentType: tournamentType);

        // Act
        TitleResponse response = dto.ToResponseModel();

        // Assert
        response.TournamentType.ShouldBe(tournamentType.Name);
    }

    [Fact(DisplayName = "Maps bowler ID from BowlerTitlesDto correctly")]
    public void BowlerTitlesDto_ToResponseModel_MapsBowlerIdCorrectly()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        BowlerTitlesDto dto = BowlerTitlesDtoFactory.Create(bowlerId: bowlerId);

        // Act
        BowlerTitlesResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerId.ShouldBe(bowlerId);
    }

    [Fact(DisplayName = "Maps bowler name from BowlerTitlesDto correctly")]
    public void BowlerTitlesDto_ToResponseModel_MapsBowlerNameCorrectly()
    {
        // Arrange
        BowlerTitlesDto dto = BowlerTitlesDtoFactory.Create();

        // Act
        BowlerTitlesResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerName.ShouldBe(dto.BowlerName.ToDisplayName());
    }

    [Fact(DisplayName = "Maps titles collection from BowlerTitlesDto correctly")]
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

    [Fact(DisplayName = "Maps bowler ID from BowlerTitleSummaryDto correctly")]
    public void BowlerTitleSummaryDto_ToResponseModel_MapsBowlerIdCorrectly()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        BowlerTitleSummaryDto dto = BowlerTitleSummaryDtoFactory.Create(bowlerId: bowlerId);

        // Act
        TitleSummaryResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerId.ShouldBe(bowlerId);
    }

    [Fact(DisplayName = "Maps bowler name from BowlerTitleSummaryDto correctly")]
    public void BowlerTitleSummaryDto_ToResponseModel_MapsBowlerNameCorrectly()
    {
        // Arrange
        BowlerTitleSummaryDto dto = BowlerTitleSummaryDtoFactory.Create();

        // Act
        TitleSummaryResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerName.ShouldBe(dto.BowlerName.ToDisplayName());
    }

    [Fact(DisplayName = "Maps title count from BowlerTitleSummaryDto correctly")]
    public void BowlerTitleSummaryDto_ToResponseModel_MapsTitleCountCorrectly()
    {
        // Arrange
        const int titleCount = 5;
        BowlerTitleSummaryDto dto = BowlerTitleSummaryDtoFactory.Create(titleCount: titleCount);

        // Act
        TitleSummaryResponse response = dto.ToResponseModel();

        // Assert
        response.TitleCount.ShouldBe(titleCount);
    }
}
