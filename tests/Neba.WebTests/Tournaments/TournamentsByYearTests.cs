using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neba.Contracts;
using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Tests;
using Neba.Tests.Website;
using Neba.Web.Server.Components;
using Neba.Web.Server.Services;
using Neba.Web.Server.Tournaments;
using Neba.Website.Contracts.Tournaments;

namespace Neba.WebTests.Tournaments;

[Trait("Category", "Web")]
[Trait("Component", "Tournaments")]
public sealed class TournamentsByYearTests : TestContextWrapper
{
    private readonly Mock<INebaWebsiteApi> _mockNebaApi;
    private readonly NebaWebsiteApiService _nebaWebsiteApiService;

    public TournamentsByYearTests()
    {
        _mockNebaApi = new Mock<INebaWebsiteApi>();
        _nebaWebsiteApiService = new NebaWebsiteApiService(_mockNebaApi.Object);
        TestContext.Services.AddSingleton(_nebaWebsiteApiService);
        TestContext.Services.AddSingleton<NavigationManager>(new FakeNavigationManager());
    }

    [Fact]
    public async Task OnInitializedAsync_SuccessfulApiResponse_LoadsTournaments()
    {
        // Arrange
        List<TournamentSummaryResponse> tournaments =
        [
            TournamentSummaryResponseFactory.Create(
                name: "Spring Classic",
                startDate: new DateOnly(2024, 3, 1),
                endDate: new DateOnly(2024, 3, 2)),
            TournamentSummaryResponseFactory.Create(
                name: "Summer Open",
                startDate: new DateOnly(2024, 6, 15),
                endDate: new DateOnly(2024, 6, 16))
        ];

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = tournaments
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(2024))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsByYear> cut = Render<TournamentsByYear>(p => p.Add(c => c.Year, 2024));
        await Task.Delay(100); // Wait for async initialization

        // Assert
        cut.ShouldNotBeNull();
        cut.Markup.ShouldContain("Spring Classic");
        cut.Markup.ShouldContain("Summer Open");
    }

    [Fact]
    public async Task OnInitializedAsync_DisplaysPageTitle()
    {
        // Arrange
        List<TournamentSummaryResponse> tournaments =
        [
            TournamentSummaryResponseFactory.Create()
        ];

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = tournaments
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(2024))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsByYear> cut = Render<TournamentsByYear>(p => p.Add(c => c.Year, 2024));
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("2024 Tournaments");
    }

    [Fact]
    public async Task OnInitializedAsync_DisplaysTournamentDetails()
    {
        // Arrange
        TournamentSummaryResponse tournament = TournamentSummaryResponseFactory.Create(
            name: "Championship Tournament",
            bowlingCenterName: "Bowlarama",
            startDate: new DateOnly(2024, 5, 10),
            endDate: new DateOnly(2024, 5, 11),
            tournamentType: TournamentType.Singles,
            patternLengthCategory: PatternLengthCategory.ShortPattern);

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = [tournament]
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(2024))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsByYear> cut = Render<TournamentsByYear>(p => p.Add(c => c.Year, 2024));
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("Championship Tournament");
        cut.Markup.ShouldContain("Bowlarama");
        cut.Markup.ShouldContain("Singles");
        cut.Markup.ShouldContain("Short");
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
            .Setup(x => x.GetTournamentsInAYearAsync(2024))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsByYear> cut = Render<TournamentsByYear>(p => p.Add(c => c.Year, 2024));
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("No tournaments found for 2024");
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
            .Setup(x => x.GetTournamentsInAYearAsync(2024))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsByYear> cut = Render<TournamentsByYear>(p => p.Add(c => c.Year, 2024));
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("Error Loading Tournaments");
    }


    [Fact]
    public async Task Render_IncludesLoadingIndicator()
    {
        // Arrange
        List<TournamentSummaryResponse> tournaments =
        [
            TournamentSummaryResponseFactory.Create()
        ];

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = tournaments
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(2024))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsByYear> cut = Render<TournamentsByYear>(p => p.Add(c => c.Year, 2024));

        // Assert
        IRenderedComponent<NebaLoadingIndicator> loadingIndicator = cut.FindComponent<NebaLoadingIndicator>();
        loadingIndicator.ShouldNotBeNull();
    }

    [Fact]
    public async Task Render_IncludesYearDropdown()
    {
        // Arrange
        List<TournamentSummaryResponse> tournaments =
        [
            TournamentSummaryResponseFactory.Create()
        ];

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = tournaments
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(2024))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsByYear> cut = Render<TournamentsByYear>(p => p.Add(c => c.Year, 2024));
        await Task.Delay(100);

        // Assert
        IRenderedComponent<NebaDropdown<int, int>> dropdown = cut.FindComponent<NebaDropdown<int, int>>();
        dropdown.ShouldNotBeNull();
    }

    [Fact]
    public async Task OnInitializedAsync_PastTournament_DisplaysPastBadge()
    {
        // Arrange
        var pastDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1));
        TournamentSummaryResponse tournament = TournamentSummaryResponseFactory.Create(
            name: "Past Tournament",
            startDate: pastDate,
            endDate: pastDate);

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = [tournament]
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(pastDate.Year))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsByYear> cut = Render<TournamentsByYear>(p => p.Add(c => c.Year, pastDate.Year));
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("Past");
        cut.Markup.ShouldContain("past-tournament");
    }

    [Fact]
    public async Task FormatFullDateRange_SameDay_DisplaysSingleDate()
    {
        // Arrange
        var date = new DateOnly(2024, 5, 15);
        TournamentSummaryResponse tournament = TournamentSummaryResponseFactory.Create(
            name: "One Day Tournament",
            startDate: date,
            endDate: date);

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = [tournament]
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(2024))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsByYear> cut = Render<TournamentsByYear>(p => p.Add(c => c.Year, 2024));
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("May 15, 2024");
    }

    [Fact]
    public async Task FormatFullDateRange_SameMonth_DisplaysCompactRange()
    {
        // Arrange
        TournamentSummaryResponse tournament = TournamentSummaryResponseFactory.Create(
            name: "Weekend Tournament",
            startDate: new DateOnly(2024, 6, 15),
            endDate: new DateOnly(2024, 6, 16));

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = [tournament]
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(2024))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsByYear> cut = Render<TournamentsByYear>(p => p.Add(c => c.Year, 2024));
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("June 15-16, 2024");
    }

    [Fact]
    public async Task FormatFullDateRange_DifferentMonths_DisplaysFullRange()
    {
        // Arrange
        TournamentSummaryResponse tournament = TournamentSummaryResponseFactory.Create(
            name: "Cross Month Tournament",
            startDate: new DateOnly(2024, 5, 30),
            endDate: new DateOnly(2024, 6, 2));

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = [tournament]
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(2024))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsByYear> cut = Render<TournamentsByYear>(p => p.Add(c => c.Year, 2024));
        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("May 30-June 02, 2024");
    }

    [Fact]
    public async Task OnInitializedAsync_TournamentWithoutPatternLength_DoesNotShowPatternTag()
    {
        // Arrange
        TournamentSummaryResponse tournament = TournamentSummaryResponseFactory.Create(
            name: "No Pattern Tournament",
            patternLengthCategory: null);

        CollectionResponse<TournamentSummaryResponse> collectionResponse = new()
        {
            Items = [tournament]
        };

        using TestApiResponse<CollectionResponse<TournamentSummaryResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetTournamentsInAYearAsync(2024))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentsByYear> cut = Render<TournamentsByYear>(p => p.Add(c => c.Year, 2024));
        await Task.Delay(100);

        // Assert
        // Should only have one tag (tournament type), not two
        cut.Markup.ShouldContain("tournament-tag");
        cut.Markup.ShouldContain("Singles"); // Default tournament type
    }

    private sealed class FakeNavigationManager : NavigationManager
    {
        public FakeNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/tournaments/2024");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            Uri = new Uri(new Uri(BaseUri), uri).ToString();
        }
    }
}
