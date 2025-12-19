using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Neba.Web.Server.Components;
using Neba.Web.Server.Notifications;

namespace Neba.WebTests.Components;

public sealed class NebaDocumentTests : TestContextWrapper
{
    [Fact]
    public void ShouldRenderLoadingIndicatorWhenIsLoadingIsTrue()
    {
        // Arrange & Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .Add(p => p.Content, null));

        // Assert
        IRenderedComponent<NebaLoadingIndicator> loadingIndicator = cut.FindComponent<NebaLoadingIndicator>();
        loadingIndicator.ShouldNotBeNull();
        loadingIndicator.Instance.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void ShouldRenderCustomLoadingText()
    {
        // Arrange & Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .Add(p => p.LoadingText, "Custom loading text...")
            .Add(p => p.Content, null));

        // Assert
        IRenderedComponent<NebaLoadingIndicator> loadingIndicator = cut.FindComponent<NebaLoadingIndicator>();
        loadingIndicator.Instance.Text.ShouldBe("Custom loading text...");
    }

    [Fact]
    public void ShouldUseDefaultLoadingText()
    {
        // Arrange & Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .Add(p => p.Content, null));

        // Assert
        IRenderedComponent<NebaLoadingIndicator> loadingIndicator = cut.FindComponent<NebaLoadingIndicator>();
        loadingIndicator.Instance.Text.ShouldBe("Loading document...");
    }

    [Fact]
    public void ShouldRenderErrorAlertWhenErrorMessageIsProvided()
    {
        // Arrange & Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.ErrorMessage, "Failed to load document")
            .Add(p => p.Content, null));

        // Assert
        IRenderedComponent<NebaAlert> alert = cut.FindComponent<NebaAlert>();
        alert.ShouldNotBeNull();
        alert.Instance.Severity.ShouldBe(NotifySeverity.Error);
        alert.Instance.Message.ShouldBe("Failed to load document");
    }

    [Fact]
    public void ShouldRenderCustomErrorTitle()
    {
        // Arrange & Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.ErrorMessage, "Something went wrong")
            .Add(p => p.ErrorTitle, "Custom Error Title")
            .Add(p => p.Content, null));

        // Assert
        IRenderedComponent<NebaAlert> alert = cut.FindComponent<NebaAlert>();
        alert.Instance.Title.ShouldBe("Custom Error Title");
    }

    [Fact]
    public void ShouldUseDefaultErrorTitle()
    {
        // Arrange & Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.ErrorMessage, "Something went wrong")
            .Add(p => p.Content, null));

        // Assert
        IRenderedComponent<NebaAlert> alert = cut.FindComponent<NebaAlert>();
        alert.Instance.Title.ShouldBe("Error Loading Document");
    }

    [Fact]
    public void ShouldMakeErrorAlertDismissible()
    {
        // Arrange & Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.ErrorMessage, "Error message")
            .Add(p => p.Content, null));

        // Assert
        IRenderedComponent<NebaAlert> alert = cut.FindComponent<NebaAlert>();
        alert.Instance.Dismissible.ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotRenderErrorAlertWhenErrorMessageIsNull()
    {
        // Arrange & Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.ErrorMessage, null)
            .Add(p => p.Content, new MarkupString("<p>Content</p>")));

        // Assert
        cut.FindComponents<NebaAlert>().ShouldBeEmpty();
    }

    [Fact]
    public void ShouldNotRenderErrorAlertWhenErrorMessageIsWhitespace()
    {
        // Arrange & Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.ErrorMessage, "   ")
            .Add(p => p.Content, new MarkupString("<p>Content</p>")));

        // Assert
        cut.FindComponents<NebaAlert>().ShouldBeEmpty();
    }

    [Fact]
    public void ShouldRenderContentWhenNotLoadingAndContentHasValue()
    {
        // Arrange
        var content = new MarkupString("<h1>Test Heading</h1><p>Test content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.Content, content));

        // Assert
        IElement contentElement = cut.Find(".neba-document-content");
        contentElement.InnerHtml.ShouldContain("Test Heading");
        contentElement.InnerHtml.ShouldContain("Test content");
    }

    [Fact]
    public void ShouldNotRenderContentWhenIsLoadingIsTrue()
    {
        // Arrange
        var content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .Add(p => p.Content, content));

        // Assert
        cut.FindAll(".neba-document-content").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldNotRenderContentWhenContentIsNull()
    {
        // Arrange & Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.Content, null));

        // Assert
        cut.FindAll(".neba-document-content").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldRenderTableOfContentsByDefault()
    {
        // Arrange
        var content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.Content, content));

        // Assert
        IElement tocElement = cut.Find(".neba-document-toc");
        tocElement.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldNotRenderTableOfContentsWhenShowTableOfContentsIsFalse()
    {
        // Arrange
        var content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.Content, content)
            .Add(p => p.ShowTableOfContents, false));

        // Assert
        cut.FindAll(".neba-document-toc").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldRenderCustomTableOfContentsTitle()
    {
        // Arrange
        var content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.Content, content)
            .Add(p => p.TableOfContentsTitle, "Custom TOC Title"));

        // Assert
        IElement tocTitle = cut.Find(".toc-title");
        tocTitle.TextContent.ShouldBe("Custom TOC Title");
    }

    [Fact]
    public void ShouldUseDefaultTableOfContentsTitle()
    {
        // Arrange
        var content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.Content, content));

        // Assert
        IElement tocTitle = cut.Find(".toc-title");
        tocTitle.TextContent.ShouldBe("Contents");
    }

    [Fact]
    public void ShouldGenerateUniqueContainerIdByDefault()
    {
        // Arrange
        var content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut1 = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content));
        IRenderedComponent<NebaDocument> cut2 = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content));

        // Assert
        IElement container1 = cut1.Find(".neba-document-container");
        IElement container2 = cut2.Find(".neba-document-container");

        container1.Id.ShouldNotBeNullOrWhiteSpace();
        container2.Id.ShouldNotBeNullOrWhiteSpace();
        container1.Id.ShouldNotBe(container2.Id);
    }

    [Fact]
    public void ShouldUseCustomDocumentId()
    {
        // Arrange
        var content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.DocumentId, "custom-doc-id"));

        // Assert
        IElement container = cut.Find(".neba-document-container");
        container.Id.ShouldBe("neba-document-custom-doc-id");
    }

    [Fact]
    public void ShouldRenderDocumentContainerStructure()
    {
        // Arrange
        var content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.DocumentId, "test-doc"));

        // Assert
        IElement container = cut.Find("#neba-document-test-doc");
        container.ShouldNotBeNull();

        IElement tocNav = cut.Find("#neba-document-toc-test-doc");
        tocNav.ShouldNotBeNull();
        tocNav.GetAttribute("aria-label").ShouldBe("Table of Contents");

        IElement contentDiv = cut.Find("#neba-document-content-test-doc");
        contentDiv.ShouldNotBeNull();
        contentDiv.ClassList.ShouldContain("neba-panel");
        contentDiv.ClassList.ShouldContain("neba-document-content");

        IElement tocList = cut.Find("#neba-document-toc-list-test-doc");
        tocList.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldInvokeOnErrorDismissCallback()
    {
        // Arrange
        var dismissCalled = false;
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.ErrorMessage, "Error message")
            .Add(p => p.OnErrorDismiss, EventCallback.Factory.Create(this, () => dismissCalled = true))
            .Add(p => p.Content, null));

        // Act
        IRenderedComponent<NebaAlert> alert = cut.FindComponent<NebaAlert>();
        IElement closeButton = alert.Find("button.neba-alert-close");
        closeButton.Click();

        // Assert
        dismissCalled.ShouldBeTrue();
    }

    [Fact]
    public void ShouldClearErrorMessageWhenDismissed()
    {
        // Arrange
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.ErrorMessage, "Error message")
            .Add(p => p.Content, null));

        // Act
        IRenderedComponent<NebaAlert> alert = cut.FindComponent<NebaAlert>();
        IElement closeButton = alert.Find("button.neba-alert-close");
        closeButton.Click();

        // Assert
        cut.FindComponents<NebaAlert>().ShouldBeEmpty();
    }

    [Fact]
    public void ShouldApplyPanelClassToContent()
    {
        // Arrange
        var content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content));

        // Assert
        IElement contentDiv = cut.Find(".neba-document-content");
        contentDiv.ClassList.ShouldContain("neba-panel");
    }

    [Fact]
    public void ShouldSetCorrectHeadingLevelsDefault()
    {
        // Arrange
        var content = new MarkupString("<h1>Heading 1</h1><h2>Heading 2</h2>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content));

        // Assert
        cut.Instance.HeadingLevels.ShouldBe("h1, h2");
    }

    [Fact]
    public void ShouldAcceptCustomHeadingLevels()
    {
        // Arrange
        var content = new MarkupString("<h1>H1</h1><h2>H2</h2><h3>H3</h3>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.HeadingLevels, "h1, h2, h3"));

        // Assert
        cut.Instance.HeadingLevels.ShouldBe("h1, h2, h3");
    }
}
