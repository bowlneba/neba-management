using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neba.Contracts;
using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Tests;
using Neba.Tests.Website;
using Neba.Web.Server.Components;
using Neba.Web.Server.History.Champions;
using Neba.Web.Server.Services;
using Neba.Website.Contracts.Bowlers;

namespace Neba.WebTests.History.Champions;

[Trait("Category", "Web")]
[Trait("Component", "History.Champions")]
public sealed class BowlerTitlesModalTests : TestContextWrapper
{
    private readonly Mock<INebaWebsiteApi> _mockNebaApi;
    private readonly NebaWebsiteApiService _nebaWebsiteApiService;

    public BowlerTitlesModalTests()
    {
        _mockNebaApi = new Mock<INebaWebsiteApi>();
        _nebaWebsiteApiService = new NebaWebsiteApiService(_mockNebaApi.Object);
        TestContext.Services.AddSingleton(_nebaWebsiteApiService);
    }

    [Fact]
    public async Task WhenModalOpens_LoadsTitles()
    {
        // Arrange
        var bowlerId = BowlerId.New();
        var titles = new List<BowlerTitleResponse>
        {
            BowlerTitleResponseFactory.Create(month: Month.March, year: 2024, tournamentType: "Singles"),
            BowlerTitleResponseFactory.Create(month: Month.June, year: 2023, tournamentType: "Doubles")
        };

        var bowlerTitles = BowlerTitlesResponseFactory.Create(
            bowlerId: bowlerId,
            bowlerName: "John Champion",
            hallOfFame: false,
            titles: titles);

        ApiResponse<BowlerTitlesResponse> apiResponse = new()
        {
            Data = bowlerTitles
        };

        using TestApiResponse<ApiResponse<BowlerTitlesResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(apiResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<BowlerTitlesModal> cut = Render<BowlerTitlesModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.BowlerId, bowlerId)
            .Add(c => c.BowlerName, "John Champion")
            .Add(c => c.HallOfFame, false)
            .Add(c => c.OnClose, EventCallback.Empty));

        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("John Champion");
        cut.Markup.ShouldContain("2 Titles");
    }

    [Fact]
    public async Task WhenModalOpens_WithHallOfFameBowler_DisplaysHofBadge()
    {
        // Arrange
        var bowlerId = BowlerId.New();
        var titles = new List<BowlerTitleResponse>
        {
            BowlerTitleResponseFactory.Create(month: Month.May, year: 2024, tournamentType: "Singles")
        };

        var bowlerTitles = BowlerTitlesResponseFactory.Create(
            bowlerId: bowlerId,
            bowlerName: "Hall of Fame Bowler",
            hallOfFame: true,
            titles: titles);

        ApiResponse<BowlerTitlesResponse> apiResponse = new()
        {
            Data = bowlerTitles
        };

        using TestApiResponse<ApiResponse<BowlerTitlesResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(apiResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<BowlerTitlesModal> cut = Render<BowlerTitlesModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.BowlerId, bowlerId)
            .Add(c => c.BowlerName, "Hall of Fame Bowler")
            .Add(c => c.HallOfFame, true)
            .Add(c => c.OnClose, EventCallback.Empty));

        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("neba-hof.jpg");
        cut.Markup.ShouldContain("Hall of Fame");
    }

    [Fact]
    public async Task WhenApiReturnsError_DisplaysErrorMessage()
    {
        // Arrange
        var bowlerId = BowlerId.New();

        var bowlerTitles = BowlerTitlesResponseFactory.Create(bowlerId: bowlerId, titleCount: 1);

        ApiResponse<BowlerTitlesResponse> apiResponse = new()
        {
            Data = bowlerTitles
        };

        using TestApiResponse<ApiResponse<BowlerTitlesResponse>> response =
            ApiResponseFactory.CreateResponse(apiResponse, System.Net.HttpStatusCode.InternalServerError);

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<BowlerTitlesModal> cut = Render<BowlerTitlesModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.BowlerId, bowlerId)
            .Add(c => c.BowlerName, "Error Bowler")
            .Add(c => c.OnClose, EventCallback.Empty));

        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("Failed to load titles");
    }

    [Fact]
    public async Task WhenApiThrowsException_DisplaysErrorMessage()
    {
        // Arrange
        var bowlerId = BowlerId.New();

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ThrowsAsync(new InvalidOperationException("Network error"));

        // Act
        IRenderedComponent<BowlerTitlesModal> cut = Render<BowlerTitlesModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.BowlerId, bowlerId)
            .Add(c => c.BowlerName, "Exception Bowler")
            .Add(c => c.OnClose, EventCallback.Empty));

        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("Failed to load titles");
        cut.Markup.ShouldContain("Network error");
    }

    [Fact]
    public async Task WhenBowlerHasNoTitles_DisplaysNoTitlesMessage()
    {
        // Arrange
        var bowlerId = BowlerId.New();

        var bowlerTitles = BowlerTitlesResponseFactory.Create(
            bowlerId: bowlerId,
            bowlerName: "No Titles Bowler",
            hallOfFame: false,
            titles: Array.Empty<BowlerTitleResponse>());

        ApiResponse<BowlerTitlesResponse> apiResponse = new()
        {
            Data = bowlerTitles
        };

        using TestApiResponse<ApiResponse<BowlerTitlesResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(apiResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<BowlerTitlesModal> cut = Render<BowlerTitlesModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.BowlerId, bowlerId)
            .Add(c => c.BowlerName, "No Titles Bowler")
            .Add(c => c.OnClose, EventCallback.Empty));

        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("No titles found for this bowler");
    }

    [Fact]
    public async Task WhenModalIsClosed_DoesNotShowTitles()
    {
        // Arrange
        var bowlerId = BowlerId.New();

        // Act - Render modal in closed state
        IRenderedComponent<BowlerTitlesModal> cut = Render<BowlerTitlesModal>(p => p
            .Add(c => c.IsOpen, false)
            .Add(c => c.BowlerId, bowlerId)
            .Add(c => c.BowlerName, "Test Bowler")
            .Add(c => c.OnClose, EventCallback.Empty));

        await Task.Delay(100);

        // Assert - API should not be called when modal is closed
        _mockNebaApi.Verify(x => x.GetBowlerTitlesAsync(It.IsAny<BowlerId>()), Times.Never);
    }

    [Fact]
    public async Task DisplaysTitlesInTable()
    {
        // Arrange
        var bowlerId = BowlerId.New();
        var titles = new List<BowlerTitleResponse>
        {
            BowlerTitleResponseFactory.Create(month: Month.March, year: 2024, tournamentType: "Singles"),
            BowlerTitleResponseFactory.Create(month: Month.October, year: 2023, tournamentType: "Doubles")
        };

        var bowlerTitles = BowlerTitlesResponseFactory.Create(
            bowlerId: bowlerId,
            bowlerName: "Champion",
            hallOfFame: false,
            titles: titles);

        ApiResponse<BowlerTitlesResponse> apiResponse = new()
        {
            Data = bowlerTitles
        };

        using TestApiResponse<ApiResponse<BowlerTitlesResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(apiResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<BowlerTitlesModal> cut = Render<BowlerTitlesModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.BowlerId, bowlerId)
            .Add(c => c.BowlerName, "Champion")
            .Add(c => c.OnClose, EventCallback.Empty));

        await Task.Delay(100);

        // Assert
        cut.Markup.ShouldContain("Singles");
        cut.Markup.ShouldContain("Doubles");
        cut.Markup.ShouldContain("#1");
        cut.Markup.ShouldContain("#2");
    }

    [Fact]
    public async Task IncludesNebaModalComponent()
    {
        // Arrange
        var bowlerId = BowlerId.New();
        var bowlerTitles = BowlerTitlesResponseFactory.Create(bowlerId: bowlerId, titleCount: 1);

        ApiResponse<BowlerTitlesResponse> apiResponse = new()
        {
            Data = bowlerTitles
        };

        using TestApiResponse<ApiResponse<BowlerTitlesResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(apiResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<BowlerTitlesModal> cut = Render<BowlerTitlesModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.BowlerId, bowlerId)
            .Add(c => c.BowlerName, "Test")
            .Add(c => c.OnClose, EventCallback.Empty));

        // Assert
        IRenderedComponent<NebaModal> modal = cut.FindComponent<NebaModal>();
        modal.ShouldNotBeNull();
    }

    [Fact]
    public async Task WhenNoBowlerId_DoesNotCallApi()
    {
        // Act
        IRenderedComponent<BowlerTitlesModal> cut = Render<BowlerTitlesModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.BowlerId, null)
            .Add(c => c.BowlerName, "Unknown")
            .Add(c => c.OnClose, EventCallback.Empty));

        await Task.Delay(100);

        // Assert
        _mockNebaApi.Verify(x => x.GetBowlerTitlesAsync(It.IsAny<BowlerId>()), Times.Never);
    }

    [Fact]
    public void WhenBowlerNameIsNull_UsesDefaultTitle()
    {
        // Act
        IRenderedComponent<BowlerTitlesModal> cut = Render<BowlerTitlesModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.BowlerId, null)
            .Add(c => c.BowlerName, null)
            .Add(c => c.OnClose, EventCallback.Empty));

        // Assert
        cut.Markup.ShouldContain("Bowler Titles");
    }

    [Fact]
    public async Task CloseButton_InvokesOnCloseCallback()
    {
        // Arrange
        var bowlerId = BowlerId.New();
        var bowlerTitles = BowlerTitlesResponseFactory.Create(bowlerId: bowlerId, titleCount: 1);

        ApiResponse<BowlerTitlesResponse> apiResponse = new()
        {
            Data = bowlerTitles
        };

        using TestApiResponse<ApiResponse<BowlerTitlesResponse>> response =
            ApiResponseFactory.CreateSuccessResponse(apiResponse);

        _mockNebaApi
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(response.ApiResponse);

        bool closeCalled = false;

        // Act
        IRenderedComponent<BowlerTitlesModal> cut = Render<BowlerTitlesModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.BowlerId, bowlerId)
            .Add(c => c.BowlerName, "Test")
            .Add(c => c.OnClose, EventCallback.Factory.Create(this, () => closeCalled = true)));

        await Task.Delay(100);

        // Find and click close button
        var closeButton = cut.Find("button.neba-modal-close");
        await closeButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        closeCalled.ShouldBeTrue();
    }
}
