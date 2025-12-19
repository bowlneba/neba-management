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

public sealed class BowlerOfTheYearTests : TestContextWrapper
{
    private readonly Mock<INebaApi> _mockNebaApi;
    private readonly NebaApiService _nebaApiService;

    public BowlerOfTheYearTests()
    {
        _mockNebaApi = new Mock<INebaApi>();
        _nebaApiService = new NebaApiService(_mockNebaApi.Object);
        TestContext.Services.AddSingleton(_nebaApiService);
    }

    [Fact]
    public void OnInitializedAsync_SuccessfulApiResponse_LoadsAwards()
    {
        // Arrange - Set up successful API response
        List<BowlerOfTheYearResponse> awards = new List<BowlerOfTheYearResponse>
        {
            BowlerOfTheYearResponseFactory.Create("John Doe", "2024", "Open"),
            BowlerOfTheYearResponseFactory.Create("Jane Smith", "2024", "Woman"),
            BowlerOfTheYearResponseFactory.Create("Bob Johnson", "2023", "Open")
        };

        CollectionResponse<BowlerOfTheYearResponse> collectionResponse = new CollectionResponse<BowlerOfTheYearResponse>
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<BowlerOfTheYearResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.BowlerOfTheYear> cut = Render<Neba.Web.Server.History.Awards.BowlerOfTheYear>();

        // Assert - Component should render without throwing
        cut.ShouldNotBeNull();
        cut.Markup.ShouldNotBeNullOrEmpty();

        // Verify the page title is present
        cut.Markup.ShouldContain("Bowler of the Year");
    }

    [Fact]
    public void OnInitializedAsync_SuccessfulApiResponse_DisplaysAwardsByYear()
    {
        // Arrange
        List<BowlerOfTheYearResponse> awards = new List<BowlerOfTheYearResponse>
        {
            BowlerOfTheYearResponseFactory.Create("John Doe", "2024", "Open"),
            BowlerOfTheYearResponseFactory.Create("Jane Smith", "2024", "Woman"),
            BowlerOfTheYearResponseFactory.Create("Bob Johnson", "2023", "Open"),
            BowlerOfTheYearResponseFactory.Create("Alice Brown", "2023", "Senior")
        };

        CollectionResponse<BowlerOfTheYearResponse> collectionResponse = new CollectionResponse<BowlerOfTheYearResponse>
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<BowlerOfTheYearResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.BowlerOfTheYear> cut = Render<Neba.Web.Server.History.Awards.BowlerOfTheYear>();

        // Assert - Verify awards are displayed
        cut.Markup.ShouldContain("2024");
        cut.Markup.ShouldContain("2023");
        cut.Markup.ShouldContain("John Doe");
        cut.Markup.ShouldContain("Jane Smith");
        cut.Markup.ShouldContain("Bob Johnson");
        cut.Markup.ShouldContain("Alice Brown");
        cut.Markup.ShouldContain("Open");
        cut.Markup.ShouldContain("Woman");
        cut.Markup.ShouldContain("Senior");
    }

    [Fact]
    public void OnInitializedAsync_EmptyResponse_DisplaysNoDataMessage()
    {
        // Arrange
        CollectionResponse<BowlerOfTheYearResponse> collectionResponse = new CollectionResponse<BowlerOfTheYearResponse>
        {
            Items = new List<BowlerOfTheYearResponse>()
        };

        using TestApiResponse<CollectionResponse<BowlerOfTheYearResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.BowlerOfTheYear> cut = Render<Neba.Web.Server.History.Awards.BowlerOfTheYear>();

        // Assert - Should display "no data" message
        cut.Markup.ShouldContain("No awards data available");
    }

    [Fact]
    public void OnInitializedAsync_ApiError_HandlesErrorGracefully()
    {
        // Arrange - Simulate API error during initialization
        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ThrowsAsync(new InvalidOperationException("API Error"));

        // Act
        IRenderedComponent<BowlerOfTheYear> component = Render<Neba.Web.Server.History.Awards.BowlerOfTheYear>();

        // Assert - Component should render without throwing even when API fails
        component.ShouldNotBeNull();
        component.Instance.ShouldBeOfType<Neba.Web.Server.History.Awards.BowlerOfTheYear>();

        // Should display error alert with appropriate message
        component.Markup.ShouldContain("Error Loading Awards");
        component.Markup.ShouldContain("Failed to load awards data");
        component.Markup.ShouldContain("API Error");
    }

    [Fact]
    public void OnInitializedAsync_ApiReturnsError_DisplaysErrorAlert()
    {
        // Arrange
        CollectionResponse<BowlerOfTheYearResponse> collectionResponse = new CollectionResponse<BowlerOfTheYearResponse>
        {
            Items = new List<BowlerOfTheYearResponse>()
        };

        using TestApiResponse<CollectionResponse<BowlerOfTheYearResponse>> response = ApiResponseFactory.CreateResponse(collectionResponse, System.Net.HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.BowlerOfTheYear> cut = Render<Neba.Web.Server.History.Awards.BowlerOfTheYear>();

        // Assert - Should display error alert when API returns error status
        cut.Markup.ShouldContain("Error Loading Awards");
        cut.Markup.ShouldContain("Failed to load awards data");
    }

    [Fact]
    public void Render_WithData_IncludesLoadingIndicator()
    {
        // Arrange
        List<BowlerOfTheYearResponse> awards = new List<BowlerOfTheYearResponse>
        {
            BowlerOfTheYearResponseFactory.Create("John Doe", "2024", "Open")
        };

        CollectionResponse<BowlerOfTheYearResponse> collectionResponse = new CollectionResponse<BowlerOfTheYearResponse>
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<BowlerOfTheYearResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.BowlerOfTheYear> cut = Render<Neba.Web.Server.History.Awards.BowlerOfTheYear>();

        // Assert - Component should render and include the loading indicator component
        cut.ShouldNotBeNull();

        // Verify that the NebaLoadingIndicator component is present in the rendered output
        IRenderedComponent<NebaLoadingIndicator> loadingIndicator = cut.FindComponent<Neba.Web.Server.Components.NebaLoadingIndicator>();
        loadingIndicator.ShouldNotBeNull();
    }

    [Fact]
    public void OnInitializedAsync_MultipleAwardsInSameSeason_GroupsCorrectly()
    {
        // Arrange
        List<BowlerOfTheYearResponse> awards = new List<BowlerOfTheYearResponse>
        {
            BowlerOfTheYearResponseFactory.Create("John Doe", "2024", "Open"),
            BowlerOfTheYearResponseFactory.Create("Jane Smith", "2024", "Woman"),
            BowlerOfTheYearResponseFactory.Create("Bob Senior", "2024", "Senior"),
            BowlerOfTheYearResponseFactory.Create("Alice Youth", "2024", "Youth"),
            BowlerOfTheYearResponseFactory.Create("Charlie Rookie", "2024", "Rookie")
        };

        CollectionResponse<BowlerOfTheYearResponse> collectionResponse = new CollectionResponse<BowlerOfTheYearResponse>
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<BowlerOfTheYearResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.BowlerOfTheYear> cut = Render<Neba.Web.Server.History.Awards.BowlerOfTheYear>();

        // Assert - All categories and names should be displayed under 2024
        cut.Markup.ShouldContain("2024");
        cut.Markup.ShouldContain("John Doe");
        cut.Markup.ShouldContain("Jane Smith");
        cut.Markup.ShouldContain("Bob Senior");
        cut.Markup.ShouldContain("Alice Youth");
        cut.Markup.ShouldContain("Charlie Rookie");
        cut.Markup.ShouldContain("Open");
        cut.Markup.ShouldContain("Woman");
        cut.Markup.ShouldContain("Senior");
        cut.Markup.ShouldContain("Youth");
        cut.Markup.ShouldContain("Rookie");
    }

    [Fact]
    public void Render_DisplaysDescriptiveText()
    {
        // Arrange
        List<BowlerOfTheYearResponse> awards = new List<BowlerOfTheYearResponse>
        {
            BowlerOfTheYearResponseFactory.Create()
        };

        CollectionResponse<BowlerOfTheYearResponse> collectionResponse = new CollectionResponse<BowlerOfTheYearResponse>
        {
            Items = awards
        };

        using TestApiResponse<CollectionResponse<BowlerOfTheYearResponse>> response = ApiResponseFactory.CreateSuccessResponse(collectionResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerOfTheYearAwardsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Awards.BowlerOfTheYear> cut = Render<Neba.Web.Server.History.Awards.BowlerOfTheYear>();

        // Assert - Verify descriptive text is present
        cut.Markup.ShouldContain("George Chikar");
        cut.Markup.ShouldContain("highest competitive distinction");
        cut.Markup.ShouldContain("season-long excellence");
    }
}
