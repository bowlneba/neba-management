using System.Reflection;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neba.Contracts;
using Neba.Contracts.Website.Bowlers;
using Neba.Tests;
using Neba.Web.Server.History.Champions;
using Neba.Web.Server.Services;

namespace Neba.WebTests.History.Champions;

public sealed class ChampionsTests : TestContextWrapper
{
    private readonly Mock<INebaApi> _mockNebaApi;
    private readonly NebaApiService _nebaApiService;

    public ChampionsTests()
    {
        _mockNebaApi = new Mock<INebaApi>();
        _nebaApiService = new NebaApiService(_mockNebaApi.Object);
        TestContext.Services.AddSingleton(_nebaApiService);
    }

    [Fact]
    public void OnInitializedAsync_SuccessfulApiResponse_LoadsChampions()
    {
        // Arrange - Set up successful API response
        using var response = ApiResponseFactory.CreateSuccessResponse(new CollectionResponse<BowlerTitleSummaryResponse> { Items = new List<BowlerTitleSummaryResponse> { BowlerTitleSummaryResponseFactory.Create() } });

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesSummaryAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Champions.Champions> cut = Render<Neba.Web.Server.History.Champions.Champions>();

        // Assert - Component should render without throwing
        cut.ShouldNotBeNull();
        cut.Markup.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void OnInitializedAsync_ApiError_HandlesErrorGracefully()
    {
        // Arrange - Simulate API error during initialization
        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesSummaryAsync())
            .ThrowsAsync(new InvalidOperationException("API Error"));

        // Act
        var component = Render<Neba.Web.Server.History.Champions.Champions>();

        // Assert - Component should render without throwing even when API fails
        component.ShouldNotBeNull();
        component.Instance.ShouldBeOfType<Neba.Web.Server.History.Champions.Champions>();
    }

    [Fact]
    public async Task HandleViewChanged_ValidView_SwitchesView()
    {
        // Arrange - Set up mocks for both summary and titles data to simulate view switching scenario
        using var summaryResponse = ApiResponseFactory.CreateSuccessResponse(new CollectionResponse<BowlerTitleSummaryResponse> { Items = new List<BowlerTitleSummaryResponse> { BowlerTitleSummaryResponseFactory.Create() } });
        using var titlesResponse = ApiResponseFactory.CreateSuccessResponse(new CollectionResponse<BowlerTitleResponse> { Items = new List<BowlerTitleResponse> { BowlerTitleResponseFactory.Bogus() } });

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesSummaryAsync())
            .ReturnsAsync(summaryResponse.ApiResponse);
        _mockNebaApi
            .Setup(x => x.GetAllTitlesAsync())
            .ReturnsAsync(titlesResponse.ApiResponse);

        IRenderedComponent<Neba.Web.Server.History.Champions.Champions> cut = Render<Neba.Web.Server.History.Champions.Champions>();
        var instance = cut.Instance;

        // Get private fields via reflection
        var selectedViewField = typeof(Neba.Web.Server.History.Champions.Champions).GetField("selectedView", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(selectedViewField);
        var titlesByYearField = typeof(Neba.Web.Server.History.Champions.Champions).GetField("titlesByYear", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(titlesByYearField);

        // Assert initial state
        var initialView = (ChampionsView)selectedViewField.GetValue(instance)!;
        Assert.Equal(ChampionsView.TitleCount, initialView);

        var initialTitlesByYear = (List<TitlesByYearViewModel>?)titlesByYearField.GetValue(instance);
        Assert.Null(initialTitlesByYear);

        // Act - Invoke HandleViewChanged to switch to Year view
        var handleViewChangedMethod = typeof(Neba.Web.Server.History.Champions.Champions).GetMethod("HandleViewChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(handleViewChangedMethod);
        await (Task)handleViewChangedMethod.Invoke(instance, new object[] { "Year" })!;

        // Assert - View should have switched and titlesByYear should be loaded
        var newView = (ChampionsView)selectedViewField.GetValue(instance)!;
        Assert.Equal(ChampionsView.Year, newView);

        var loadedTitlesByYear = (List<TitlesByYearViewModel>?)titlesByYearField.GetValue(instance);
        Assert.NotNull(loadedTitlesByYear);
        Assert.NotEmpty(loadedTitlesByYear);
    }

    [Fact]
    public void Render_WithData_IncludesModal()
    {
        // Arrange - Set up successful API response with champion data
        using var response = ApiResponseFactory.CreateSuccessResponse(new CollectionResponse<BowlerTitleSummaryResponse> { Items = new List<BowlerTitleSummaryResponse> { BowlerTitleSummaryResponseFactory.Create() } });

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesSummaryAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Champions.Champions> cut = Render<Neba.Web.Server.History.Champions.Champions>();

        // Assert - Component should render and include the modal component
        cut.ShouldNotBeNull();

        // Verify that the BowlerTitlesModal component is present in the rendered output
        var modalComponent = cut.FindComponent<Neba.Web.Server.History.Champions.BowlerTitlesModal>();
        modalComponent.ShouldNotBeNull();
    }
}
