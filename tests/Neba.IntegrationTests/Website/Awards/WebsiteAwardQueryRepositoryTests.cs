using Microsoft.EntityFrameworkCore;
using Neba.Domain.Awards;
using Neba.Tests.Website;
using Neba.Website.Application.Awards.BowlerOfTheYear;
using Neba.Website.Application.Awards.HighAverage;
using Neba.Website.Application.Awards.HighBlock;
using Neba.Website.Domain.Bowlers;
using Neba.Website.Infrastructure.Database;
using Neba.Website.Infrastructure.Database.Repositories;

namespace Neba.IntegrationTests.Website.Awards;

public sealed class WebsiteAwardQueryRepositoryTests : IAsyncLifetime
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
    public async Task ListBowlerOfTheYearAwardsAsync_ShouldReturnAllAwards()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(_database.ConnectionString)
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
                BowlerName = award.Bowler.Name,
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

        foreach (BowlerOfTheYearAwardDto dto in result)
        {
            var expectedAward = expectedAwards.First(a => a.Id == dto.Id);

            dto.Id.ShouldBe(expectedAward.Id);
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
                .UseNpgsql(_database.ConnectionString)
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
                BowlerName = award.Bowler.Name,
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

        foreach (HighBlockAwardDto dto in result)
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
                .UseNpgsql(_database.ConnectionString)
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
                BowlerName = award.Bowler.Name,
                award.Season,
                Average = award.Average ?? -1,
                Games = award.SeasonTotalGames,
                award.Tournaments
            })
            .ToListAsync(TestContext.Current.CancellationToken);

        WebsiteAwardQueryRepository repository = new(websiteDbContext);

        // Act
        IReadOnlyCollection<HighAverageAwardDto> result
            = await repository.ListHighAverageAwardsAsync(CancellationToken.None);

        // Assert
        result.Count.ShouldBe(expectedAwards.Count);

        foreach (HighAverageAwardDto dto in result)
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
