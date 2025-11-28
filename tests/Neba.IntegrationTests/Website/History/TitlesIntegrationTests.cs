using System.Net;
using System.Net.Http.Json;
using Neba.Contracts;
using Neba.Contracts.History.Titles;
using Neba.Domain.Bowlers;
using Neba.IntegrationTests.Infrastructure;
using Neba.Tests;

namespace Neba.IntegrationTests.Website.History;

public sealed class TitlesIntegrationTests
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
        HttpResponseMessage response = await httpClient.GetAsync(new Uri($"/history/titles", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<GetTitlesResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<GetTitlesResponse>>();

        result.ShouldNotBeNull();

        result.Items.Count.ShouldBe(totalTitles);
        result.TotalItems.ShouldBe(totalTitles);
    }
}
