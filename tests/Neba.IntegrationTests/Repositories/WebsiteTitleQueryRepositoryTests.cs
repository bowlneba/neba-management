using Microsoft.EntityFrameworkCore;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Domain.Bowlers;
using Neba.Infrastructure.Database.Website;
using Neba.Infrastructure.Database.Website.Repositories;
using Neba.Tests;

namespace Neba.IntegrationTests.Repositories;

/// <summary>
/// Collection fixture to share a single WebsiteDatabase instance across all tests in this class.
/// </summary>
[CollectionDefinition(nameof(WebsiteTitleQueryRepositoryTests))]
public sealed class WebsiteTitleQueryRepositoryTestsFixture
    : ICollectionFixture<WebsiteDatabase>;

[Collection(nameof(WebsiteTitleQueryRepositoryTests))]
public sealed class WebsiteTitleQueryRepositoryTests(WebsiteDatabase database) : IAsyncLifetime
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

        var repository = new WebsiteTitleQueryRepository(websiteDbContext);

        // Act
        IReadOnlyCollection<BowlerTitleDto> result
            = await repository.ListTitlesAsync(CancellationToken.None);

        // Assert
        result.Count.ShouldBe(expectedTitleCount);

        var seedBowlerResult = result.Where(dto => dto.BowlerId == seedBowler.Id).ToList();
        seedBowlerResult.Count.ShouldBe(seedBowler.Titles.Count);
        seedBowlerResult.ShouldAllBe(dto => dto.BowlerName == seedBowler.Name.ToDisplayName());
    }
}
