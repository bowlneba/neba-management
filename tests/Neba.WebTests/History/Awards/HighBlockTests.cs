using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neba.Contracts;
using Neba.Contracts.Website.Awards;
using Neba.Tests;
using Neba.Web.Server.Services;

namespace Neba.WebTests.History.Awards;

public sealed class HighBlockTests : TestContextWrapper
{
    private readonly Mock<INebaApi> _mockNebaApi;
    private readonly NebaApiService _nebaApiService;

    public HighBlockTests()
    {
        _mockNebaApi = new Mock<INebaApi>();
        _nebaApiService = new NebaApiService(_mockNebaApi.Object);
        TestContext.Services.AddSingleton(_nebaApiService);
    }

    [Fact]
    public void OnInitializedAsync_SuccessfulApiResponse_LoadsAwards()
    {
        // Arrange - Set up successful API response
        var awards = new List<HighBlockAwardResponse>
        {
            HighBlockAwardResponseFactory.Create(bowlerName: "John Doe", season: "2024", score: 1200),
            HighBlockAwardResponseFactory.Create(bowlerName: "Jane Smith", season: "2023", score: 1180),
            HighBlockAwardResponseFactory.Create(bowlerName: "Bob Johnson", season: "2022", score: 1150)
        };

        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = awards
        };

        using var response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.HighBlock> cut = Render<Neba.Web.Server.History.Awards.HighBlock>();

        // Assert - Component should render without throwing
        cut.ShouldNotBeNull();
        cut.Markup.ShouldNotBeNullOrEmpty();

        // Verify the page title is present
        cut.Markup.ShouldContain("High Block");
    }

    [Fact]
    public void OnInitializedAsync_SuccessfulApiResponse_DisplaysAwardsByYear()
    {
        // Arrange
        var awards = new List<HighBlockAwardResponse>
        {
            HighBlockAwardResponseFactory.Create(bowlerName: "John Doe", season: "2024", score: 1200),
            HighBlockAwardResponseFactory.Create(bowlerName: "Jane Smith", season: "2023", score: 1180),
            HighBlockAwardResponseFactory.Create(bowlerName: "Bob Johnson", season: "2022", score: 1150),
            HighBlockAwardResponseFactory.Create(bowlerName: "Alice Brown", season: "2021", score: 1125)
        };

        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = awards
        };

        using var response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.HighBlock> cut = Render<Neba.Web.Server.History.Awards.HighBlock>();

        // Assert - Verify awards are displayed
        cut.Markup.ShouldContain("2024");
        cut.Markup.ShouldContain("2023");
        cut.Markup.ShouldContain("2022");
        cut.Markup.ShouldContain("2021");
        cut.Markup.ShouldContain("John Doe");
        cut.Markup.ShouldContain("Jane Smith");
        cut.Markup.ShouldContain("Bob Johnson");
        cut.Markup.ShouldContain("Alice Brown");
        cut.Markup.ShouldContain("1200");
        cut.Markup.ShouldContain("1180");
        cut.Markup.ShouldContain("1150");
        cut.Markup.ShouldContain("1125");
    }

    [Fact]
    public void OnInitializedAsync_TiedScores_DisplaysTieIndicator()
    {
        // Arrange
        var awards = new List<HighBlockAwardResponse>
        {
            HighBlockAwardResponseFactory.Create(bowlerName: "John Doe", season: "2024", score: 1200),
            HighBlockAwardResponseFactory.Create(bowlerName: "Jane Smith", season: "2024", score: 1200),
            HighBlockAwardResponseFactory.Create(bowlerName: "Bob Johnson", season: "2023", score: 1180)
        };

        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = awards
        };

        using var response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.HighBlock> cut = Render<Neba.Web.Server.History.Awards.HighBlock>();

        // Assert - Verify tie indicator is displayed
        cut.Markup.ShouldContain("TIE");
        cut.Markup.ShouldContain("Jane Smith, John Doe"); // Should be alphabetically ordered
        cut.Markup.ShouldContain("1200");
    }

    [Fact]
    public void OnInitializedAsync_EmptyResponse_DisplaysNoDataMessage()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = new List<HighBlockAwardResponse>()
        };

        using var response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.HighBlock> cut = Render<Neba.Web.Server.History.Awards.HighBlock>();

        // Assert - Should display "no data" message
        cut.Markup.ShouldContain("No high block awards data available");
    }

    [Fact]
    public void OnInitializedAsync_ApiError_HandlesErrorGracefully()
    {
        // Arrange - Simulate API error during initialization
        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ThrowsAsync(new InvalidOperationException("API Error"));

        // Act
        var component = Render<Neba.Web.Server.History.Awards.HighBlock>();

        // Assert - Component should render without throwing even when API fails
        component.ShouldNotBeNull();
        component.Instance.ShouldBeOfType<Neba.Web.Server.History.Awards.HighBlock>();

        // Should display error alert with appropriate message (error comes from ApiErrors.Unexpected)
        component.Markup.ShouldContain("Error Loading Awards");
        component.Markup.ShouldContain("An unexpected error occurred");
        component.Markup.ShouldContain("API Error");
    }

    [Fact]
    public void OnInitializedAsync_ApiReturnsError_DisplaysErrorAlert()
    {
        // Arrange
        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = new List<HighBlockAwardResponse>()
        };

        using var response = ApiResponseFactory.CreateResponse(collectionResponse, System.Net.HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.HighBlock> cut = Render<Neba.Web.Server.History.Awards.HighBlock>();

        // Assert - Should display error alert when API returns error status
        cut.Markup.ShouldContain("Error Loading Awards");
    }

    [Fact]
    public void Render_WithData_IncludesLoadingIndicator()
    {
        // Arrange
        var awards = new List<HighBlockAwardResponse>
        {
            HighBlockAwardResponseFactory.Create(bowlerName: "John Doe", season: "2024", score: 1200)
        };

        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = awards
        };

        using var response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.HighBlock> cut = Render<Neba.Web.Server.History.Awards.HighBlock>();

        // Assert - Component should render and include the loading indicator component
        cut.ShouldNotBeNull();

        // Verify that the NebaLoadingIndicator component is present in the rendered output
        var loadingIndicator = cut.FindComponent<Neba.Web.Server.Components.NebaLoadingIndicator>();
        loadingIndicator.ShouldNotBeNull();
    }

    [Fact]
    public void OnInitializedAsync_MultipleAwardsInSameSeason_HandlesTieCorrectly()
    {
        // Arrange - Three-way tie
        var awards = new List<HighBlockAwardResponse>
        {
            HighBlockAwardResponseFactory.Create(bowlerName: "John Doe", season: "2024", score: 1200),
            HighBlockAwardResponseFactory.Create(bowlerName: "Jane Smith", season: "2024", score: 1200),
            HighBlockAwardResponseFactory.Create(bowlerName: "Bob Senior", season: "2024", score: 1200)
        };

        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = awards
        };

        using var response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.HighBlock> cut = Render<Neba.Web.Server.History.Awards.HighBlock>();

        // Assert - All names should be displayed with TIE indicator
        cut.Markup.ShouldContain("2024");
        cut.Markup.ShouldContain("TIE");
        cut.Markup.ShouldContain("Bob Senior, Jane Smith, John Doe"); // Alphabetically ordered
        cut.Markup.ShouldContain("1200");
    }

    [Fact]
    public void Render_DisplaysDescriptiveText()
    {
        // Arrange
        var awards = new List<HighBlockAwardResponse>
        {
            HighBlockAwardResponseFactory.Create()
        };

        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = awards
        };

        using var response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.HighBlock> cut = Render<Neba.Web.Server.History.Awards.HighBlock>();

        // Assert - Verify descriptive text is present
        cut.Markup.ShouldContain("High Block");
        cut.Markup.ShouldContain("five-game block score");
        cut.Markup.ShouldContain("exceptional sustained performance");
    }

    [Fact]
    public void OnInitializedAsync_DisplaysCorrectTableHeaders()
    {
        // Arrange
        var awards = new List<HighBlockAwardResponse>
        {
            HighBlockAwardResponseFactory.Create(bowlerName: "John Doe", season: "2024", score: 1200)
        };

        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = awards
        };

        using var response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.HighBlock> cut = Render<Neba.Web.Server.History.Awards.HighBlock>();

        // Assert - Verify table headers are present
        cut.Markup.ShouldContain("Year");
        cut.Markup.ShouldContain("Bowler(s)");
        cut.Markup.ShouldContain("Score");
    }

    [Fact]
    public void OnInitializedAsync_SingleBowler_NoTieIndicator()
    {
        // Arrange
        var awards = new List<HighBlockAwardResponse>
        {
            HighBlockAwardResponseFactory.Create(bowlerName: "John Doe", season: "2024", score: 1200)
        };

        var collectionResponse = new CollectionResponse<HighBlockAwardResponse>
        {
            Items = awards
        };

        using var response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetHighBlockAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.HighBlock> cut = Render<Neba.Web.Server.History.Awards.HighBlock>();

        // Assert - TIE indicator should NOT be present for single bowler
        cut.Markup.ShouldNotContain("TIE");
        cut.Markup.ShouldContain("John Doe");
        cut.Markup.ShouldContain("1200");
    }
}
