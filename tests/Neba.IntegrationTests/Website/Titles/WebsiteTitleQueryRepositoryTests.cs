using Microsoft.EntityFrameworkCore;
using Neba.Domain.Bowlers;
using Neba.Infrastructure.Database.Website;
using Neba.Infrastructure.Database.Website.Repositories;
using Neba.Tests.Website;
using Neba.Website.Application.Bowlers.BowlerTitles;
using Neba.Website.Domain.Bowlers;

namespace Neba.IntegrationTests.Website.Titles;

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

        foreach (var dto in seedBowlerResult)
        {
            var expectedTitle = seedBowler.Titles.First(t =>
                t.Month == dto.TournamentMonth &&
                t.Year == dto.TournamentYear &&
                t.TournamentType == dto.TournamentType);

            dto.BowlerId.ShouldBe(seedBowler.Id);
            dto.BowlerName.ShouldBe(seedBowler.Name.ToDisplayName());
            dto.TournamentMonth.ShouldBe(expectedTitle.Month);
            dto.TournamentYear.ShouldBe(expectedTitle.Year);
            dto.TournamentType.ShouldBe(expectedTitle.TournamentType);
        }
    }

    [Fact]
    public async Task ListTitleSummariesAsync_ShouldReturnCorrectSummaries()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(database.ConnectionString)
                .Options);

        IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100, 1965);
        await websiteDbContext.Bowlers.AddRangeAsync(seedBowlers);
        await websiteDbContext.SaveChangesAsync();

        var expectedSummaries = seedBowlers
            .Where(bowler => bowler.Titles.Count > 0)
            .Select(bowler => new
            {
                BowlerId = bowler.Id,
                BowlerName = bowler.Name.ToDisplayName(),
                TitleCount = bowler.Titles.Count
            })
            .ToList();

        var repository = new WebsiteTitleQueryRepository(websiteDbContext);

        // Act
        IReadOnlyCollection<BowlerTitleSummaryDto> result
            = await repository.ListTitleSummariesAsync(CancellationToken.None);

        // Assert
        result.Count.ShouldBe(expectedSummaries.Count);

        foreach (var dto in result)
        {
            var expectedSummary = expectedSummaries.First(s => s.BowlerId == dto.BowlerId);

            dto.BowlerId.ShouldBe(expectedSummary.BowlerId);
            dto.BowlerName.ShouldBe(expectedSummary.BowlerName);
            dto.TitleCount.ShouldBe(expectedSummary.TitleCount);
        }

        result.ShouldAllBe(dto => !string.IsNullOrEmpty(dto.BowlerName));
        result.ShouldAllBe(dto => dto.TitleCount > 0);
    }
}
