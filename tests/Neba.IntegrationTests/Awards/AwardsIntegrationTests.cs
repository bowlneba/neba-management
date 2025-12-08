using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Neba.Contracts;
using Neba.Contracts.Website.Awards;
using Neba.Domain.Bowlers;
using Neba.IntegrationTests.Infrastructure;
using Neba.Tests;

namespace Neba.IntegrationTests.Awards;

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
            => await context.Bowlers.AsNoTracking().SelectMany(b => b.BowlerOfTheYears).CountAsync());

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
}
