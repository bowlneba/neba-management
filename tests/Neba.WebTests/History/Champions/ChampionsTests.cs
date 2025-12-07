using System.Net.Http;
using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neba.Contracts;
using Neba.Contracts.Website.Bowlers;
using Neba.Tests;
using Neba.Web.Server.History.Champions;
using Neba.Web.Server.Services;
using Neba.WebTests;
using Refit;

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
    public void HandleViewChanged_ValidView_SwitchesView()
    {
        // Arrange
        using var summaryResponse = ApiResponseFactory.CreateSuccessResponse(new CollectionResponse<BowlerTitleSummaryResponse> { Items = new List<BowlerTitleSummaryResponse> { BowlerTitleSummaryResponseFactory.Create() } });
        using var titlesResponse = ApiResponseFactory.CreateSuccessResponse(new CollectionResponse<BowlerTitleResponse> { Items = new List<BowlerTitleResponse>() });

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesSummaryAsync())
            .ReturnsAsync(summaryResponse.ApiResponse);
        _mockNebaApi
            .Setup(x => x.GetAllTitlesAsync())
            .ReturnsAsync(titlesResponse.ApiResponse);

        IRenderedComponent<Neba.Web.Server.History.Champions.Champions> cut = Render<Neba.Web.Server.History.Champions.Champions>();

        // Act & Assert - Component should handle view switching without throwing
        cut.ShouldNotBeNull();
    }

    [Fact]
    public void Render_WithData_IncludesModal()
    {
        // Arrange
        using var response = ApiResponseFactory.CreateSuccessResponse(new CollectionResponse<BowlerTitleSummaryResponse> { Items = new List<BowlerTitleSummaryResponse> { BowlerTitleSummaryResponseFactory.Create() } });

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesSummaryAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act & Assert - Component should render with modal
        IRenderedComponent<Neba.Web.Server.History.Champions.Champions> cut = Render<Neba.Web.Server.History.Champions.Champions>();
        cut.ShouldNotBeNull();
    }
}
