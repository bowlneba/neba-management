using Microsoft.EntityFrameworkCore;
using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Domain.Bowlers;
using Neba.Infrastructure.Database.Website;
using Neba.Infrastructure.Database.Website.Repositories;
using Neba.Tests;

namespace Neba.UnitTests.Repositories;

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
    public async Task GetBowlerTitleCountsAsync_ShouldOnlyReturnBowlersWithTitles()
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

        // Act
        IReadOnlyCollection<BowlerTitleCountDto> result
            = await repository.GetBowlerTitleCountsAsync(CancellationToken.None);

        // Assert
        result.ShouldAllBe(dto => dto.TitleCount > 0);
    }
}
