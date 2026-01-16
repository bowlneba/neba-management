using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neba.Contracts;
using Neba.Tests;
using Neba.Tests.Website;
using Neba.Web.Server.Components;
using Neba.Web.Server.History.Awards;
using Neba.Web.Server.Services;
using Neba.Website.Contracts.Awards;

namespace Neba.WebTests.History.Awards;

[Trait("Category", "Web")]
[Trait("Component", "History.Awards")]
public sealed class HighAverageTests : TestContextWrapper
{
    private readonly Mock<INebaWebsiteApi> _mockNebaApi;
    private readonly NebaWebsiteApiService _nebaWebsiteApiService;

    public HighAverageTests()
    {
        _mockNebaApi = new Mock<INebaWebsiteApi>();
        _nebaWebsiteApiService = new NebaWebsiteApiService(_mockNebaApi.Object);
        TestContext.Services.AddSingleton(_nebaWebsiteApiService);
    }

    [Fact]
    public void OnInitializedAsync_SuccessfulApiResponse_LoadsAwards()
    {
        // Arrange
        List<HighAverageAwardResponse> awards =
        [
            HighAverageAwardResponseFactory.Create(bowlerName: "John Doe", season: "2024", average: 225.50m),
            HighAverageAwardResponseFactory.Create(bowlerName: "Jane Smith", season: "2023", average: 220.75m)
        ];

        CollectionResponse<HighAverageAwardResponse> collectionResponse = new()
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<HighAverage> cut = Render<HighAverage>();

        // Assert
        cut.ShouldNotBeNull();
        cut.Markup.ShouldContain("High Average");
    }

    [Fact]
    public void OnInitializedAsync_DisplaysAwardsBySeason()
    {
        // Arrange
        List<HighAverageAwardResponse> awards =
        [
            HighAverageAwardResponseFactory.Create(bowlerName: "John Doe", season: "2024", average: 225.50m),
            HighAverageAwardResponseFactory.Create(bowlerName: "Jane Smith", season: "2023", average: 220.75m),
            HighAverageAwardResponseFactory.Create(bowlerName: "Bob Johnson", season: "2022", average: 218.00m)
        ];

        CollectionResponse<HighAverageAwardResponse> collectionResponse = new()
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<HighAverage> cut = Render<HighAverage>();

        // Assert
        cut.Markup.ShouldContain("2024");
        cut.Markup.ShouldContain("2023");
        cut.Markup.ShouldContain("2022");
        cut.Markup.ShouldContain("John Doe");
        cut.Markup.ShouldContain("Jane Smith");
        cut.Markup.ShouldContain("Bob Johnson");
    }

    [Fact]
    public void OnInitializedAsync_FormatsAveragesCorrectly()
    {
        // Arrange
        List<HighAverageAwardResponse> awards =
        [
            HighAverageAwardResponseFactory.Create(bowlerName: "Precision Bowler", season: "2024", average: 225.50m)
        ];

        CollectionResponse<HighAverageAwardResponse> collectionResponse = new()
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<HighAverage> cut = Render<HighAverage>();

        // Assert
        cut.Markup.ShouldContain("225.50");
    }

    [Fact]
    public void OnInitializedAsync_HighestAverage_DisplaysRecordIndicator()
    {
        // Arrange
        List<HighAverageAwardResponse> awards =
        [
            HighAverageAwardResponseFactory.Create(bowlerName: "Record Holder", season: "2024", average: 235.00m),
            HighAverageAwardResponseFactory.Create(bowlerName: "Second Place", season: "2023", average: 220.00m),
            HighAverageAwardResponseFactory.Create(bowlerName: "Third Place", season: "2022", average: 215.00m)
        ];

        CollectionResponse<HighAverageAwardResponse> collectionResponse = new()
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<HighAverage> cut = Render<HighAverage>();

        // Assert
        cut.Markup.ShouldContain("RECORD");
        cut.Markup.ShouldContain("Record Holder");
    }

    [Fact]
    public void OnInitializedAsync_EmptyResponse_DisplaysNoDataMessage()
    {
        // Arrange
        CollectionResponse<HighAverageAwardResponse> collectionResponse = new()
        {
            Items = new List<HighAverageAwardResponse>()
        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<HighAverage> cut = Render<HighAverage>();

        // Assert
        cut.Markup.ShouldContain("No high average awards data available");
    }

    [Fact]
    public void OnInitializedAsync_ApiReturnsError_DisplaysErrorAlert()
    {
        // Arrange
        CollectionResponse<HighAverageAwardResponse> collectionResponse = new()
        {
            Items = new List<HighAverageAwardResponse>()
        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> response =
            ApiResponseFactory.CreateResponse(collectionResponse, System.Net.HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<HighAverage> cut = Render<HighAverage>();

        // Assert
        cut.Markup.ShouldContain("Error Loading Awards");
    }

    [Fact]
    public void Render_IncludesLoadingIndicator()
    {
        // Arrange
        List<HighAverageAwardResponse> awards =
        [
            HighAverageAwardResponseFactory.Create()
        ];

        CollectionResponse<HighAverageAwardResponse> collectionResponse = new()
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<HighAverage> cut = Render<HighAverage>();

        // Assert
        IRenderedComponent<NebaLoadingIndicator> loadingIndicator = cut.FindComponent<NebaLoadingIndicator>();
        loadingIndicator.ShouldNotBeNull();
    }

    [Fact]
    public void Render_DisplaysDescriptiveText()
    {
        // Arrange
        List<HighAverageAwardResponse> awards =
        [
            HighAverageAwardResponseFactory.Create()
        ];

        CollectionResponse<HighAverageAwardResponse> collectionResponse = new()
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<HighAverage> cut = Render<HighAverage>();

        // Assert
        cut.Markup.ShouldContain("High Average");
        cut.Markup.ShouldContain("season-long average");
    }

    [Fact]
    public void OnInitializedAsync_DisplaysCorrectTableHeaders()
    {
        // Arrange
        List<HighAverageAwardResponse> awards =
        [
            HighAverageAwardResponseFactory.Create()
        ];

        CollectionResponse<HighAverageAwardResponse> collectionResponse = new()
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<HighAverage> cut = Render<HighAverage>();

        // Assert
        cut.Markup.ShouldContain("Year");
        cut.Markup.ShouldContain("Bowler");
        cut.Markup.ShouldContain("Avg.");
        cut.Markup.ShouldContain("Games");
        cut.Markup.ShouldContain("Tournaments");
    }

    [Fact]
    public void OnInitializedAsync_DisplaysGamesAndTournaments()
    {
        // Arrange
        List<HighAverageAwardResponse> awards =
        [
            HighAverageAwardResponseFactory.Create(
                bowlerName: "Stats Bowler",
                season: "2024",
                average: 220.00m,
                games: 45,
                tournaments: 10)
        ];

        CollectionResponse<HighAverageAwardResponse> collectionResponse = new()
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<HighAverage> cut = Render<HighAverage>();

        // Assert
        cut.Markup.ShouldContain("45");
        cut.Markup.ShouldContain("10");
    }

    [Fact]
    public void OnInitializedAsync_NullGames_DisplaysDash()
    {
        // Arrange
        List<HighAverageAwardResponse> awards =
        [
            new HighAverageAwardResponse
            {
                BowlerName = "Old Timer",
                Season = "1980",
                Average = 200.00m,
                Games = null,
                Tournaments = null
            }
        ];

        CollectionResponse<HighAverageAwardResponse> collectionResponse = new()
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<HighAverage> cut = Render<HighAverage>();

        // Assert
        // Dashes are displayed for null values
        var markup = cut.Markup;
        markup.ShouldContain("\u2014"); // Em dash
    }

    [Fact]
    public void OnInitializedAsync_MultipleRecordHolders_AllShowRecordIndicator()
    {
        // Arrange - Two bowlers tied for record
        List<HighAverageAwardResponse> awards =
        [
            HighAverageAwardResponseFactory.Create(bowlerName: "Tied First", season: "2024", average: 230.00m),
            HighAverageAwardResponseFactory.Create(bowlerName: "Also First", season: "2023", average: 230.00m),
            HighAverageAwardResponseFactory.Create(bowlerName: "Second", season: "2022", average: 220.00m)
        ];

        CollectionResponse<HighAverageAwardResponse> collectionResponse = new()
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<HighAverageAwardResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighAverageAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<HighAverage> cut = Render<HighAverage>();

        // Assert - Both tied bowlers should show RECORD
        var recordCount = cut.Markup.Split("RECORD").Length - 1;
        recordCount.ShouldBe(2);
    }
}
