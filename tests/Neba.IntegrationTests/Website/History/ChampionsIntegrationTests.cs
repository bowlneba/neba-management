using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Neba.Contracts;
using Neba.Contracts.History.Champions;
using Neba.Domain.Bowlers;
using Neba.IntegrationTests.Infrastructure;
using Neba.Tests;

namespace Neba.IntegrationTests.Website.History;

public sealed class ChampionsIntegrationTests
    : IntegrationTestBase
{
    [Fact]
    public async Task GetBowlerTitleCounts_ShouldReturnExpectedResults()
    {
        // Arrange
        await ResetDatabaseAsync();

        int bowlersWithTitlesCount = 0;

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(200, 1963);
            context.Bowlers.AddRange(seedBowlers);
            await context.SaveChangesAsync();

            bowlersWithTitlesCount = seedBowlers.Count(b => b.Titles.Count > 0);
        });

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/history/champions", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<GetBowlerTitleCountsResponse>? result =
            await response.Content.ReadFromJsonAsync<CollectionResponse<GetBowlerTitleCountsResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(bowlersWithTitlesCount);
        result.TotalItems.ShouldBe(result.Items.Count);
    }

    [Fact]
    public async Task GetBowlerTitles_ShouldReturnNotFound_WhenBowlerDoesNotExist()
    {
        // Arrange
        await ResetDatabaseAsync();

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(50, 1980);
            context.Bowlers.AddRange(seedBowlers);
            await context.SaveChangesAsync();
        });

        BowlerId nonExistantBowlerId = BowlerId.New();

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri($"/history/champions/{nonExistantBowlerId}", UriKind.Relative));

        // Assert
        Dictionary<string, object> metadata = new()
        {
            { "bowlerId", nonExistantBowlerId }
        };

        await response.ShouldBeNotFound("Bowler.NotFound", "Bowler was not found.", metadata);
    }

    [Fact]
    public async Task GetBowlerTitles_ShouldReturnResults_WhenBowlerExists()
    {
        // Arrange
        await ResetDatabaseAsync();

        BowlerId seedBowlerId = BowlerId.Empty;

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(50, 1980);

            seedBowlerId = seedBowlers.First(bowler => bowler.Titles.Count > 0).Id;

            context.Bowlers.AddRange(seedBowlers);
            await context.SaveChangesAsync();
        });

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri($"/history/champions/{seedBowlerId}", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        ApiResponse<GetBowlerTitlesResponse>? result =
            await response.Content.ReadFromJsonAsync<ApiResponse<GetBowlerTitlesResponse>>();

        result.ShouldNotBeNull();
        result.Data.BowlerId.ShouldBe(seedBowlerId.Value);
    }
}
