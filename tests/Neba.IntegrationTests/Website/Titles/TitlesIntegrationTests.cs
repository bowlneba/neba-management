using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Neba.Contracts;
using Neba.Domain.Bowlers;
using Neba.IntegrationTests.Infrastructure;
using Neba.Tests;
using Neba.Tests.Website;
using Neba.Website.Contracts.Titles;

namespace Neba.IntegrationTests.Website.Titles;

[Collection(nameof(Infrastructure.Collections.TitlesIntegrationTests))]
public sealed class TitlesIntegrationTests
    : ApiTestsBase
{
    [Fact]
    public async Task ListTitles_ShouldReturnExpectedResults()
    {
        // Arrange
        await ResetDatabaseAsync();

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100);
            context.Bowlers.AddRange(seedBowlers);
            await context.SaveChangesAsync();
        });

        int totalTitles = await ExecuteAsync(async context
            => await context.Bowlers.AsNoTracking().SelectMany(b => b.Titles).CountAsync());

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
        await ResetDatabaseAsync();

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<Bowler> seedBowlers = BowlerFactory.Bogus(100);
            context.Bowlers.AddRange(seedBowlers);
            await context.SaveChangesAsync();
        });

        int totalBowlersWithTitles = await ExecuteAsync(async context
            => await context.Bowlers.AsNoTracking().Where(b => b.Titles.Count > 0).CountAsync());

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
