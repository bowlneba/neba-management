using Microsoft.EntityFrameworkCore;
using Neba.Tests.Infrastructure;
using Neba.Tests.Website;
using Neba.Website.Application.BowlingCenters;
using Neba.Website.Domain.BowlingCenters;
using Neba.Website.Infrastructure.Database;
using Neba.Website.Infrastructure.Database.Repositories;

namespace Neba.IntegrationTests.Website.BowlingCenters;

[Trait("Category", "Integration")]
[Trait("Component", "Website.BowlingCenters")]

public sealed class WebsiteBowlingCenterQueryRepositoryTests : IAsyncLifetime
{
    private DatabaseContainer _database = null!;

    /// <summary>
    /// Called before each test class - initializes a fresh database container.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        _database = new DatabaseContainer();
        await _database.InitializeAsync();
    }

    /// <summary>
    /// Called after all tests complete - disposes the database container.
    /// </summary>
    public async ValueTask DisposeAsync()
        => await _database.DisposeAsync();

    [Fact]
    public async Task ListBowlingCentersAsync_ShouldOnlyReturnOpenCenters()
    {
        // Arrange
        await using var websiteDbContext = new WebsiteDbContext(
            new DbContextOptionsBuilder<WebsiteDbContext>()
                .UseNpgsql(_database.ConnectionString)
                .Options);

        // Create one explicitly open center
        BowlingCenter openCenter = BowlingCenterFactory.Create(
            name: "Open Bowling Center",
            isClosed: false);

        // Create one explicitly closed center
        BowlingCenter closedCenter = BowlingCenterFactory.Create(
            name: "Closed Bowling Center",
            isClosed: true);

        // Create additional centers using Bogus
        IReadOnlyCollection<BowlingCenter> bogusCenter = BowlingCenterFactory.Bogus(10, seed: 2025);

        await websiteDbContext.BowlingCenters.AddAsync(openCenter);
        await websiteDbContext.BowlingCenters.AddAsync(closedCenter);
        await websiteDbContext.BowlingCenters.AddRangeAsync(bogusCenter);
        await websiteDbContext.SaveChangesAsync();

        var repository = new WebsiteBowlingCenterQueryRepository(websiteDbContext);

        // Act
        IReadOnlyCollection<BowlingCenterDto> result
            = await repository.ListBowlingCentersAsync(CancellationToken.None);

        // Assert
        // The closed center should NOT be in the results
        result.ShouldNotContain(dto => dto.Name == closedCenter.Name);

        // The open center SHOULD be in the results
        BowlingCenterDto? openCenterResult = result.FirstOrDefault(dto => dto.Name == openCenter.Name);
        openCenterResult.ShouldNotBeNull();
        openCenterResult.Name.ShouldBe(openCenter.Name);
        openCenterResult.Street.ShouldBe(openCenter.Address.Street);
        openCenterResult.Unit.ShouldBe(openCenter.Address.Unit);
        openCenterResult.City.ShouldBe(openCenter.Address.City);
        openCenterResult.State.Value.ShouldBe(openCenter.Address.Region);
        openCenterResult.ZipCode.ShouldBe(openCenter.Address.PostalCode);
        openCenterResult.PhoneNumber.ShouldBe($"{openCenter.PhoneNumber.CountryCode}{openCenter.PhoneNumber.Number}");
        openCenterResult.Extension.ShouldBe(openCenter.PhoneNumber.Extension);
        openCenterResult.Latitude.ShouldBe(openCenter.Address.Coordinates!.Latitude);
        openCenterResult.Longitude.ShouldBe(openCenter.Address.Coordinates!.Longitude);
        openCenterResult.IsClosed.ShouldBe(false);

        // Count the expected number of open centers from bogus data
        int expectedOpenCentersFromBogus = bogusCenter.Count(bc => !bc.IsClosed);
        int expectedTotalOpenCenters = expectedOpenCentersFromBogus + 1; // +1 for the explicitly open center

        result.Count.ShouldBe(expectedTotalOpenCenters);
    }
}
