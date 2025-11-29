using System.Net;
using System.Net.Http.Json;
using Neba.Contracts;
using Neba.Contracts.Website.Bowlers;
using Neba.Domain.Bowlers;
using Neba.IntegrationTests.Infrastructure;
using Neba.Tests;

namespace Neba.IntegrationTests.Website.Bowlers;

public sealed class BowlersTitlesIntegrationTests
    : IntegrationTestBase
{
    [Fact]
    public async Task GetTitles_ShouldReturnExpectedResults()
    {
        // Arrange
        await ResetDatabaseAsync();

        int totalTitles = 0;

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100);
            context.Bowlers.AddRange(seedBowlers);
            await context.SaveChangesAsync();

            totalTitles = seedBowlers.Sum(b => b.Titles.Count);
        });

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/bowlers/titles", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<GetTitlesResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<GetTitlesResponse>>();

        result.ShouldNotBeNull();

        result.Items.Count.ShouldBe(totalTitles);
        result.TotalItems.ShouldBe(totalTitles);
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
        HttpResponseMessage response = await httpClient.GetAsync(new Uri($"/bowlers/{seedBowlerId}/titles", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        ApiResponse<GetBowlerTitlesResponse>? result =
            await response.Content.ReadFromJsonAsync<ApiResponse<GetBowlerTitlesResponse>>();

        result.ShouldNotBeNull();
        result.Data.BowlerId.ShouldBe(seedBowlerId.Value);
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

        BowlerId nonExistentBowlerId = BowlerId.New();

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri($"/bowlers/{nonExistentBowlerId}/titles", UriKind.Relative));

        // Assert
        Dictionary<string, object> metadata = new()
        {
            { "bowlerId", nonExistentBowlerId }
        };

        await response.ShouldBeNotFound("Bowler.NotFound", "Bowler was not found.", metadata);
    }
}
