using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neba.Tests;
using Neba.Web.Server.Components;
using Neba.Web.Server.Services;
using Neba.Web.Server.Tournaments;

namespace Neba.WebTests.Tournaments;

public sealed class TournamentRulesTests : TestContextWrapper
{
    private readonly Mock<INebaApi> _mockNebaApi;
    private readonly NebaApiService _nebaApiService;

    public TournamentRulesTests()
    {
        _mockNebaApi = new Mock<INebaApi>();
        _nebaApiService = new NebaApiService(_mockNebaApi.Object);
        TestContext.Services.AddSingleton(_nebaApiService);
    }

    [Fact]
    public async Task ShouldRenderPageTitle()
    {
        // Arrange
        using var response = ApiResponseFactory.CreateSuccessResponse("<p>Rules content</p>");
        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentRules> cut = Render<TournamentRules>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        cut.Markup.ShouldContain("<h1");
        cut.Markup.ShouldContain("NEBA Tournament Rules");
    }

    [Fact]
    public async Task ShouldRenderPageDescription()
    {
        // Arrange
        using var response = ApiResponseFactory.CreateSuccessResponse("<p>Rules content</p>");
        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentRules> cut = Render<TournamentRules>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        cut.Markup.ShouldContain("Official rules and regulations for NEBA tournaments");
    }

    [Fact]
    public async Task ShouldCallGetTournamentRulesAsyncOnInitialization()
    {
        // Arrange
        using var response = ApiResponseFactory.CreateSuccessResponse("<h1>Tournament Rules</h1>");
        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentRules> cut = Render<TournamentRules>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        _mockNebaApi.Verify(x => x.GetTournamentRulesAsync(), Times.Once);
    }

    [Fact]
    public async Task ShouldDisplayRulesContentWhenApiCallSucceeds()
    {
        // Arrange
        var rulesHtml = "<h1>Rule 1</h1><p>Description of rule 1</p>";
        using var response = ApiResponseFactory.CreateSuccessResponse(rulesHtml);
        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentRules> cut = Render<TournamentRules>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        IRenderedComponent<NebaDocument> document = cut.FindComponent<NebaDocument>();
        document.Instance.Content.ShouldNotBeNull();
        document.Instance.Content.Value.Value.ShouldContain("Rule 1");
        document.Instance.IsLoading.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldDisplayErrorWhenApiCallFails()
    {
        // Arrange
        using var response = ApiResponseFactory.CreateResponse("<p>Error</p>", System.Net.HttpStatusCode.InternalServerError);
        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentRules> cut = Render<TournamentRules>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        IRenderedComponent<NebaDocument> document = cut.FindComponent<NebaDocument>();
        document.Instance.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
        document.Instance.ErrorMessage.ShouldContain("Failed to load tournament rules");
        document.Instance.IsLoading.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldDisplayErrorWhenExceptionIsThrown()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ThrowsAsync(new InvalidOperationException("Network error"));

        // Act
        IRenderedComponent<TournamentRules> cut = Render<TournamentRules>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        IRenderedComponent<NebaDocument> document = cut.FindComponent<NebaDocument>();
        document.Instance.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
        document.Instance.ErrorMessage.ShouldContain("Failed to load tournament rules");
        document.Instance.ErrorMessage.ShouldContain("Network error");
        document.Instance.IsLoading.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldConfigureNebaDocumentWithCorrectParameters()
    {
        // Arrange
        using var response = ApiResponseFactory.CreateSuccessResponse("<h1>Rules</h1>");
        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentRules> cut = Render<TournamentRules>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        IRenderedComponent<NebaDocument> document = cut.FindComponent<NebaDocument>();
        document.Instance.LoadingText.ShouldBe("Loading tournament rules...");
        document.Instance.ErrorTitle.ShouldBe("Error Loading Tournament Rules");
        document.Instance.ShowTableOfContents.ShouldBeTrue();
        document.Instance.TableOfContentsTitle.ShouldBe("Contents");
        document.Instance.HeadingLevels.ShouldBe("h1, h2");
        document.Instance.DocumentId.ShouldBe("tournament-rules");
    }

    [Fact]
    public async Task ShouldNotSetContentWhenApiReturnsNotFoundError()
    {
        // Arrange
        using var response = ApiResponseFactory.CreateResponse("<p>Not found</p>", System.Net.HttpStatusCode.NotFound);
        _mockNebaApi
            .Setup(x => x.GetTournamentRulesAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<TournamentRules> cut = Render<TournamentRules>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        IRenderedComponent<NebaDocument> document = cut.FindComponent<NebaDocument>();
        document.Instance.Content.ShouldBeNull();
        document.Instance.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }
}
