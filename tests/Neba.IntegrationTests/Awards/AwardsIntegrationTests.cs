using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Neba.Contracts;
using Neba.Contracts.Website.Awards;
using Neba.Domain.Awards;
using Neba.Domain.Bowlers;
using Neba.IntegrationTests.Infrastructure;
using Neba.Tests;

namespace Neba.IntegrationTests.Awards;

[Collection(nameof(Infrastructure.Collections.AwardsIntegrationTest))]
public sealed class AwardsIntegrationTests
    : ApiTestsBase
{
    [Fact]
    public async Task ListBowlerOfTheYearAwards_ShouldReturnExpectedResults()
    {
        // Arrange
        await ResetDatabaseAsync();

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100);
            context.Bowlers.AddRange(seedBowlers);
            await context.SaveChangesAsync();
        });

        int totalBowlerOfTheYearWins = await ExecuteAsync(async context
            => await context.Bowlers.AsNoTracking()
                .SelectMany(b => b.SeasonAwards)
                .Where(sa => sa.AwardType == SeasonAwardType.BowlerOfTheYear)
                .CountAsync());

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/awards/bowler-of-the-year", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<BowlerOfTheYearResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<BowlerOfTheYearResponse>>();

        result.ShouldNotBeNull();

        result.Items.Count.ShouldBe(totalBowlerOfTheYearWins);
        result.TotalItems.ShouldBe(totalBowlerOfTheYearWins);
    }

    [Fact]
    public async Task ListBowlerOfTheYearAwards_WithNoData_ShouldReturnEmptyCollection()
    {
        // Arrange
        await ResetDatabaseAsync();

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/awards/bowler-of-the-year", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<BowlerOfTheYearResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<BowlerOfTheYearResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(0);
        result.TotalItems.ShouldBe(0);
    }

    [Fact]
    public async Task ListBowlerOfTheYearAwards_ShouldIncludeAllRequiredFields()
    {
        // Arrange
        await ResetDatabaseAsync();

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(50);
            context.Bowlers.AddRange(seedBowlers);
            await context.SaveChangesAsync();
        });

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/awards/bowler-of-the-year", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<BowlerOfTheYearResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<BowlerOfTheYearResponse>>();

        result.ShouldNotBeNull();

        // Verify each award has all required fields populated
        foreach (var award in result.Items)
        {
            award.BowlerName.ShouldNotBeNullOrWhiteSpace();
            award.Season.ShouldNotBeNullOrWhiteSpace();
            award.Category.ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task ListHighBlockAwards_ShouldReturnExpectedResults()
    {
        // Arrange
        await ResetDatabaseAsync();

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100);
            context.Bowlers.AddRange(seedBowlers);
            await context.SaveChangesAsync();
        });

        int totalHighBlockWins = await ExecuteAsync(async context
            => await context.Bowlers.AsNoTracking()
                .SelectMany(b => b.SeasonAwards)
                .Where(sa => sa.AwardType == SeasonAwardType.High5GameBlock)
                .CountAsync());

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/awards/high-block", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<HighBlockAwardResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<HighBlockAwardResponse>>();

        result.ShouldNotBeNull();

        result.Items.Count.ShouldBe(totalHighBlockWins);
        result.TotalItems.ShouldBe(totalHighBlockWins);
    }

    [Fact]
    public async Task ListHighBlockAwards_WithNoData_ShouldReturnEmptyCollection()
    {
        // Arrange
        await ResetDatabaseAsync();

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/awards/high-block", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<HighBlockAwardResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<HighBlockAwardResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(0);
        result.TotalItems.ShouldBe(0);
    }

    [Fact]
    public async Task ListHighBlockAwards_ShouldIncludeAllRequiredFields()
    {
        // Arrange
        await ResetDatabaseAsync();

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(50);
            context.Bowlers.AddRange(seedBowlers);
            await context.SaveChangesAsync();
        });

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/awards/high-block", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<HighBlockAwardResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<HighBlockAwardResponse>>();

        result.ShouldNotBeNull();

        // Verify each award has all required fields populated
        foreach (var award in result.Items)
        {
            award.Id.ShouldNotBe(Ulid.Empty);
            award.BowlerName.ShouldNotBeNullOrWhiteSpace();
            award.Season.ShouldNotBeNullOrWhiteSpace();
            award.Score.ShouldBeGreaterThan(0);
        }
    }

    [Fact]
    public async Task ListHighAverageAwards_ShouldReturnExpectedResults()
    {
        // Arrange
        await ResetDatabaseAsync();

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100);
            context.Bowlers.AddRange(seedBowlers);
            await context.SaveChangesAsync();
        });

        int totalHighAverageWins = await ExecuteAsync(async context
            => await context.Bowlers.AsNoTracking()
                .SelectMany(b => b.SeasonAwards)
                .Where(sa => sa.AwardType == SeasonAwardType.HighAverage)
                .CountAsync());

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/awards/high-average", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<HighAverageAwardResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<HighAverageAwardResponse>>();

        result.ShouldNotBeNull();

        result.Items.Count.ShouldBe(totalHighAverageWins);
        result.TotalItems.ShouldBe(totalHighAverageWins);
    }

    [Fact]
    public async Task ListHighAverageAwards_WithNoData_ShouldReturnEmptyCollection()
    {
        // Arrange
        await ResetDatabaseAsync();

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/awards/high-average", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<HighAverageAwardResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<HighAverageAwardResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(0);
        result.TotalItems.ShouldBe(0);
    }

    [Fact]
    public async Task ListHighAverageAwards_ShouldIncludeAllRequiredFields()
    {
        // Arrange
        await ResetDatabaseAsync();

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(50);
            context.Bowlers.AddRange(seedBowlers);
            await context.SaveChangesAsync();
        });

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/awards/high-average", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<HighAverageAwardResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<HighAverageAwardResponse>>();

        result.ShouldNotBeNull();

        // Verify each award has all required fields populated
        foreach (var award in result.Items)
        {
            award.Id.ShouldNotBe(Ulid.Empty);
            award.BowlerName.ShouldNotBeNullOrWhiteSpace();
            award.Season.ShouldNotBeNullOrWhiteSpace();
            award.Average.ShouldBeGreaterThan(0);
        }
    }
}
