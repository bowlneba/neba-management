using System.Reflection;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neba.Contracts;
using Neba.Tests;
using Neba.Tests.Website;
using Neba.Web.Server.History.Champions;
using Neba.Web.Server.Services;
using Neba.Website.Contracts.Titles;

namespace Neba.WebTests.History.Champions;
[Trait("Category", "Web")]
[Trait("Component", "History.Champions")]

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
        using TestApiResponse<CollectionResponse<TitleSummaryResponse>> response = ApiResponseFactory.CreateSuccessResponse(new CollectionResponse<TitleSummaryResponse> { Items = new List<TitleSummaryResponse> { TitleSummaryResponseFactory.Create() } });

        _mockNebaApi
            .Setup(x => x.GetTitlesSummaryAsync())
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
            .Setup(x => x.GetTitlesSummaryAsync())
            .ThrowsAsync(new InvalidOperationException("API Error"));

        // Act
        IRenderedComponent<Web.Server.History.Champions.Champions> component = Render<Neba.Web.Server.History.Champions.Champions>();

        // Assert - Component should render without throwing even when API fails
        component.ShouldNotBeNull();
        component.Instance.ShouldBeOfType<Neba.Web.Server.History.Champions.Champions>();
    }

    [Fact]
    public async Task HandleViewChanged_ValidView_SwitchesView()
    {
        // Arrange - Set up mocks for both summary and titles data to simulate view switching scenario
        using TestApiResponse<CollectionResponse<TitleSummaryResponse>> summaryResponse = ApiResponseFactory.CreateSuccessResponse(new CollectionResponse<TitleSummaryResponse> { Items = new List<TitleSummaryResponse> { TitleSummaryResponseFactory.Create() } });
        using TestApiResponse<CollectionResponse<TitleResponse>> titlesResponse = ApiResponseFactory.CreateSuccessResponse(new CollectionResponse<TitleResponse> { Items = new List<TitleResponse> { TitleResponseFactory.Bogus() } });

        _mockNebaApi
            .Setup(x => x.GetTitlesSummaryAsync())
            .ReturnsAsync(summaryResponse.ApiResponse);
        _mockNebaApi
            .Setup(x => x.GetAllTitlesAsync())
            .ReturnsAsync(titlesResponse.ApiResponse);

        IRenderedComponent<Neba.Web.Server.History.Champions.Champions> cut = Render<Neba.Web.Server.History.Champions.Champions>();
        Web.Server.History.Champions.Champions instance = cut.Instance;

        // Get private fields via reflection
        FieldInfo? selectedViewField = typeof(Neba.Web.Server.History.Champions.Champions).GetField("_selectedView", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(selectedViewField);
        FieldInfo? titlesByYearField = typeof(Neba.Web.Server.History.Champions.Champions).GetField("_titlesByYear", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(titlesByYearField);

        // Assert initial state
        ChampionsView initialView = (ChampionsView)selectedViewField.GetValue(instance)!;
        Assert.Equal(ChampionsView.TitleCount, initialView);

        List<TitlesByYearViewModel>? initialTitlesByYear = (List<TitlesByYearViewModel>?)titlesByYearField.GetValue(instance);
        Assert.Null(initialTitlesByYear);

        // Act - Invoke HandleViewChanged to switch to Year view
        MethodInfo? handleViewChangedMethod = typeof(Neba.Web.Server.History.Champions.Champions).GetMethod("HandleViewChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(handleViewChangedMethod);
        await (Task)handleViewChangedMethod.Invoke(instance, new object[] { "Year" })!;

        // Assert - View should have switched and titlesByYear should be loaded
        ChampionsView newView = (ChampionsView)selectedViewField.GetValue(instance)!;
        Assert.Equal(ChampionsView.Year, newView);

        List<TitlesByYearViewModel>? loadedTitlesByYear = (List<TitlesByYearViewModel>?)titlesByYearField.GetValue(instance);
        Assert.NotNull(loadedTitlesByYear);
        Assert.NotEmpty(loadedTitlesByYear);
    }

    [Fact]
    public void Render_WithData_IncludesModal()
    {
        // Arrange - Set up successful API response with champion data
        using TestApiResponse<CollectionResponse<TitleSummaryResponse>> response = ApiResponseFactory.CreateSuccessResponse(new CollectionResponse<TitleSummaryResponse> { Items = new List<TitleSummaryResponse> { TitleSummaryResponseFactory.Create() } });

        _mockNebaApi
            .Setup(x => x.GetTitlesSummaryAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Champions.Champions> cut = Render<Neba.Web.Server.History.Champions.Champions>();

        // Assert - Component should render and include the modal component
        cut.ShouldNotBeNull();

        // Verify that the BowlerTitlesModal component is present in the rendered output
        IRenderedComponent<BowlerTitlesModal> modalComponent = cut.FindComponent<Neba.Web.Server.History.Champions.BowlerTitlesModal>();
        modalComponent.ShouldNotBeNull();
    }
}
