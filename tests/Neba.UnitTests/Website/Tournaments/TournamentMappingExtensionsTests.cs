using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Tests.Website;
using Neba.Website.Application.Tournaments;
using Neba.Website.Domain.BowlingCenters;
using Neba.Website.Domain.Tournaments;
using Neba.Website.Infrastructure.Database;

namespace Neba.UnitTests.Website.Tournaments;

[Trait("Category", "Unit")]
[Trait("Component", "Website.Tournaments")]
public sealed class TournamentMappingExtensionsTests
{
    private static Tournament CreateTournamentWithNullBowlingCenter()
    {
        // Use reflection to set the internal BowlingCenter property to null
        Tournament tournament = TournamentFactory.Create(bowlingCenterId: BowlingCenterId.New());

        // Set BowlingCenter to null
        typeof(Tournament)
            .GetProperty("BowlingCenter", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(tournament, null);

        // Set BowlingCenterId to null
        typeof(Tournament)
            .GetProperty("BowlingCenterId")!
            .SetValue(tournament, null);

        return tournament;
    }

    [Fact(DisplayName = "Maps Id to TournamentSummaryDto")]
    public void ToTournamentSummaryDto_ShouldMapId()
    {
        // Arrange
        TournamentId id = TournamentId.New();
        Tournament tournament = TournamentFactory.Create(
            id: id,
            bowlingCenterId: BowlingCenterId.New());

        // Act
        TournamentSummaryDto result = tournament.ToTournamentSummaryDto();

        // Assert
        result.Id.ShouldBe(id);
    }

    [Fact(DisplayName = "Maps Name to TournamentSummaryDto")]
    public void ToTournamentSummaryDto_ShouldMapName()
    {
        // Arrange
        const string name = "Spring Classic Tournament";
        Tournament tournament = TournamentFactory.Create(
            name: name,
            bowlingCenterId: BowlingCenterId.New());

        // Act
        TournamentSummaryDto result = tournament.ToTournamentSummaryDto();

        // Assert
        result.Name.ShouldBe(name);
    }

    [Fact(DisplayName = "Maps BowlingCenterId to TournamentSummaryDto")]
    public void ToTournamentSummaryDto_ShouldMapBowlingCenterId()
    {
        // Arrange
        BowlingCenterId bowlingCenterId = BowlingCenterId.New();
        Tournament tournament = TournamentFactory.Create(bowlingCenterId: bowlingCenterId);

        // Act
        TournamentSummaryDto result = tournament.ToTournamentSummaryDto();

        // Assert
        result.BowlingCenterId.ShouldBe(bowlingCenterId);
    }

    [Fact(DisplayName = "Maps null BowlingCenterId to TournamentSummaryDto")]
    public void ToTournamentSummaryDto_WithNullBowlingCenterId_ShouldMapNullBowlingCenterId()
    {
        // Arrange
        Tournament tournament = CreateTournamentWithNullBowlingCenter();

        // Act
        TournamentSummaryDto result = tournament.ToTournamentSummaryDto();

        // Assert
        result.BowlingCenterId.ShouldBeNull();
    }

    [Fact(DisplayName = "Maps BowlingCenterName from BowlingCenter to TournamentSummaryDto")]
    public void ToTournamentSummaryDto_ShouldMapBowlingCenterName()
    {
        // Arrange
        const string bowlingCenterName = "Elite Lanes Bowling Center";
        BowlingCenter bowlingCenter = BowlingCenterFactory.Create(name: bowlingCenterName);
        Tournament tournament = TournamentFactory.Create(
            id: null,
            name: null,
            startDate: null,
            endDate: null,
            bowlingCenter: bowlingCenter,
            tournamentType: null,
            lanePattern: null,
            websiteId: null,
            applicationId: null);

        // Act
        TournamentSummaryDto result = tournament.ToTournamentSummaryDto();

        // Assert
        result.BowlingCenterName.ShouldBe(bowlingCenterName);
    }

    [Fact(DisplayName = "Maps null BowlingCenterName when BowlingCenter is null")]
    public void ToTournamentSummaryDto_WithNullBowlingCenter_ShouldMapNullBowlingCenterName()
    {
        // Arrange
        Tournament tournament = CreateTournamentWithNullBowlingCenter();

        // Act
        TournamentSummaryDto result = tournament.ToTournamentSummaryDto();

        // Assert
        result.BowlingCenterName.ShouldBeNull();
    }

    [Fact(DisplayName = "Maps StartDate to TournamentSummaryDto")]
    public void ToTournamentSummaryDto_ShouldMapStartDate()
    {
        // Arrange
        DateOnly startDate = new(2024, 3, 15);
        Tournament tournament = TournamentFactory.Create(
            startDate: startDate,
            bowlingCenterId: BowlingCenterId.New());

        // Act
        TournamentSummaryDto result = tournament.ToTournamentSummaryDto();

        // Assert
        result.StartDate.ShouldBe(startDate);
    }

    [Fact(DisplayName = "Maps EndDate to TournamentSummaryDto")]
    public void ToTournamentSummaryDto_ShouldMapEndDate()
    {
        // Arrange
        DateOnly endDate = new(2024, 3, 17);
        Tournament tournament = TournamentFactory.Create(
            endDate: endDate,
            bowlingCenterId: BowlingCenterId.New());

        // Act
        TournamentSummaryDto result = tournament.ToTournamentSummaryDto();

        // Assert
        result.EndDate.ShouldBe(endDate);
    }

    [Fact(DisplayName = "Maps TournamentType to TournamentSummaryDto")]
    public void ToTournamentSummaryDto_ShouldMapTournamentType()
    {
        // Arrange
        TournamentType tournamentType = TournamentType.Doubles;
        Tournament tournament = TournamentFactory.Create(
            tournamentType: tournamentType,
            bowlingCenterId: BowlingCenterId.New());

        // Act
        TournamentSummaryDto result = tournament.ToTournamentSummaryDto();

        // Assert
        result.TournamentType.ShouldBe(tournamentType);
    }

    [Fact(DisplayName = "Maps PatternLengthCategory from LanePattern to TournamentSummaryDto")]
    public void ToTournamentSummaryDto_ShouldMapPatternLengthCategory()
    {
        // Arrange
        PatternLengthCategory lengthCategory = PatternLengthCategory.ShortPattern;
        LanePattern lanePattern = LanePatternFactory.Create(lengthCategory: lengthCategory);
        Tournament tournament = TournamentFactory.Create(
            lanePattern: lanePattern,
            bowlingCenterId: BowlingCenterId.New());

        // Act
        TournamentSummaryDto result = tournament.ToTournamentSummaryDto();

        // Assert
        result.PatternLengthCategory.ShouldBe(lengthCategory);
    }

    [Fact(DisplayName = "Maps null PatternLengthCategory when LanePattern is null")]
    public void ToTournamentSummaryDto_WithNullLanePattern_ShouldMapNullPatternLengthCategory()
    {
        // Arrange
        Tournament tournament = TournamentFactory.Create(
            lanePattern: null,
            bowlingCenterId: BowlingCenterId.New());

        // Act
        TournamentSummaryDto result = tournament.ToTournamentSummaryDto();

        // Assert
        result.PatternLengthCategory.ShouldBeNull();
    }

    [Fact(DisplayName = "Maps null ThumbnailUrl to TournamentSummaryDto")]
    public void ToTournamentSummaryDto_ShouldMapNullThumbnailUrl()
    {
        // Arrange
        Tournament tournament = TournamentFactory.Create(bowlingCenterId: BowlingCenterId.New());

        // Act
        TournamentSummaryDto result = tournament.ToTournamentSummaryDto();

        // Assert
        result.ThumbnailUrl.ShouldBeNull();
    }
}
