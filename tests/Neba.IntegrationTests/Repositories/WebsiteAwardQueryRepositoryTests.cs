using Microsoft.EntityFrameworkCore;
using Neba.Application.Awards;
using Neba.Domain.Awards;
using Neba.Domain.Bowlers;
using Neba.Infrastructure.Database.Website;
using Neba.Infrastructure.Database.Website.Repositories;
using Neba.Tests;

namespace Neba.IntegrationTests.Repositories;

[CollectionDefinition(nameof(WebsiteAwardQueryRepositoryTests))]
public sealed class WebsiteAwardQueryRepositoryTestsFixture
    : ICollectionFixture<WebsiteDatabase>;

[Collection(nameof(WebsiteAwardQueryRepositoryTests))]
public sealed class WebsiteAwardQueryRepositoryTests(WebsiteDatabase database) : IAsyncLifetime
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
    public async Task ListBowlerOfTheYearAwardsAsync_ShouldReturnAllAwards()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(database.ConnectionString)
                .Options);

        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(50, 1970);
        await websiteDbContext.Bowlers.AddRangeAsync(seedBowlers);

        await websiteDbContext.SaveChangesAsync();

        int bowlerOfTheYearAwardCount = await websiteDbContext.SeasonAwards
            .CountAsync(award => award.AwardType == SeasonAwardType.BowlerOfTheYear, TestContext.Current.CancellationToken);

        WebsiteAwardQueryRepository repository = new(websiteDbContext);

        // Act
        IReadOnlyCollection<BowlerOfTheYearDto> result
            = await repository.ListBowlerOfTheYearAwardsAsync(CancellationToken.None);

        // Assert
        result.Count.ShouldBe(bowlerOfTheYearAwardCount);
    }

    [Fact]
    public async Task ListHigh5GameBlockAwardsAsync_ShouldReturnAllAwards()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(database.ConnectionString)
                .Options);

        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(50, 1970);
        await websiteDbContext.Bowlers.AddRangeAsync(seedBowlers);

        await websiteDbContext.SaveChangesAsync();

        int highBlockAwardCount = await websiteDbContext.SeasonAwards
            .CountAsync(award => award.AwardType == Domain.Awards.SeasonAwardType.High5GameBlock, TestContext.Current.CancellationToken);

        WebsiteAwardQueryRepository repository = new(websiteDbContext);

        // Act
        IReadOnlyCollection<HighBlockAwardDto> result
            = await repository.ListHigh5GameBlockAwardsAsync(CancellationToken.None);

        // Assert
        result.Count.ShouldBe(highBlockAwardCount);
    }
}
