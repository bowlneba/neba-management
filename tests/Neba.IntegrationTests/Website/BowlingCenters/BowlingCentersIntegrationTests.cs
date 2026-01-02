using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Neba.Contracts;
using Neba.IntegrationTests.Infrastructure;
using Neba.Tests.Website;
using Neba.Website.Contracts.BowlingCenters;
using Neba.Website.Domain.BowlingCenters;

namespace Neba.IntegrationTests.Website.BowlingCenters;

[Trait("Category", "Integration")]
[Trait("Component", "Website.BowlingCenters")]

public sealed class BowlingCentersIntegrationTests
    : ApiTestsBase
{
    [Fact]
    public async Task GetBowlingCenters_ShouldReturnExpectedResults()
    {
        // Arrange
        await SeedAsync(async context =>
        {
            IReadOnlyCollection<BowlingCenter> seedBowlingCenters = BowlingCenterFactory.Bogus(50, seed: 2025);
            context.BowlingCenters.AddRange(seedBowlingCenters);
            await context.SaveChangesAsync();
        });

        int totalOpenCenters = await ExecuteAsync(async context
            => await context.BowlingCenters.AsNoTracking().Where(bc => !bc.IsClosed).CountAsync());

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/bowling-centers", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<BowlingCenterResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<BowlingCenterResponse>>();

        result.ShouldNotBeNull();

        result.Items.Count.ShouldBe(totalOpenCenters);
        result.TotalItems.ShouldBe(totalOpenCenters);
    }

    [Fact]
    public async Task GetBowlingCenters_ShouldOnlyReturnOpenCenters()
    {
        // Arrange
        await SeedAsync(async context =>
        {
            BowlingCenter openCenter = BowlingCenterFactory.Create(
                name: "Open Test Center",
                isClosed: false);

            BowlingCenter closedCenter = BowlingCenterFactory.Create(
                name: "Closed Test Center",
                isClosed: true);

            await context.BowlingCenters.AddAsync(openCenter);
            await context.BowlingCenters.AddAsync(closedCenter);
            await context.SaveChangesAsync();
        });

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/bowling-centers", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<BowlingCenterResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<BowlingCenterResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(1);
        result.TotalItems.ShouldBe(1);

        BowlingCenterResponse returnedCenter = result.Items.Single();
        returnedCenter.Name.ShouldBe("Open Test Center");
        returnedCenter.IsClosed.ShouldBe(false);
    }

    [Fact]
    public async Task GetBowlingCenters_ShouldReturnEmptyCollection_WhenNoBowlingCentersExist()
    {
        // Arrange - no seeding, empty database
        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/bowling-centers", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<BowlingCenterResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<BowlingCenterResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(0);
        result.TotalItems.ShouldBe(0);
    }
}
