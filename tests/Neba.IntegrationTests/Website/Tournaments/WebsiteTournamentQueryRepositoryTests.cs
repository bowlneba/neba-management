using Microsoft.EntityFrameworkCore;
using Neba.Tests.Infrastructure;
using Neba.Tests.Website;
using Neba.Website.Application.Tournaments;
using Neba.Website.Domain.BowlingCenters;
using Neba.Website.Domain.Tournaments;
using Neba.Website.Infrastructure.Database;
using Neba.Website.Infrastructure.Database.Repositories;

namespace Neba.IntegrationTests.Website.Tournaments;

[Trait("Category", "Integration")]
[Trait("Component", "Website.Tournaments")]
public sealed class WebsiteTournamentQueryRepositoryTests : IAsyncLifetime
{
    private DatabaseContainer _database = null!;

    /// <summary>
    /// Called before each test class - initializes a fresh database container.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        _database = new DatabaseContainer();
        await _database.InitializeAsync();
    }

    /// <summary>
    /// Called after all tests complete - disposes the database container.
    /// </summary>
    public async ValueTask DisposeAsync()
        => await _database.DisposeAsync();

    [Fact(DisplayName = "Returns tournaments starting on or after the specified date")]
    public async Task ListTournamentsAfterDateAsync_WithTournamentsAfterDate_ReturnsTournamentsAfterDate()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(_database.ConnectionString)
                .Options);

        // Create seed bowling centers for tournament relationships
        IReadOnlyCollection<BowlingCenter> seedBowlingCenters = BowlingCenterFactory.Bogus(5, seed: 2024);
        await websiteDbContext.BowlingCenters.AddRangeAsync(seedBowlingCenters);
        await websiteDbContext.SaveChangesAsync();

        // Create tournaments before the cutoff date
        Tournament tournamentBeforeCutoff1 = TournamentFactory.Create(
            name: "Early Tournament 1",
            startDate: new DateOnly(2024, 1, 10),
            endDate: new DateOnly(2024, 1, 10),
            bowlingCenter: seedBowlingCenters.First());

        Tournament tournamentBeforeCutoff2 = TournamentFactory.Create(
            name: "Early Tournament 2",
            startDate: new DateOnly(2024, 1, 14),
            endDate: new DateOnly(2024, 1, 14),
            bowlingCenter: seedBowlingCenters.First());

        // Create tournaments on or after the cutoff date
        Tournament tournamentOnCutoff = TournamentFactory.Create(
            name: "Tournament On Cutoff",
            startDate: new DateOnly(2024, 1, 15),
            endDate: new DateOnly(2024, 1, 15),
            bowlingCenter: seedBowlingCenters.ElementAt(1));

        Tournament tournamentAfterCutoff1 = TournamentFactory.Create(
            name: "Late Tournament 1",
            startDate: new DateOnly(2024, 2, 1),
            endDate: new DateOnly(2024, 2, 1),
            bowlingCenter: seedBowlingCenters.ElementAt(2));

        Tournament tournamentAfterCutoff2 = TournamentFactory.Create(
            name: "Late Tournament 2",
            startDate: new DateOnly(2024, 3, 15),
            endDate: new DateOnly(2024, 3, 15),
            bowlingCenter: seedBowlingCenters.ElementAt(3));

        // Create additional tournaments using Bogus
        IReadOnlyCollection<Tournament> bogusTournaments = TournamentFactory.Bogus(
            count: 10,
            seedBowlingCenters: seedBowlingCenters,
            seed: 2024);

        await websiteDbContext.Tournaments.AddAsync(tournamentBeforeCutoff1);
        await websiteDbContext.Tournaments.AddAsync(tournamentBeforeCutoff2);
        await websiteDbContext.Tournaments.AddAsync(tournamentOnCutoff);
        await websiteDbContext.Tournaments.AddAsync(tournamentAfterCutoff1);
        await websiteDbContext.Tournaments.AddAsync(tournamentAfterCutoff2);
        await websiteDbContext.Tournaments.AddRangeAsync(bogusTournaments);
        await websiteDbContext.SaveChangesAsync();

        var repository = new WebsiteTournamentQueryRepository(websiteDbContext);
        DateOnly cutoffDate = new DateOnly(2024, 1, 15);

        // Act
        IReadOnlyCollection<TournamentSummaryDto> result
            = await repository.ListTournamentsAfterDateAsync(cutoffDate, CancellationToken.None);

        // Assert
        // Tournaments before cutoff should NOT be in results
        result.ShouldNotContain(dto => dto.Name == tournamentBeforeCutoff1.Name);
        result.ShouldNotContain(dto => dto.Name == tournamentBeforeCutoff2.Name);

        // Tournaments on or after cutoff SHOULD be in results
        result.ShouldContain(dto => dto.Name == tournamentOnCutoff.Name);
        result.ShouldContain(dto => dto.Name == tournamentAfterCutoff1.Name);
        result.ShouldContain(dto => dto.Name == tournamentAfterCutoff2.Name);

        // Verify tournaments are ordered by start date
        DateOnly[] startDates = result.Select(dto => dto.StartDate).ToArray();
        startDates.ShouldBe(startDates.OrderBy(d => d).ToArray());

        // Count bogus tournaments that meet the criteria
        int bogusTournamentsAfterDate = bogusTournaments.Count(t => t.StartDate >= cutoffDate);
        int expectedCount = 3 + bogusTournamentsAfterDate; // 3 explicit tournaments

        result.Count.ShouldBe(expectedCount);
    }

    [Fact(DisplayName = "Returns all tournaments in the specified year")]
    public async Task ListTournamentsInYearAsync_WithTournamentsInYear_ReturnsTournamentsInYear()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(_database.ConnectionString)
                .Options);

        // Create seed bowling centers for tournament relationships
        IReadOnlyCollection<BowlingCenter> seedBowlingCenters = BowlingCenterFactory.Bogus(5, seed: 2025);
        await websiteDbContext.BowlingCenters.AddRangeAsync(seedBowlingCenters);
        await websiteDbContext.SaveChangesAsync();

        // Create tournaments in year 2023
        Tournament tournament2023_1 = TournamentFactory.Create(
            name: "Tournament 2023-1",
            startDate: new DateOnly(2023, 3, 15),
            endDate: new DateOnly(2023, 3, 15),
            bowlingCenter: seedBowlingCenters.First());

        // Create tournaments in year 2024
        Tournament tournament2024_1 = TournamentFactory.Create(
            name: "Tournament 2024-1",
            startDate: new DateOnly(2024, 1, 10),
            endDate: new DateOnly(2024, 1, 10),
            bowlingCenter: seedBowlingCenters.ElementAt(1));

        Tournament tournament2024_2 = TournamentFactory.Create(
            name: "Tournament 2024-2",
            startDate: new DateOnly(2024, 6, 15),
            endDate: new DateOnly(2024, 6, 15),
            bowlingCenter: seedBowlingCenters.ElementAt(2));

        Tournament tournament2024_3 = TournamentFactory.Create(
            name: "Tournament 2024-3",
            startDate: new DateOnly(2024, 12, 31),
            endDate: new DateOnly(2024, 12, 31),
            bowlingCenter: seedBowlingCenters.ElementAt(3));

        // Create tournaments in year 2025
        Tournament tournament2025_1 = TournamentFactory.Create(
            name: "Tournament 2025-1",
            startDate: new DateOnly(2025, 2, 1),
            endDate: new DateOnly(2025, 2, 1),
            bowlingCenter: seedBowlingCenters.ElementAt(4));

        // Create additional tournaments using Bogus
        IReadOnlyCollection<Tournament> bogusTournaments = TournamentFactory.Bogus(
            count: 15,
            seedBowlingCenters: seedBowlingCenters,
            seed: 2025);

        await websiteDbContext.Tournaments.AddAsync(tournament2023_1);
        await websiteDbContext.Tournaments.AddAsync(tournament2024_1);
        await websiteDbContext.Tournaments.AddAsync(tournament2024_2);
        await websiteDbContext.Tournaments.AddAsync(tournament2024_3);
        await websiteDbContext.Tournaments.AddAsync(tournament2025_1);
        await websiteDbContext.Tournaments.AddRangeAsync(bogusTournaments);
        await websiteDbContext.SaveChangesAsync();

        var repository = new WebsiteTournamentQueryRepository(websiteDbContext);
        const int targetYear = 2024;

        // Act
        IReadOnlyCollection<TournamentSummaryDto> result
            = await repository.ListTournamentsInYearAsync(targetYear, CancellationToken.None);

        // Assert
        // Tournaments from other years should NOT be in results
        result.ShouldNotContain(dto => dto.Name == tournament2023_1.Name);
        result.ShouldNotContain(dto => dto.Name == tournament2025_1.Name);

        // Tournaments from 2024 SHOULD be in results
        result.ShouldContain(dto => dto.Name == tournament2024_1.Name);
        result.ShouldContain(dto => dto.Name == tournament2024_2.Name);
        result.ShouldContain(dto => dto.Name == tournament2024_3.Name);

        // Verify all returned tournaments are from the target year
        result.ShouldAllBe(dto => dto.StartDate.Year == targetYear);

        // Verify tournaments are ordered by start date
        DateOnly[] startDates = result.Select(dto => dto.StartDate).ToArray();
        startDates.ShouldBe(startDates.OrderBy(d => d).ToArray());

        // Count bogus tournaments that meet the criteria
        int bogusTournamentsInYear = bogusTournaments.Count(t => t.StartDate.Year == targetYear);
        int expectedCount = 3 + bogusTournamentsInYear; // 3 explicit tournaments

        result.Count.ShouldBe(expectedCount);
    }

    [Fact(DisplayName = "Returns empty collection when no tournaments exist after date")]
    public async Task ListTournamentsAfterDateAsync_WithNoTournamentsAfterDate_ReturnsEmptyCollection()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(_database.ConnectionString)
                .Options);

        // Create seed bowling centers for tournament relationships
        IReadOnlyCollection<BowlingCenter> seedBowlingCenters = BowlingCenterFactory.Bogus(3, seed: 2026);
        await websiteDbContext.BowlingCenters.AddRangeAsync(seedBowlingCenters);
        await websiteDbContext.SaveChangesAsync();

        // Create only tournaments before the cutoff date
        Tournament tournamentBeforeCutoff = TournamentFactory.Create(
            name: "Old Tournament",
            startDate: new DateOnly(2020, 1, 1),
            endDate: new DateOnly(2020, 1, 1),
            bowlingCenter: seedBowlingCenters.First());

        await websiteDbContext.Tournaments.AddAsync(tournamentBeforeCutoff);
        await websiteDbContext.SaveChangesAsync();

        var repository = new WebsiteTournamentQueryRepository(websiteDbContext);
        DateOnly cutoffDate = new DateOnly(2025, 1, 1);

        // Act
        IReadOnlyCollection<TournamentSummaryDto> result
            = await repository.ListTournamentsAfterDateAsync(cutoffDate, CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Returns empty collection when no tournaments exist in year")]
    public async Task ListTournamentsInYearAsync_WithNoTournamentsInYear_ReturnsEmptyCollection()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(_database.ConnectionString)
                .Options);

        // Create seed bowling centers for tournament relationships
        IReadOnlyCollection<BowlingCenter> seedBowlingCenters = BowlingCenterFactory.Bogus(3, seed: 2027);
        await websiteDbContext.BowlingCenters.AddRangeAsync(seedBowlingCenters);
        await websiteDbContext.SaveChangesAsync();

        // Create tournaments in different years
        Tournament tournament2023 = TournamentFactory.Create(
            name: "Tournament 2023",
            startDate: new DateOnly(2023, 6, 1),
            endDate: new DateOnly(2023, 6, 1),
            bowlingCenter: seedBowlingCenters.First());

        Tournament tournament2025 = TournamentFactory.Create(
            name: "Tournament 2025",
            startDate: new DateOnly(2025, 6, 1),
            endDate: new DateOnly(2025, 6, 1),
            bowlingCenter: seedBowlingCenters.ElementAt(1));

        await websiteDbContext.Tournaments.AddAsync(tournament2023);
        await websiteDbContext.Tournaments.AddAsync(tournament2025);
        await websiteDbContext.SaveChangesAsync();

        websiteDbContext.ChangeTracker.Clear();

        var repository = new WebsiteTournamentQueryRepository(websiteDbContext);
        const int targetYear = 2024;

        // Act
        IReadOnlyCollection<TournamentSummaryDto> result
            = await repository.ListTournamentsInYearAsync(targetYear, CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Correctly maps all tournament properties to DTO including related entities")]
    public async Task ListTournamentsAfterDateAsync_WithTournament_MapsAllPropertiesCorrectly()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(_database.ConnectionString)
                .Options);

        // Create seed bowling center
        BowlingCenter bowlingCenter = BowlingCenterFactory.Create(
            name: "Elite Lanes Bowling Center");
        await websiteDbContext.BowlingCenters.AddAsync(bowlingCenter);
        await websiteDbContext.SaveChangesAsync();

        // Create tournament with all properties including lane pattern
        LanePattern lanePattern = LanePatternFactory.Create(
            lengthCategory: Domain.Tournaments.PatternLengthCategory.LongPattern);
        Tournament tournament = TournamentFactory.Create(
            name: "Championship Tournament",
            startDate: new DateOnly(2024, 5, 20),
            endDate: new DateOnly(2024, 5, 22),
            bowlingCenter: bowlingCenter,
            tournamentType: Domain.Tournaments.TournamentType.Doubles,
            lanePattern: lanePattern);

        await websiteDbContext.Tournaments.AddAsync(tournament);
        await websiteDbContext.SaveChangesAsync();

        var repository = new WebsiteTournamentQueryRepository(websiteDbContext);

        // Act
        IReadOnlyCollection<TournamentSummaryDto> result
            = await repository.ListTournamentsAfterDateAsync(new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        result.Count.ShouldBe(1);
        TournamentSummaryDto dto = result.Single();

        // Verify all properties are mapped correctly
        dto.Id.ShouldBe(tournament.Id);
        dto.Name.ShouldBe(tournament.Name);
        dto.BowlingCenterId.ShouldBe(bowlingCenter.Id);
        dto.BowlingCenterName.ShouldBe(bowlingCenter.Name);
        dto.StartDate.ShouldBe(tournament.StartDate);
        dto.EndDate.ShouldBe(tournament.EndDate);
        dto.TournamentType.ShouldBe(tournament.TournamentType);
        dto.PatternLengthCategory.ShouldBe(lanePattern.LengthCategory);
        dto.ThumbnailUrl.ShouldBeNull();
    }

    [Fact(DisplayName = "Maps null PatternLengthCategory when LanePattern is null")]
    public async Task ListTournamentsAfterDateAsync_WithoutLanePattern_MapsNullPatternLengthCategory()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(_database.ConnectionString)
                .Options);

        // Create seed bowling center
        BowlingCenter bowlingCenter = BowlingCenterFactory.Create(
            name: "Test Bowling Center");
        await websiteDbContext.BowlingCenters.AddAsync(bowlingCenter);
        await websiteDbContext.SaveChangesAsync();

        // Create tournament without lane pattern
        Tournament tournament = TournamentFactory.Create(
            name: "House Shot Tournament",
            startDate: new DateOnly(2024, 6, 1),
            endDate: new DateOnly(2024, 6, 1),
            bowlingCenter: bowlingCenter,
            lanePattern: null);

        await websiteDbContext.Tournaments.AddAsync(tournament);
        await websiteDbContext.SaveChangesAsync();

        var repository = new WebsiteTournamentQueryRepository(websiteDbContext);

        // Act
        IReadOnlyCollection<TournamentSummaryDto> result
            = await repository.ListTournamentsAfterDateAsync(new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        result.Count.ShouldBe(1);
        TournamentSummaryDto dto = result.Single();

        dto.PatternLengthCategory.ShouldBeNull();
    }
}
