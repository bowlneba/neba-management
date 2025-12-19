using Microsoft.EntityFrameworkCore;
using Neba.Tests.Website;
using Neba.Website.Application.Awards.BowlerOfTheYear;
using Neba.Website.Application.Awards.HighAverage;
using Neba.Website.Application.Awards.HighBlock;
using Neba.Website.Domain.Awards;
using Neba.Website.Domain.Bowlers;
using Neba.Website.Infrastructure.Database;
using Neba.Website.Infrastructure.Database.Repositories;

namespace Neba.IntegrationTests.Website.Awards;

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

        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(50, 1960);
        await websiteDbContext.Bowlers.AddRangeAsync(seedBowlers);

        await websiteDbContext.SaveChangesAsync();

        var expectedAwards = await websiteDbContext.SeasonAwards
            .AsNoTracking()
            .Where(award => award.AwardType == SeasonAwardType.BowlerOfTheYear)
            .Select(award => new
            {
                award.Id,
                award.BowlerId,
                BowlerName = award.Bowler.Name.ToDisplayName(),
                award.Season,
                award.BowlerOfTheYearCategory
            })
            .ToListAsync(TestContext.Current.CancellationToken);

        WebsiteAwardQueryRepository repository = new(websiteDbContext);

        // Act
        IReadOnlyCollection<BowlerOfTheYearAwardDto> result
            = await repository.ListBowlerOfTheYearAwardsAsync(CancellationToken.None);

        // Assert
        result.Count.ShouldBe(expectedAwards.Count);

        foreach (var dto in result)
        {
            var expectedAward = expectedAwards.First(a => a.Id == dto.Id);

            dto.Id.ShouldBe(expectedAward.Id);
            dto.BowlerId.ShouldBe(expectedAward.BowlerId);
            dto.BowlerName.ShouldBe(expectedAward.BowlerName);
            dto.Season.ShouldBe(expectedAward.Season);
            dto.Category.ShouldBe(expectedAward.BowlerOfTheYearCategory);
        }
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

        var expectedAwards = await websiteDbContext.SeasonAwards
            .AsNoTracking()
            .Where(award => award.AwardType == SeasonAwardType.High5GameBlock)
            .Select(award => new
            {
                award.Id,
                BowlerName = award.Bowler.Name.ToDisplayName(),
                award.Season,
                HighBlockScore = award.HighBlockScore ?? -1
            })
            .ToListAsync(TestContext.Current.CancellationToken);

        WebsiteAwardQueryRepository repository = new(websiteDbContext);

        // Act
        IReadOnlyCollection<HighBlockAwardDto> result
            = await repository.ListHigh5GameBlockAwardsAsync(CancellationToken.None);

        // Assert
        result.Count.ShouldBe(expectedAwards.Count);

        foreach (var dto in result)
        {
            var expectedAward = expectedAwards.First(a => a.Id == dto.Id);

            dto.Id.ShouldBe(expectedAward.Id);
            dto.BowlerName.ShouldBe(expectedAward.BowlerName);
            dto.Season.ShouldBe(expectedAward.Season);
            dto.Score.ShouldBe(expectedAward.HighBlockScore);
        }
    }

    [Fact]
    public async Task ListHighAverageAwardsAsync_ShouldReturnAllAwards()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(database.ConnectionString)
                .Options);

        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(50, 1980);
        await websiteDbContext.Bowlers.AddRangeAsync(seedBowlers);

        await websiteDbContext.SaveChangesAsync();

        var expectedAwards = await websiteDbContext.SeasonAwards
            .AsNoTracking()
            .Where(award => award.AwardType == SeasonAwardType.HighAverage)
            .Select(award => new
            {
                award.Id,
                BowlerName = award.Bowler.Name.ToDisplayName(),
                award.Season,
                Average = award.Average ?? -1,
                Games = award.SeasonTotalGames,
                Tournaments = award.Tournaments
            })
            .ToListAsync(TestContext.Current.CancellationToken);

        WebsiteAwardQueryRepository repository = new(websiteDbContext);

        // Act
        IReadOnlyCollection<HighAverageAwardDto> result
            = await repository.ListHighAverageAwardsAsync(CancellationToken.None);

        // Assert
        result.Count.ShouldBe(expectedAwards.Count);

        foreach (var dto in result)
        {
            var expectedAward = expectedAwards.First(a => a.Id == dto.Id);

            dto.Id.ShouldBe(expectedAward.Id);
            dto.BowlerName.ShouldBe(expectedAward.BowlerName);
            dto.Season.ShouldBe(expectedAward.Season);
            dto.Average.ShouldBe(expectedAward.Average);
            dto.Games.ShouldBe(expectedAward.Games);
            dto.Tournaments.ShouldBe(expectedAward.Tournaments);
        }
    }
}
