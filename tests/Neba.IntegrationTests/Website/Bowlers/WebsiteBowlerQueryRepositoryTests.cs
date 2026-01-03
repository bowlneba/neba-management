using Microsoft.EntityFrameworkCore;
using Neba.Domain.Identifiers;
using Neba.Tests.Infrastructure;
using Neba.Tests.Website;
using Neba.Website.Application.Bowlers.BowlerTitles;
using Neba.Website.Application.Tournaments;
using Neba.Website.Domain.Bowlers;
using Neba.Website.Domain.BowlingCenters;
using Neba.Website.Domain.Tournaments;
using Neba.Website.Infrastructure.Database;
using Neba.Website.Infrastructure.Database.Repositories;

namespace Neba.IntegrationTests.Website.Bowlers;

[Trait("Category", "Integration")]
[Trait("Component", "Website.Bowlers")]

public sealed class WebsiteBowlerQueryRepositoryTests : IAsyncLifetime
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

    [Fact]
    public async Task GetBowlerTitlesAsync_BowlerId_ShouldReturnNull_WhenBowlerDoesNotExist()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(_database.ConnectionString)
                .Options);

        IReadOnlyCollection<BowlingCenter> seedBowlingCenters = BowlingCenterFactory.Bogus(10, 1960);
        await websiteDbContext.BowlingCenters.AddRangeAsync(seedBowlingCenters);
        await websiteDbContext.SaveChangesAsync();

        IReadOnlyCollection<Tournament> seedTournaments = TournamentFactory.Bogus(500, seedBowlingCenters, 1963);
        await websiteDbContext.Tournaments.AddRangeAsync(seedTournaments);
        await websiteDbContext.SaveChangesAsync();

        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100, seedTournaments, 1963);
        await websiteDbContext.Bowlers.AddRangeAsync(seedBowlers);
        await websiteDbContext.SaveChangesAsync();

        var repository = new WebsiteBowlerQueryRepository(websiteDbContext);
        BowlerId nonExistentBowlerId = BowlerId.New();

        // Act
        BowlerTitlesDto? result
            = await repository.GetBowlerTitlesAsync(nonExistentBowlerId, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetBowlerTitlesAsync_BowlerId_ShouldReturnCorrectTitles_ForExistingBowler()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(_database.ConnectionString)
                .Options);

        IReadOnlyCollection<BowlingCenter> seedBowlingCenters = BowlingCenterFactory.Bogus(10, 1960);
        await websiteDbContext.BowlingCenters.AddRangeAsync(seedBowlingCenters);
        await websiteDbContext.SaveChangesAsync();

        IReadOnlyCollection<Tournament> seedTournaments = TournamentFactory.Bogus(500, seedBowlingCenters, 1963);
        await websiteDbContext.Tournaments.AddRangeAsync(seedTournaments);
        await websiteDbContext.SaveChangesAsync();

        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100, seedTournaments, 1963);
        await websiteDbContext.Bowlers.AddRangeAsync(seedBowlers);
        await websiteDbContext.SaveChangesAsync();

        Bowler seedBowler = seedBowlers.First(bowler => bowler.Titles.Count > 3);

        var repository = new WebsiteBowlerQueryRepository(websiteDbContext);

        // Act
        BowlerTitlesDto? result
            = await repository.GetBowlerTitlesAsync(seedBowler.Id, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.BowlerId.ShouldBe(seedBowler.Id);
        result.BowlerName.ShouldBe(seedBowler.Name);
        result.Titles.Count.ShouldBe(seedBowler.Titles.Count);

        for (int i = 0; i < result.Titles.Count; i++)
        {
            TitleDto dto = result.Titles.ElementAt(i);
            Title expectedTitle = seedBowler.Titles
                .OrderBy(title => title.Tournament.EndDate)
                .ThenBy(title => title.Tournament.TournamentType)
                .ElementAt(i);

            dto.TournamentDate.Month.ShouldBe(expectedTitle.Tournament.EndDate.Month);
            dto.TournamentDate.Year.ShouldBe(expectedTitle.Tournament.EndDate.Year);
            dto.TournamentType.ShouldBe(expectedTitle.Tournament.TournamentType);
        }

        result.Titles.ShouldAllBe(dto => dto.TournamentDate.Month > 0);
        result.Titles.ShouldAllBe(dto => dto.TournamentDate.Year > 0);
        result.Titles.ShouldAllBe(dto => dto.TournamentType != null);
    }
}
