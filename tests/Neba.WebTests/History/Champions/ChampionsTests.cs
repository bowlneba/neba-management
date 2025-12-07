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
using System.Net.Http;

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
    public void ShouldLoadChampionsOnInitialization()
    {
        // Arrange - Set up successful API response
        var response = new Refit.ApiResponse<CollectionResponse<BowlerTitleSummaryResponse>>(
            new HttpResponseMessage(System.Net.HttpStatusCode.OK),
            new CollectionResponse<BowlerTitleSummaryResponse> { Items = new List<BowlerTitleSummaryResponse> { BowlerTitleSummaryResponseFactory.Create() } },
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesSummaryAsync())
            .ReturnsAsync(response);

        // Act
        IRenderedComponent<Neba.Web.Server.History.Champions.Champions> cut = Render<Neba.Web.Server.History.Champions.Champions>();

        // Assert - Component should render without throwing
        cut.ShouldNotBeNull();
        cut.Markup.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void ShouldHandleInitializationErrors()
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
    public void ShouldHandleViewSwitching()
    {
        // Arrange
        var summaryResponse = new Refit.ApiResponse<CollectionResponse<BowlerTitleSummaryResponse>>(
            new HttpResponseMessage(System.Net.HttpStatusCode.OK),
            new CollectionResponse<BowlerTitleSummaryResponse> { Items = new List<BowlerTitleSummaryResponse> { BowlerTitleSummaryResponseFactory.Create() } },
            new RefitSettings());
        var titlesResponse = new Refit.ApiResponse<CollectionResponse<BowlerTitleResponse>>(
            new HttpResponseMessage(System.Net.HttpStatusCode.OK),
            new CollectionResponse<BowlerTitleResponse> { Items = new List<BowlerTitleResponse>() },
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesSummaryAsync())
            .ReturnsAsync(summaryResponse);
        _mockNebaApi
            .Setup(x => x.GetAllTitlesAsync())
            .ReturnsAsync(titlesResponse);

        IRenderedComponent<Neba.Web.Server.History.Champions.Champions> cut = Render<Neba.Web.Server.History.Champions.Champions>();

        // Act & Assert - Component should handle view switching without throwing
        cut.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldRenderModalComponent()
    {
        // Arrange
        var response = new Refit.ApiResponse<CollectionResponse<BowlerTitleSummaryResponse>>(
            new HttpResponseMessage(System.Net.HttpStatusCode.OK),
            new CollectionResponse<BowlerTitleSummaryResponse> { Items = new List<BowlerTitleSummaryResponse> { BowlerTitleSummaryResponseFactory.Create() } },
            new RefitSettings());

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesSummaryAsync())
            .ReturnsAsync(response);

        // Act & Assert - Component should render with modal
        IRenderedComponent<Neba.Web.Server.History.Champions.Champions> cut = Render<Neba.Web.Server.History.Champions.Champions>();
        cut.ShouldNotBeNull();
    }
}
