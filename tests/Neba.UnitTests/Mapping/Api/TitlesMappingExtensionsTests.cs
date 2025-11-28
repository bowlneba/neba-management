using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Contracts.History.Titles;
using Neba.Domain.Bowlers;
using Neba.Api.Endpoints.Website.History.Titles;
using Neba.Tests;
using Neba.Domain.Tournaments;

namespace Neba.UnitTests.Mapping.Api;

public sealed class TitlesMappingExtensionsTests
{
    [Fact]
    public void ToResponseModel_ShouldMapBowlerId()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create(bowlerId: bowlerId);

        // Act
        GetTitlesResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerId.ShouldBe(bowlerId.Value);
    }

    [Fact]
    public void ToResponseModel_ShouldMapBowlerName()
    {
        // Arrange
        string bowlerName = "John Doe";
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create(bowlerName: bowlerName);

        // Act
        GetTitlesResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerName.ShouldBe(bowlerName);
    }

    [Fact]
    public void ToResponseModel_ShouldMapTournamentMonth()
    {
        // Arrange
        Month tournamentMonth = Month.May;
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create(tournamentMonth: tournamentMonth);

        // Act
        GetTitlesResponse response = dto.ToResponseModel();

        // Assert
        response.TournamentMonth.ShouldBe(tournamentMonth);
    }

    [Fact]
    public void ToResponseModel_ShouldMapTournamentYear()
    {
        // Arrange
        const int tournamentYear = 2022;
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create(tournamentYear: tournamentYear);

        // Act
        GetTitlesResponse response = dto.ToResponseModel();

        // Assert
        response.TournamentYear.ShouldBe(tournamentYear);
    }

    [Fact]
    public void ToResponseModel_ShouldMapTournamentType()
    {
        // Arrange
        TournamentType tournamentType = TournamentType.Doubles;
        BowlerTitleDto dto = BowlerTitleDtoFactory.Create(tournamentType: tournamentType);

        // Act
        GetTitlesResponse response = dto.ToResponseModel();

        // Assert
        response.TournamentType.ShouldBe(tournamentType.Name);
    }
}
