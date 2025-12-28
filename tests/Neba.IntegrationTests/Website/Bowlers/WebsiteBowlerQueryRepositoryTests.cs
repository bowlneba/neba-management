using Microsoft.EntityFrameworkCore;
using Neba.Domain.Identifiers;
using Neba.Tests.Website;
using Neba.Website.Application.Bowlers.BowlerTitles;
using Neba.Website.Application.Tournaments;
using Neba.Website.Domain.Bowlers;
using Neba.Website.Domain.Tournaments;
using Neba.Website.Infrastructure.Database;
using Neba.Website.Infrastructure.Database.Repositories;

namespace Neba.IntegrationTests.Website.Bowlers;

public sealed class WebsiteBowlerQueryRepositoryTests : IAsyncLifetime
{
    private WebsiteDatabase _database = null!;

    /// <summary>
    /// Called before each test class - initializes a fresh database container.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        _database = new WebsiteDatabase();
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

        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100, 1963);
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

        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100, 1963);
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
                .OrderBy(title => title.Year)
                .ThenBy(title => title.Month)
                .ThenBy(title => title.TournamentType)
                .ElementAt(i);

            dto.Month.ShouldBe(expectedTitle.Month);
            dto.Year.ShouldBe(expectedTitle.Year);
            dto.TournamentType.ShouldBe(expectedTitle.TournamentType);
        }

        result.Titles.ShouldAllBe(dto => dto.Month != null);
        result.Titles.ShouldAllBe(dto => dto.Year > 0);
        result.Titles.ShouldAllBe(dto => dto.TournamentType != null);
    }
}
