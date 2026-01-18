using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Neba.Contracts;
using Neba.IntegrationTests.Infrastructure;
using Neba.Tests.Website;
using Neba.Website.Contracts.Tournaments;
using Neba.Website.Domain.BowlingCenters;
using Neba.Website.Domain.Tournaments;

namespace Neba.IntegrationTests.Website.Tournaments;

[Trait("Category", "Integration")]
[Trait("Component", "Website.Tournaments")]
public sealed class TournamentsIntegrationTests
    : ApiTestsBase
{
    [Fact(DisplayName = "Returns all future tournaments with expected results")]
    public async Task ListFutureTournaments_ShouldReturnExpectedResults()
    {
        // Arrange
        await SeedAsync(async context =>
        {
            IReadOnlyCollection<BowlingCenter> centers = BowlingCenterFactory.Bogus(10, seed: 42);
            await context.BowlingCenters.AddRangeAsync(centers);
            await context.SaveChangesAsync();

            // Create past, current, and future tournaments
            Tournament pastTournament = TournamentFactory.Create(
                name: "Past Tournament",
                startDate: new DateOnly(2024, 1, 1),
                endDate: new DateOnly(2024, 1, 2),
                bowlingCenter: centers.First());

            Tournament currentTournament = TournamentFactory.Create(
                name: "Current Tournament",
                startDate: DateOnly.FromDateTime(DateTime.Today),
                endDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                bowlingCenter: centers.Skip(1).First());

            Tournament futureTournament1 = TournamentFactory.Create(
                name: "Future Tournament 1",
                startDate: DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                endDate: DateOnly.FromDateTime(DateTime.Today.AddDays(12)),
                bowlingCenter: centers.Skip(2).First());

            Tournament futureTournament2 = TournamentFactory.Create(
                name: "Future Tournament 2",
                startDate: DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
                endDate: DateOnly.FromDateTime(DateTime.Today.AddDays(32)),
                bowlingCenter: centers.Skip(3).First());

            await context.Tournaments.AddRangeAsync(pastTournament, currentTournament, futureTournament1, futureTournament2);
            await context.SaveChangesAsync();
        });

        int totalFutureTournaments = await ExecuteAsync(async context =>
            await context.Tournaments
                .Where(t => t.StartDate >= DateOnly.FromDateTime(DateTime.Today))
                .CountAsync());

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/tournaments/future", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<TournamentSummaryResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<TournamentSummaryResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(totalFutureTournaments);
        result.TotalItems.ShouldBe(totalFutureTournaments);
    }

    [Fact(DisplayName = "Returns empty collection when no future tournaments exist")]
    public async Task ListFutureTournaments_WithNoFutureTournaments_ShouldReturnEmptyCollection()
    {
        // Arrange
        await SeedAsync(async context =>
        {
            IReadOnlyCollection<BowlingCenter> centers = BowlingCenterFactory.Bogus(5, seed: 42);
            await context.BowlingCenters.AddRangeAsync(centers);
            await context.SaveChangesAsync();

            // Create only past tournaments
            Tournament pastTournament1 = TournamentFactory.Create(
                name: "Past Tournament 1",
                startDate: new DateOnly(2023, 1, 1),
                endDate: new DateOnly(2023, 1, 2),
                bowlingCenter: centers.First());

            Tournament pastTournament2 = TournamentFactory.Create(
                name: "Past Tournament 2",
                startDate: new DateOnly(2023, 6, 1),
                endDate: new DateOnly(2023, 6, 2),
                bowlingCenter: centers.Skip(1).First());

            await context.Tournaments.AddRangeAsync(pastTournament1, pastTournament2);
            await context.SaveChangesAsync();
        });

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/tournaments/future", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<TournamentSummaryResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<TournamentSummaryResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(0);
        result.TotalItems.ShouldBe(0);
    }

    [Fact(DisplayName = "Returns empty collection when no tournaments exist at all")]
    public async Task ListFutureTournaments_WithNoData_ShouldReturnEmptyCollection()
    {
        // Arrange
        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/tournaments/future", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<TournamentSummaryResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<TournamentSummaryResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(0);
        result.TotalItems.ShouldBe(0);
    }

    [Fact(DisplayName = "Returns future tournaments with all required fields populated")]
    public async Task ListFutureTournaments_ShouldIncludeAllRequiredFields()
    {
        // Arrange
        await SeedAsync(async context =>
        {
            IReadOnlyCollection<BowlingCenter> centers = BowlingCenterFactory.Bogus(5, seed: 42);
            await context.BowlingCenters.AddRangeAsync(centers);
            await context.SaveChangesAsync();

            Tournament futureTournament = TournamentFactory.Create(
                name: "Future Tournament with All Fields",
                startDate: DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                endDate: DateOnly.FromDateTime(DateTime.Today.AddDays(12)),
                bowlingCenter: centers.First());

            context.Tournaments.Add(futureTournament);
            await context.SaveChangesAsync();
        });

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/tournaments/future", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<TournamentSummaryResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<TournamentSummaryResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBeGreaterThan(0);

        foreach (TournamentSummaryResponse tournament in result.Items)
        {
            tournament.Id.Value.ShouldNotBe(default);
            tournament.Name.ShouldNotBeNullOrWhiteSpace();
            tournament.StartDate.ShouldNotBe(default);
            tournament.EndDate.ShouldNotBe(default);
            tournament.TournamentType.ShouldNotBeNull();
            tournament.StartDate.ShouldBeLessThanOrEqualTo(tournament.EndDate);
        }
    }

    [Fact(DisplayName = "Returns all tournaments in a specific year with expected results")]
    public async Task ListTournamentsInAYear_ShouldReturnExpectedResults()
    {
        // Arrange
        const int testYear = 2024;

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<BowlingCenter> centers = BowlingCenterFactory.Bogus(10, seed: 42);
            await context.BowlingCenters.AddRangeAsync(centers);
            await context.SaveChangesAsync();

            // Create tournaments in different years
            Tournament tournament2023 = TournamentFactory.Create(
                name: "2023 Tournament",
                startDate: new DateOnly(2023, 6, 1),
                endDate: new DateOnly(2023, 6, 2),
                bowlingCenter: centers.First());

            Tournament tournament2024_1 = TournamentFactory.Create(
                name: "2024 Tournament 1",
                startDate: new DateOnly(2024, 3, 1),
                endDate: new DateOnly(2024, 3, 2),
                bowlingCenter: centers.Skip(1).First());

            Tournament tournament2024_2 = TournamentFactory.Create(
                name: "2024 Tournament 2",
                startDate: new DateOnly(2024, 9, 1),
                endDate: new DateOnly(2024, 9, 2),
                bowlingCenter: centers.Skip(2).First());

            Tournament tournament2025 = TournamentFactory.Create(
                name: "2025 Tournament",
                startDate: new DateOnly(2025, 3, 1),
                endDate: new DateOnly(2025, 3, 2),
                bowlingCenter: centers.Skip(3).First());

            await context.Tournaments.AddRangeAsync(tournament2023, tournament2024_1, tournament2024_2, tournament2025);
            await context.SaveChangesAsync();
        });

        int totalTournamentsIn2024 = await ExecuteAsync(async context =>
            await context.Tournaments
                .Where(t => t.StartDate.Year == testYear)
                .CountAsync());

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri($"/tournaments/{testYear}", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<TournamentSummaryResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<TournamentSummaryResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(totalTournamentsIn2024);
        result.TotalItems.ShouldBe(totalTournamentsIn2024);

        // Verify all tournaments are from the requested year
        foreach (TournamentSummaryResponse tournament in result.Items)
        {
            tournament.StartDate.Year.ShouldBe(testYear);
        }
    }

    [Fact(DisplayName = "Returns empty collection when no tournaments exist in specified year")]
    public async Task ListTournamentsInAYear_WithNoTournamentsInYear_ShouldReturnEmptyCollection()
    {
        // Arrange
        const int testYear = 2030;

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<BowlingCenter> centers = BowlingCenterFactory.Bogus(5, seed: 42);
            await context.BowlingCenters.AddRangeAsync(centers);
            await context.SaveChangesAsync();

            // Create tournaments in different years
            Tournament tournament2024 = TournamentFactory.Create(
                name: "2024 Tournament",
                startDate: new DateOnly(2024, 6, 1),
                endDate: new DateOnly(2024, 6, 2),
                bowlingCenter: centers.First());

            context.Tournaments.Add(tournament2024);
            await context.SaveChangesAsync();
        });

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri($"/tournaments/{testYear}", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<TournamentSummaryResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<TournamentSummaryResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(0);
        result.TotalItems.ShouldBe(0);
    }

    [Fact(DisplayName = "Returns tournaments in a year with all required fields populated")]
    public async Task ListTournamentsInAYear_ShouldIncludeAllRequiredFields()
    {
        // Arrange
        const int testYear = 2024;

        await SeedAsync(async context =>
        {
            IReadOnlyCollection<BowlingCenter> centers = BowlingCenterFactory.Bogus(5, seed: 42);
            await context.BowlingCenters.AddRangeAsync(centers);
            await context.SaveChangesAsync();

            Tournament tournament = TournamentFactory.Create(
                name: "2024 Complete Tournament",
                startDate: new DateOnly(2024, 6, 1),
                endDate: new DateOnly(2024, 6, 2),
                bowlingCenter: centers.First());

            context.Tournaments.Add(tournament);
            await context.SaveChangesAsync();
        });

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri($"/tournaments/{testYear}", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<TournamentSummaryResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<TournamentSummaryResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBeGreaterThan(0);

        foreach (TournamentSummaryResponse tournament in result.Items)
        {
            tournament.Id.Value.ShouldNotBe(default);
            tournament.Name.ShouldNotBeNullOrWhiteSpace();
            tournament.StartDate.ShouldNotBe(default);
            tournament.EndDate.ShouldNotBe(default);
            tournament.TournamentType.ShouldNotBeNull();
            tournament.StartDate.Year.ShouldBe(testYear);
            tournament.StartDate.ShouldBeLessThanOrEqualTo(tournament.EndDate);
        }
    }

    [Fact(DisplayName = "Returns tournaments with bowling center information when available")]
    public async Task ListTournaments_ShouldIncludeBowlingCenterInformation()
    {
        // Arrange
        await SeedAsync(async context =>
        {
            IReadOnlyCollection<BowlingCenter> centers = BowlingCenterFactory.Bogus(5, seed: 42);
            await context.BowlingCenters.AddRangeAsync(centers);
            await context.SaveChangesAsync();

            Tournament tournament = TournamentFactory.Create(
                name: "Tournament with Center",
                startDate: DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                endDate: DateOnly.FromDateTime(DateTime.Today.AddDays(12)),
                bowlingCenter: centers.First());

            context.Tournaments.Add(tournament);
            await context.SaveChangesAsync();
        });

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/tournaments/future", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<TournamentSummaryResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<TournamentSummaryResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBeGreaterThan(0);

        TournamentSummaryResponse tournament = result.Items.First();
        tournament.BowlingCenterId.ShouldNotBeNull();
        tournament.BowlingCenterName.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Returns tournaments sorted chronologically")]
    public async Task ListFutureTournaments_ShouldBeSortedByDate()
    {
        // Arrange
        await SeedAsync(async context =>
        {
            IReadOnlyCollection<BowlingCenter> centers = BowlingCenterFactory.Bogus(5, seed: 42);
            await context.BowlingCenters.AddRangeAsync(centers);
            await context.SaveChangesAsync();

            Tournament tournament1 = TournamentFactory.Create(
                name: "Later Tournament",
                startDate: DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
                endDate: DateOnly.FromDateTime(DateTime.Today.AddDays(32)),
                bowlingCenter: centers.First());

            Tournament tournament2 = TournamentFactory.Create(
                name: "Earlier Tournament",
                startDate: DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                endDate: DateOnly.FromDateTime(DateTime.Today.AddDays(12)),
                bowlingCenter: centers.Skip(1).First());

            Tournament tournament3 = TournamentFactory.Create(
                name: "Middle Tournament",
                startDate: DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
                endDate: DateOnly.FromDateTime(DateTime.Today.AddDays(22)),
                bowlingCenter: centers.Skip(2).First());

            await context.Tournaments.AddRangeAsync(tournament1, tournament2, tournament3);
            await context.SaveChangesAsync();
        });

        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/tournaments/future", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CollectionResponse<TournamentSummaryResponse>? result
            = await response.Content.ReadFromJsonAsync<CollectionResponse<TournamentSummaryResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(3);

        // Verify chronological ordering
        DateOnly? previousDate = null;
        foreach (TournamentSummaryResponse tournament in result.Items)
        {
            if (previousDate.HasValue)
            {
                tournament.StartDate.ShouldBeGreaterThanOrEqualTo(previousDate.Value);
            }
            previousDate = tournament.StartDate;
        }
    }
}
