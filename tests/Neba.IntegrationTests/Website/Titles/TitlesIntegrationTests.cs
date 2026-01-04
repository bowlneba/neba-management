using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Neba.Contracts;
using Neba.IntegrationTests.Infrastructure;
using Neba.Tests.Website;
using Neba.Website.Contracts.Titles;
using Neba.Website.Domain.Bowlers;
using Neba.Website.Domain.BowlingCenters;
using Neba.Website.Domain.Tournaments;

namespace Neba.IntegrationTests.Website.Titles;

[Trait("Category", "Integration")]
[Trait("Component", "Website.Titles")]

public sealed class TitlesIntegrationTests
    : ApiTestsBase
{
    [Fact]
    public async Task ListTitles_ShouldReturnExpectedResults()
    {
        // Arrange
        await SeedAsync(async context =>
                {
                    IReadOnlyCollection<BowlingCenter> seedBowlingCenters = BowlingCenterFactory.Bogus(10, 1960);
                    await context.BowlingCenters.AddRangeAsync(seedBowlingCenters);
                    await context.SaveChangesAsync();

                    IReadOnlyCollection<Tournament> seedTournaments = TournamentFactory.Bogus(500, seedBowlingCenters, 1963);
                    await context.Tournaments.AddRangeAsync(seedTournaments);
                    await context.SaveChangesAsync();

                    IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100);
                    context.Bowlers.AddRange(seedBowlers);
                    await context.SaveChangesAsync();

                    IReadOnlyCollection<Title> seedTitles = TitleFactory.Bogus(200, seedTournaments, seedBowlers);
                    context.Set<Title>().AddRange(seedTitles);
                    await context.SaveChangesAsync();
                });

        int totalTitles = await ExecuteAsync(async context
            => await context.Set<Title>().AsNoTracking().CountAsync());

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/titles", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<TitleResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<TitleResponse>>();

        result.ShouldNotBeNull();

        result.Items.Count.ShouldBe(totalTitles);
        result.TotalItems.ShouldBe(totalTitles);
    }

    [Fact]
    public async Task ListTitleSummaries_ShouldReturnExpectedResults()
    {
        // Arrange
        await SeedAsync(async context =>
                {
                    IReadOnlyCollection<BowlingCenter> seedBowlingCenters = BowlingCenterFactory.Bogus(10, 1960);
                    await context.BowlingCenters.AddRangeAsync(seedBowlingCenters);
                    await context.SaveChangesAsync();

                    IReadOnlyCollection<Tournament> seedTournaments = TournamentFactory.Bogus(500, seedBowlingCenters, 1963);
                    await context.Tournaments.AddRangeAsync(seedTournaments);
                    await context.SaveChangesAsync();

                    IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100);
                    context.Bowlers.AddRange(seedBowlers);
                    await context.SaveChangesAsync();

                    IReadOnlyCollection<Title> seedTitles = TitleFactory.Bogus(200, seedTournaments, seedBowlers);
                    context.Set<Title>().AddRange(seedTitles);
                    await context.SaveChangesAsync();
                });

        int totalBowlersWithTitles = await ExecuteAsync(async context
            => await context.Set<Title>().AsNoTracking().Select(t => t.BowlerId).Distinct().CountAsync());

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/titles/summary", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<TitleSummaryResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<TitleSummaryResponse>>();

        result.ShouldNotBeNull();

        result.Items.Count.ShouldBe(totalBowlersWithTitles);
        result.TotalItems.ShouldBe(totalBowlersWithTitles);
    }
}
