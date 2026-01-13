using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neba.Contracts;
using Neba.Tests;
using Neba.Web.Server.Components;
using Neba.Web.Server.Pages;
using Neba.Web.Server.Services;

namespace Neba.WebTests.Pages;

[Trait("Category", "Web")]
[Trait("Component", "Pages")]

public sealed class BylawsTests : TestContextWrapper
{
    private readonly Mock<INebaWebsiteApi> _mockNebaApi;
    private readonly NebaWebsiteApiService _NebaWebsiteApiService;

    public BylawsTests()
    {
        _mockNebaApi = new Mock<INebaWebsiteApi>();
        _NebaWebsiteApiService = new NebaWebsiteApiService(_mockNebaApi.Object);
        TestContext.Services.AddSingleton(_NebaWebsiteApiService);
    }

    [Fact]
    public async Task ShouldRenderPageTitle()
    {
        // Arrange
        using TestApiResponse<DocumentResponse<string>> response = ApiResponseFactory.CreateDocumentResponse("<p>Bylaws content</p>");
        _mockNebaApi
            .Setup(x => x.GetBylawsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Bylaws> cut = Render<Bylaws>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        cut.Markup.ShouldContain("<h1");
        cut.Markup.ShouldContain("NEBA Bylaws");
    }

    [Fact]
    public async Task ShouldRenderPageDescription()
    {
        // Arrange
        using TestApiResponse<DocumentResponse<string>> response = ApiResponseFactory.CreateDocumentResponse("<p>Bylaws content</p>");
        _mockNebaApi
            .Setup(x => x.GetBylawsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Bylaws> cut = Render<Bylaws>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        cut.Markup.ShouldContain("Official bylaws and organizational guidelines");
    }

    [Fact]
    public async Task ShouldCallGetBylawsAsyncOnInitialization()
    {
        // Arrange
        using TestApiResponse<DocumentResponse<string>> response = ApiResponseFactory.CreateDocumentResponse("<h1>NEBA Bylaws</h1>");
        _mockNebaApi
            .Setup(x => x.GetBylawsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        Render<Bylaws>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        _mockNebaApi.Verify(x => x.GetBylawsAsync(), Times.Once);
    }

    [Fact]
    public async Task ShouldDisplayBylawsContentWhenApiCallSucceeds()
    {
        // Arrange
        const string bylawsHtml = "<h1>Article 1</h1><p>Description of article 1</p>";
        using TestApiResponse<DocumentResponse<string>> response = ApiResponseFactory.CreateDocumentResponse(bylawsHtml);
        _mockNebaApi
            .Setup(x => x.GetBylawsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Bylaws> cut = Render<Bylaws>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        IRenderedComponent<NebaDocument> document = cut.FindComponent<NebaDocument>();
        document.Instance.Content.ShouldNotBeNull();
        document.Instance.Content.Value.Value.ShouldContain("Article 1");
        document.Instance.IsLoading.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldDisplayErrorWhenApiCallFails()
    {
        // Arrange
        using TestApiResponse<DocumentResponse<string>> response = ApiResponseFactory.CreateDocumentResponse("<p>Error</p>", System.Net.HttpStatusCode.InternalServerError);
        _mockNebaApi
            .Setup(x => x.GetBylawsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Bylaws> cut = Render<Bylaws>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        IRenderedComponent<NebaDocument> document = cut.FindComponent<NebaDocument>();
        document.Instance.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
        document.Instance.ErrorMessage.ShouldContain("Failed to load bylaws");
        document.Instance.IsLoading.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldDisplayErrorWhenExceptionIsThrown()
    {
        // Arrange
        _mockNebaApi
            .Setup(x => x.GetBylawsAsync())
            .ThrowsAsync(new InvalidOperationException("Network error"));

        // Act
        IRenderedComponent<Bylaws> cut = Render<Bylaws>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        IRenderedComponent<NebaDocument> document = cut.FindComponent<NebaDocument>();
        document.Instance.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
        document.Instance.ErrorMessage.ShouldContain("Failed to load bylaws");
        document.Instance.ErrorMessage.ShouldContain("Network error");
        document.Instance.IsLoading.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldConfigureNebaDocumentWithCorrectParameters()
    {
        // Arrange
        using TestApiResponse<DocumentResponse<string>> response = ApiResponseFactory.CreateDocumentResponse("<h1>Bylaws</h1>");
        _mockNebaApi
            .Setup(x => x.GetBylawsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Bylaws> cut = Render<Bylaws>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        IRenderedComponent<NebaDocument> document = cut.FindComponent<NebaDocument>();
        document.Instance.LoadingText.ShouldBe("Loading bylaws...");
        document.Instance.ErrorTitle.ShouldBe("Error Loading Bylaws");
        document.Instance.ShowTableOfContents.ShouldBeTrue();
        document.Instance.TableOfContentsTitle.ShouldBe("Contents");
        document.Instance.HeadingLevels.ShouldBe("h1, h2");
        document.Instance.DocumentId.ShouldBe("bylaws");
    }

    [Fact]
    public async Task ShouldNotSetContentWhenApiReturnsNotFoundError()
    {
        // Arrange
        using TestApiResponse<DocumentResponse<string>> response = ApiResponseFactory.CreateDocumentResponse("<p>Not found</p>", System.Net.HttpStatusCode.NotFound);
        _mockNebaApi
            .Setup(x => x.GetBylawsAsync())
            .ReturnsAsync(response.ApiResponse);

        // Act
        IRenderedComponent<Bylaws> cut = Render<Bylaws>();
        await Task.Delay(50); // Wait for async initialization

        // Assert
        IRenderedComponent<NebaDocument> document = cut.FindComponent<NebaDocument>();
        document.Instance.Content.ShouldBeNull();
        document.Instance.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }
}
