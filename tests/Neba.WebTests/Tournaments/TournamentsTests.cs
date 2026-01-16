using Bunit;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neba.Contracts;
using Neba.Domain.Tournaments;
using Neba.Tests;
using Neba.Tests.Website;
using Neba.Web.Server.Components;
using Neba.Web.Server.Services;
using Neba.Web.Server.Tournaments;
using Neba.Website.Contracts.Tournaments;
using TournamentsPage = Neba.Web.Server.Tournaments.Tournaments;

namespace Neba.WebTests.Tournaments;

[Trait("Category", "Web")]
[Trait("Component", "Tournaments")]
public sealed class TournamentsTests : TestContextWrapper
{
    private readonly Mock<INebaWebsiteApi> _mockNebaApi;
    private readonly NebaWebsiteApiService _nebaWebsiteApiService;

    public TournamentsTests()
    {
        _mockNebaApi = new Mock<INebaWebsiteApi>();
        _nebaWebsiteApiService = new NebaWebsiteApiService(_mockNebaApi.Object);
        TestContext.Services.AddSingleton(_nebaWebsiteApiService);
    }

    [Fact]
    public async Task OnInitializedAsync_SuccessfulApiResponse_LoadsTournaments()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(1));
        List<TournamentSummaryResponse> tournaments =
        [
            TournamentSummaryResponseFactory.Create(
                name: "Future Classic",
                startDate: futureDate,
                endDate: futureDate.AddDays(1)),
            TournamentSummaryResponseFactory.Create(
                name: "Summer Open",
                startDate: futureDate.AddMonths(2),
                endDate: futureDate.AddMonths(2).AddDays(1))
        ];

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = tournaments
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsPage> cut = Render<TournamentsPage>();
        await Task.Delay(100);

        // Assert
        cut.ShouldNotBeNull();
        cut.Markup.ShouldContain("Future Classic");
        cut.Markup.ShouldContain("Summer Open");
    }

    [Fact]
    public async Task OnInitializedAsync_DisplaysPageTitle()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(1));
        List<TournamentSummaryResponse> tournaments =
        [
            TournamentSummaryResponseFactory.Create(startDate: futureDate, endDate: futureDate)
        ];

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = tournaments
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsPage> cut = Render<TournamentsPage>();
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("Future Tournaments");
    }

    [Fact]
    public async Task OnInitializedAsync_DisplaysTournamentDetails()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(1));
        var tournament = TournamentSummaryResponseFactory.Create(
            name: "Championship Tournament",
            bowlingCenterName: "Strike Zone",
            startDate: futureDate,
            endDate: futureDate.AddDays(1),
            tournamentType: TournamentType.Doubles,
            patternLengthCategory: PatternLengthCategory.MediumPattern);

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = [tournament]
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsPage> cut = Render<TournamentsPage>();
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("Championship Tournament");
        cut.Markup.ShouldContain("Strike Zone");
        cut.Markup.ShouldContain("Doubles");
        cut.Markup.ShouldContain("Medium");
    }

    [Fact]
    public async Task OnInitializedAsync_EmptyResponse_DisplaysNoTournamentsMessage()
    {
        // Arrange
        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = []
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsPage> cut = Render<TournamentsPage>();
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("No Future tournaments at this time");
    }

    [Fact]
    public async Task OnInitializedAsync_ApiError_DisplaysErrorMessage()
    {
        // Arrange
        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = []
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateResponse(collectionResponse, System.Net.HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsPage> cut = Render<TournamentsPage>();
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("Error Loading Tournaments");
    }


    [Fact]
    public async Task Render_IncludesLoadingIndicator()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(1));
        List<TournamentSummaryResponse> tournaments =
        [
            TournamentSummaryResponseFactory.Create(startDate: futureDate, endDate: futureDate)
        ];

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = tournaments
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsPage> cut = Render<TournamentsPage>();

        // Assert
        IRenderedComponent<NebaLoadingIndicator> loadingIndicator = cut.FindComponent<NebaLoadingIndicator>();
        loadingIndicator.ShouldNotBeNull();
    }

    [Fact]
    public async Task FormatDateRange_SameDay_DisplaysShortDate()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Now.AddMonths(1));
        var tournament = TournamentSummaryResponseFactory.Create(
            name: "One Day Event",
            startDate: date,
            endDate: date);

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = [tournament]
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsPage> cut = Render<TournamentsPage>();
        await Task.Delay(100);

        // Assert
        // Short format: "MMM dd"
        cut.Markup.ShouldContain(date.ToString("MMM dd", CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task FormatDateRange_SameMonth_DisplaysCompactRange()
    {
        // Arrange
        var startDate = new DateOnly(DateTime.Now.Year + 1, 6, 15);
        var endDate = new DateOnly(DateTime.Now.Year + 1, 6, 16);
        var tournament = TournamentSummaryResponseFactory.Create(
            name: "Weekend Event",
            startDate: startDate,
            endDate: endDate);

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = [tournament]
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsPage> cut = Render<TournamentsPage>();
        await Task.Delay(100);

        // Assert
        // Same month format: "MMM dd-dd"
        cut.Markup.ShouldContain("Jun 15-16");
    }

    [Fact]
    public async Task FormatDateRange_DifferentMonths_DisplaysFullRange()
    {
        // Arrange
        var startDate = new DateOnly(DateTime.Now.Year + 1, 5, 30);
        var endDate = new DateOnly(DateTime.Now.Year + 1, 6, 2);
        var tournament = TournamentSummaryResponseFactory.Create(
            name: "Cross Month Event",
            startDate: startDate,
            endDate: endDate);

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = [tournament]
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsPage> cut = Render<TournamentsPage>();
        await Task.Delay(100);

        // Assert
        // Different month format: "MMM dd-MMM dd"
        cut.Markup.ShouldContain("May 30-Jun 02");
    }

    [Fact]
    public async Task OnInitializedAsync_TournamentWithoutPatternLength_DoesNotShowPatternTag()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(1));
        var tournament = TournamentSummaryResponseFactory.Create(
            name: "No Pattern Tournament",
            startDate: futureDate,
            endDate: futureDate,
            patternLengthCategory: null);

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = [tournament]
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsPage> cut = Render<TournamentsPage>();
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("tournament-tag");
        cut.Markup.ShouldContain("Singles"); // Default tournament type
    }

    [Fact]
    public async Task OnInitializedAsync_DisplaysTimelineLayout()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(1));
        var tournament = TournamentSummaryResponseFactory.Create(
            name: "Timeline Tournament",
            startDate: futureDate,
            endDate: futureDate);

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = [tournament]
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsPage> cut = Render<TournamentsPage>();
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("tournament-timeline");
        cut.Markup.ShouldContain("tournament-timeline-item");
        cut.Markup.ShouldContain("tournament-timeline-date");
    }

    [Fact]
    public async Task OnInitializedAsync_DisplaysYearInTimeline()
    {
        // Arrange
        var futureDate = new DateOnly(DateTime.Now.Year + 1, 3, 15);
        var tournament = TournamentSummaryResponseFactory.Create(
            name: "Next Year Tournament",
            startDate: futureDate,
            endDate: futureDate);

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = [tournament]
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsPage> cut = Render<TournamentsPage>();
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain((DateTime.Now.Year + 1).ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task OnInitializedAsync_DisplaysLocationWithIcon()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(1));
        var tournament = TournamentSummaryResponseFactory.Create(
            name: "Location Test",
            bowlingCenterName: "Test Lanes",
            startDate: futureDate,
            endDate: futureDate);

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = [tournament]
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetFutureTournamentsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsPage> cut = Render<TournamentsPage>();
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("Test Lanes");
    }
}
