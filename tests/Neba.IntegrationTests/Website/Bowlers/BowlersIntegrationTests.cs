using System.Net;
using System.Net.Http.Json;
using Neba.Contracts;
using Neba.Domain.Identifiers;
using Neba.IntegrationTests.Infrastructure;
using Neba.Tests.Website;
using Neba.Website.Contracts.Bowlers;
using Neba.Website.Domain.Bowlers;
using Neba.Website.Domain.BowlingCenters;
using Neba.Website.Domain.Tournaments;

namespace Neba.IntegrationTests.Website.Bowlers;

[Trait("Category", "Integration")]
[Trait("Component", "Website.Bowlers")]

public sealed class BowlersIntegrationTests
    : ApiTestsBase
{
    [Fact]
    public async Task GetBowlerTitles_ShouldReturnResults_WhenBowlerExists()
    {
        // Arrange
        BowlerId seedBowlerId = BowlerId.Empty;

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<BowlingCenter> seedBowlingCenters = BowlingCenterFactory.Bogus(10, 1960);
            await context.BowlingCenters.AddRangeAsync(seedBowlingCenters);
            await context.SaveChangesAsync();

            IReadOnlyCollection<Tournament> seedTournaments = TournamentFactory.Bogus(50, seedBowlingCenters, 1980);
            await context.Tournaments.AddRangeAsync(seedTournaments);
            await context.SaveChangesAsync();

            IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(50, 1980);
            await context.Bowlers.AddRangeAsync(seedBowlers);
            await context.SaveChangesAsync();

            IReadOnlyCollection<Title> seedTitles = TitleFactory.Bogus(200, seedTournaments, seedBowlers);
            await context.Titles.AddRangeAsync(seedTitles);
            await context.SaveChangesAsync();

            seedBowlerId = seedBowlers.First(bowler => bowler.Titles.Count > 0).Id;
        });

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri($"/bowlers/{seedBowlerId.Value}/titles", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        ApiResponse<BowlerTitlesResponse>? result =
            await response.Content.ReadFromJsonAsync<ApiResponse<BowlerTitlesResponse>>();

        result.ShouldNotBeNull();
        result.Data.BowlerId.ShouldBe(seedBowlerId);
    }

    [Fact]
    public async Task GetBowlerTitles_ShouldReturnNotFound_WhenBowlerDoesNotExist()
    {
        // Arrange
        await SeedAsync(async context =>
        {
            IReadOnlyCollection<BowlingCenter> seedBowlingCenters = BowlingCenterFactory.Bogus(10, 1960);
            await context.BowlingCenters.AddRangeAsync(seedBowlingCenters);
            await context.SaveChangesAsync();

            IReadOnlyCollection<Tournament> seedTournaments = TournamentFactory.Bogus(50, seedBowlingCenters, 1980);
            await context.Tournaments.AddRangeAsync(seedTournaments);
            await context.SaveChangesAsync();

            IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(50, 1980);
            context.Bowlers.AddRange(seedBowlers);
            await context.SaveChangesAsync();

            IReadOnlyCollection<Title> seedTitles = TitleFactory.Bogus(200, seedTournaments, seedBowlers);
            await context.Titles.AddRangeAsync(seedTitles);
            await context.SaveChangesAsync();
        });

        BowlerId nonExistentBowlerId = BowlerId.New();

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri($"/bowlers/{nonExistentBowlerId.Value}/titles", UriKind.Relative));

        // Assert
        Dictionary<string, object> metadata = new()
        {
            { "bowlerId", nonExistentBowlerId }
        };

        await response.ShouldBeNotFound("Bowler.NotFound", "Bowler was not found.", metadata);
    }
}
