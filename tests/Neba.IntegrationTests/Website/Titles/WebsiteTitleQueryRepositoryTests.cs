using Microsoft.EntityFrameworkCore;
using Neba.Tests.Infrastructure;
using Neba.Tests.Website;
using Neba.Website.Application.Bowlers.BowlerTitles;
using Neba.Website.Domain.Bowlers;
using Neba.Website.Domain.BowlingCenters;
using Neba.Website.Domain.Tournaments;
using Neba.Website.Infrastructure.Database;
using Neba.Website.Infrastructure.Database.Repositories;

namespace Neba.IntegrationTests.Website.Titles;

[Trait("Category", "Integration")]
[Trait("Component", "Website.Titles")]

public sealed class WebsiteTitleQueryRepositoryTests : IAsyncLifetime
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
    public async Task GetAllBowlerTitlesAsync_ShouldReturnAllTitles()
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

        int expectedTitleCount = seedBowlers.Sum(bowler => bowler.Titles.Count);
        Bowler seedBowler = seedBowlers.First(bowler => bowler.Titles.Count > 0);

        var repository = new WebsiteTitleQueryRepository(websiteDbContext);

        // Act
        IReadOnlyCollection<BowlerTitleDto> result
            = await repository.ListTitlesAsync(CancellationToken.None);

        // Assert
        result.Count.ShouldBe(expectedTitleCount);

        var seedBowlerResult = result.Where(dto => dto.BowlerId == seedBowler.Id).ToList();
        seedBowlerResult.Count.ShouldBe(seedBowler.Titles.Count);
        seedBowlerResult.ShouldAllBe(dto => dto.BowlerName == seedBowler.Name);

        foreach (BowlerTitleDto? dto in seedBowlerResult)
        {
            Title expectedTitle = seedBowler.Titles.First(t =>
                t.Tournament.EndDate.Month == dto.TournamentDate.Month &&
                t.Tournament.EndDate.Year == dto.TournamentDate.Year &&
                t.Tournament.TournamentType == dto.TournamentType);

            dto.BowlerId.ShouldBe(seedBowler.Id);
            dto.BowlerName.ShouldBe(seedBowler.Name);
            dto.TournamentDate.Month.ShouldBe(expectedTitle.Tournament.EndDate.Month);
            dto.TournamentDate.Year.ShouldBe(expectedTitle.Tournament.EndDate.Year);
            dto.TournamentType.ShouldBe(expectedTitle.Tournament.TournamentType);
        }
    }

    [Fact]
    public async Task ListTitleSummariesAsync_ShouldReturnCorrectSummaries()
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

        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100, seedTournaments, 1965);
        await websiteDbContext.Bowlers.AddRangeAsync(seedBowlers);
        await websiteDbContext.SaveChangesAsync();

        var expectedSummaries = seedBowlers
            .Where(bowler => bowler.Titles.Count > 0)
            .Select(bowler => new
            {
                BowlerId = bowler.Id,
                BowlerName = bowler.Name,
                TitleCount = bowler.Titles.Count
            })
            .ToList();

        var repository = new WebsiteTitleQueryRepository(websiteDbContext);

        // Act
        IReadOnlyCollection<BowlerTitleSummaryDto> result
            = await repository.ListTitleSummariesAsync(CancellationToken.None);

        // Assert
        result.Count.ShouldBe(expectedSummaries.Count);

        foreach (BowlerTitleSummaryDto dto in result)
        {
            var expectedSummary = expectedSummaries.First(s => s.BowlerId == dto.BowlerId);

            dto.BowlerId.ShouldBe(expectedSummary.BowlerId);
            dto.BowlerName.ShouldBe(expectedSummary.BowlerName);
            dto.TitleCount.ShouldBe(expectedSummary.TitleCount);
        }

        result.ShouldAllBe(dto => dto.BowlerName != null);
        result.ShouldAllBe(dto => dto.TitleCount > 0);
    }
}
