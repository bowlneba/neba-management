using System.Net;
using System.Net.Http.Json;
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
    public async Task GetBowlerTitleCounts_ReturnsExpectedResults()
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
}
