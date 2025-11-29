using Microsoft.EntityFrameworkCore;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Domain.Bowlers;
using Neba.Domain.Tournaments;
using Neba.Infrastructure.Database.Website;
using Neba.Infrastructure.Database.Website.Repositories;
using Neba.Tests;

namespace Neba.IntegrationTests.Repositories;

/// <summary>
/// Collection fixture to share a single WebsiteDatabase instance across all tests in this class.
/// </summary>
[CollectionDefinition(nameof(WebsiteBowlerQueryRepositoryTests))]
public sealed class WebsiteBowlerQueryRepositoryTestsFixture : ICollectionFixture<WebsiteDatabase>;

[Collection(nameof(WebsiteBowlerQueryRepositoryTests))]
public sealed class WebsiteBowlerQueryRepositoryTests(WebsiteDatabase database) : IAsyncLifetime
{
    /// <summary>
    /// Called before each test - resets the database to a clean state.
    /// </summary>
    public async ValueTask InitializeAsync()
        => await database.ResetAsync();

    /// <summary>
    /// Called after each test - no cleanup needed.
    /// </summary>
    public ValueTask DisposeAsync()
        => ValueTask.CompletedTask;

    [Fact]
    public async Task GetBowlerTitlesAsync_BowlerId_ShouldReturnNull_WhenBowlerDoesNotExist()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(database.ConnectionString)
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
                .UseNpgsql(database.ConnectionString)
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
        result.BowlerName.ShouldBe(seedBowler.Name.ToDisplayName());
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
    }

    [Fact]
    public async Task GetAllBowlerTitlesAsync_ShouldReturnAllTitles()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(database.ConnectionString)
                .Options);

        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100, 1963);
        await websiteDbContext.Bowlers.AddRangeAsync(seedBowlers);
        await websiteDbContext.SaveChangesAsync();

        int expectedTitleCount = seedBowlers.Sum(bowler => bowler.Titles.Count);
        Bowler seedBowler = seedBowlers.First(bowler => bowler.Titles.Count > 0);

        var repository = new WebsiteBowlerQueryRepository(websiteDbContext);

        // Act
        IReadOnlyCollection<BowlerTitleDto> result
            = await repository.GetBowlerTitlesAsync(CancellationToken.None);

        // Assert
        result.Count.ShouldBe(expectedTitleCount);

        var seedBowlerResult = result.Where(dto => dto.BowlerId == seedBowler.Id).ToList();
        seedBowlerResult.Count.ShouldBe(seedBowler.Titles.Count);
        seedBowlerResult.ShouldAllBe(dto => dto.BowlerName == seedBowler.Name.ToDisplayName());
    }
}
