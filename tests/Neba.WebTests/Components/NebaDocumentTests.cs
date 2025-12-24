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
        MarkupString content = new MarkupString("<h1>Test Heading</h1><p>Test content</p>");

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
        MarkupString content = new MarkupString("<p>Content</p>");

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
        MarkupString content = new MarkupString("<h1>Heading</h1>");

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
        MarkupString content = new MarkupString("<h1>Heading</h1>");

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
        MarkupString content = new MarkupString("<h1>Heading</h1>");

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
        MarkupString content = new MarkupString("<h1>Heading</h1>");

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
        MarkupString content = new MarkupString("<p>Content</p>");

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
        MarkupString content = new MarkupString("<p>Content</p>");

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
        MarkupString content = new MarkupString("<p>Content</p>");

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
        bool dismissCalled = false;
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
        MarkupString content = new MarkupString("<p>Content</p>");

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
        MarkupString content = new MarkupString("<h1>Heading 1</h1><h2>Heading 2</h2>");

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
        MarkupString content = new MarkupString("<h1>H1</h1><h2>H2</h2><h3>H3</h3>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.HeadingLevels, "h1, h2, h3"));

        // Assert
        cut.Instance.HeadingLevels.ShouldBe("h1, h2, h3");
    }

    [Fact]
    public void ShouldRenderMobileTocButton()
    {
        // Arrange
        MarkupString content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.ShowTableOfContents, true));

        // Assert
        IElement tocButton = cut.Find(".neba-document-toc-mobile-btn");
        tocButton.ShouldNotBeNull();
        tocButton.GetAttribute("aria-label").ShouldBe("Open table of contents");
    }

    [Fact]
    public void ShouldNotRenderMobileTocButtonWhenShowTableOfContentsIsFalse()
    {
        // Arrange
        MarkupString content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.ShowTableOfContents, false));

        // Assert
        cut.FindAll(".neba-document-toc-mobile-btn").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldRenderMobileTocModal()
    {
        // Arrange
        MarkupString content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.ShowTableOfContents, true)
            .Add(p => p.DocumentId, "test-doc"));

        // Assert
        IElement tocModal = cut.Find("#neba-document-toc-modal-test-doc");
        tocModal.ShouldNotBeNull();
        tocModal.GetAttribute("role").ShouldBe("dialog");
        tocModal.GetAttribute("aria-modal").ShouldBe("true");
    }

    [Fact]
    public void ShouldRenderMobileTocModalWithCloseButton()
    {
        // Arrange
        MarkupString content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.ShowTableOfContents, true)
            .Add(p => p.DocumentId, "test-doc"));

        // Assert
        IElement closeButton = cut.Find("#neba-document-toc-modal-close-test-doc");
        closeButton.ShouldNotBeNull();
        closeButton.GetAttribute("aria-label").ShouldBe("Close table of contents");
    }

    [Fact]
    public void ShouldRenderSlideoverPanel()
    {
        // Arrange
        MarkupString content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.DocumentId, "test-doc"));

        // Assert
        IElement slideover = cut.Find("#neba-document-slideover-test-doc");
        slideover.ShouldNotBeNull();
        slideover.GetAttribute("role").ShouldBe("dialog");
        slideover.GetAttribute("aria-modal").ShouldBe("true");
    }

    [Fact]
    public void ShouldRenderSlideoverPanelWithCloseButton()
    {
        // Arrange
        MarkupString content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.DocumentId, "test-doc"));

        // Assert
        IElement closeButton = cut.Find("#neba-document-slideover-close-test-doc");
        closeButton.ShouldNotBeNull();
        closeButton.GetAttribute("aria-label").ShouldBe("Close document");
    }

    [Fact]
    public void ShouldRenderSlideoverPanelContentArea()
    {
        // Arrange
        MarkupString content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.DocumentId, "test-doc"));

        // Assert
        IElement slideoverContent = cut.Find("#neba-document-slideover-content-test-doc");
        slideoverContent.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldNotRenderLastUpdatedFooterWhenMetadataIsNull()
    {
        // Arrange
        MarkupString content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.Metadata, null));

        // Assert
        cut.FindAll(".neba-document-last-updated-footer").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldNotRenderLastUpdatedFooterWhenMetadataDoesNotContainLastUpdatedUtc()
    {
        // Arrange
        MarkupString content = new MarkupString("<p>Content</p>");
        var metadata = new Dictionary<string, string>
        {
            { "SomeOtherKey", "SomeValue" }
        };

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.Metadata, metadata));

        // Assert
        cut.FindAll(".neba-document-last-updated-footer").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldRenderLastUpdatedFooterWithDateOnly()
    {
        // Arrange
        MarkupString content = new MarkupString("<p>Content</p>");
        var metadata = new Dictionary<string, string>
        {
            { "LastUpdatedUtc", "2024-01-15T10:30:00Z" }
        };

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.Metadata, metadata));

        // Assert
        IElement footer = cut.Find(".neba-document-last-updated-footer");
        footer.ShouldNotBeNull();
        footer.TextContent.ShouldContain("2024-01-15");
    }

    [Fact]
    public void ShouldRenderLastUpdatedFooterWithDateAndUsername()
    {
        // Arrange
        MarkupString content = new MarkupString("<p>Content</p>");
        var metadata = new Dictionary<string, string>
        {
            { "LastUpdatedUtc", "2024-01-15T10:30:00Z" },
            { "LastUpdatedBy", "Admin User" }
        };

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.Metadata, metadata));

        // Assert
        IElement footer = cut.Find(".neba-document-last-updated-footer");
        footer.ShouldNotBeNull();
        footer.TextContent.ShouldContain("2024-01-15");
        footer.TextContent.ShouldContain("Admin User");
        footer.TextContent.ShouldContain("by");
    }

    [Fact]
    public void ShouldNotRenderLastUpdatedFooterWhenDateIsInvalid()
    {
        // Arrange
        MarkupString content = new MarkupString("<p>Content</p>");
        var metadata = new Dictionary<string, string>
        {
            { "LastUpdatedUtc", "invalid-date" },
            { "LastUpdatedBy", "Admin User" }
        };

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.Metadata, metadata));

        // Assert
        cut.FindAll(".neba-document-last-updated-footer").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldGenerateUniqueIdsForAllTocElements()
    {
        // Arrange
        MarkupString content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.DocumentId, "test-doc"));

        // Assert
        cut.Find("#neba-document-toc-test-doc").ShouldNotBeNull();
        cut.Find("#neba-document-toc-list-test-doc").ShouldNotBeNull();
        cut.Find("#neba-document-toc-mobile-btn-test-doc").ShouldNotBeNull();
        cut.Find("#neba-document-toc-modal-test-doc").ShouldNotBeNull();
        cut.Find("#neba-document-toc-modal-overlay-test-doc").ShouldNotBeNull();
        cut.Find("#neba-document-toc-modal-close-test-doc").ShouldNotBeNull();
        cut.Find("#neba-document-toc-modal-title-test-doc").ShouldNotBeNull();
        cut.Find("#neba-document-toc-mobile-list-test-doc").ShouldNotBeNull();
    }

    [Fact]
    public void ShouldGenerateUniqueIdsForSlideoverElements()
    {
        // Arrange
        MarkupString content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.DocumentId, "test-doc"));

        // Assert
        cut.Find("#neba-document-slideover-test-doc").ShouldNotBeNull();
        cut.Find("#neba-document-slideover-overlay-test-doc").ShouldNotBeNull();
        cut.Find("#neba-document-slideover-close-test-doc").ShouldNotBeNull();
        cut.Find("#neba-document-slideover-title-test-doc").ShouldNotBeNull();
        cut.Find("#neba-document-slideover-content-test-doc").ShouldNotBeNull();
    }

    [Fact]
    public void ShouldRenderMobileTocModalWithTitleMatchingTableOfContentsTitle()
    {
        // Arrange
        MarkupString content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.TableOfContentsTitle, "Custom Mobile Title")
            .Add(p => p.DocumentId, "test-doc"));

        // Assert
        IElement modalTitle = cut.Find("#neba-document-toc-modal-title-test-doc");
        modalTitle.TextContent.ShouldBe("Custom Mobile Title");
    }

    [Fact]
    public void ShouldRenderSlideoverWithLoadingTitle()
    {
        // Arrange
        MarkupString content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.DocumentId, "test-doc"));

        // Assert
        IElement slideoverTitle = cut.Find("#neba-document-slideover-title-test-doc");
        slideoverTitle.TextContent.ShouldBe("Loading...");
    }

    [Fact]
    public void ShouldIncludeModuleScriptTag()
    {
        // Arrange
        MarkupString content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content));

        // Assert
        IElement scriptTag = cut.Find("script[src='./Components/NebaDocument.razor.js']");
        scriptTag.ShouldNotBeNull();
        scriptTag.GetAttribute("type").ShouldBe("module");
    }

    [Fact]
    public void ShouldIncludeStylesheetLink()
    {
        // Arrange
        MarkupString content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content));

        // Assert
        IElement linkTag = cut.Find("link[href='/neba-document.css']");
        linkTag.ShouldNotBeNull();
        linkTag.GetAttribute("rel").ShouldBe("stylesheet");
    }

    [Fact]
    public void ShouldRenderTocStickyContainer()
    {
        // Arrange
        MarkupString content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.ShowTableOfContents, true));

        // Assert
        IElement stickyContainer = cut.Find(".toc-sticky");
        stickyContainer.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldRenderMobileTocButtonWithIcon()
    {
        // Arrange
        MarkupString content = new("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.ShowTableOfContents, true));

        // Assert
        IElement tocButton = cut.Find(".neba-document-toc-mobile-btn");
        IElement? svg = tocButton.QuerySelector("svg");
        svg.ShouldNotBeNull();
        svg.GetAttribute("aria-hidden").ShouldBe("true");

        IElement? span = tocButton.QuerySelector("span");
        span.ShouldNotBeNull();
        span.TextContent.ShouldBe("Contents");
    }

    [Fact]
    public void ShouldRenderSlideoverWithOverlayAndPanel()
    {
        // Arrange
        MarkupString content = new("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.DocumentId, "test-doc"));

        // Assert
        IElement overlay = cut.Find(".neba-document-slideover-overlay");
        overlay.ShouldNotBeNull();

        IElement panel = cut.Find(".neba-document-slideover-panel");
        panel.ShouldNotBeNull();

        IElement header = cut.Find(".neba-document-slideover-header");
        header.ShouldNotBeNull();

        IElement slideoverContentElement = cut.Find(".neba-document-slideover-content");
        slideoverContentElement.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldRenderMobileTocModalWithOverlayAndContent()
    {
        // Arrange
        MarkupString content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.ShowTableOfContents, true)
            .Add(p => p.DocumentId, "test-doc"));

        // Assert
        IElement overlay = cut.Find(".neba-document-toc-modal-overlay");
        overlay.ShouldNotBeNull();

        IElement modalContent = cut.Find(".neba-document-toc-modal-content");
        modalContent.ShouldNotBeNull();

        IElement header = cut.Find(".neba-document-toc-modal-header");
        header.ShouldNotBeNull();

        IElement body = cut.Find(".neba-document-toc-modal-body");
        body.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldRenderMultipleInstancesWithDifferentIds()
    {
        // Arrange
        MarkupString content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut1 = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.DocumentId, "doc-1"));

        IRenderedComponent<NebaDocument> cut2 = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.DocumentId, "doc-2"));

        // Assert
        cut1.Find("#neba-document-doc-1").ShouldNotBeNull();
        cut1.Find("#neba-document-content-doc-1").ShouldNotBeNull();
        cut1.Find("#neba-document-toc-doc-1").ShouldNotBeNull();

        cut2.Find("#neba-document-doc-2").ShouldNotBeNull();
        cut2.Find("#neba-document-content-doc-2").ShouldNotBeNull();
        cut2.Find("#neba-document-toc-doc-2").ShouldNotBeNull();
    }

    [Fact]
    public void ShouldHaveAccessibleTocNavigation()
    {
        // Arrange
        MarkupString content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.ShowTableOfContents, true));

        // Assert
        IElement nav = cut.Find("nav.neba-document-toc");
        nav.GetAttribute("aria-label").ShouldBe("Table of Contents");
    }

    [Fact]
    public void ShouldRenderCloseButtonsWithAccessibleLabels()
    {
        // Arrange
        MarkupString content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.ShowTableOfContents, true)
            .Add(p => p.DocumentId, "test-doc"));

        // Assert
        IElement tocCloseButton = cut.Find("#neba-document-toc-modal-close-test-doc");
        tocCloseButton.GetAttribute("aria-label").ShouldBe("Close table of contents");

        IElement slideoverCloseButton = cut.Find("#neba-document-slideover-close-test-doc");
        slideoverCloseButton.GetAttribute("aria-label").ShouldBe("Close document");
    }

    [Fact]
    public void ShouldRenderIconsWithAriaHiddenTrue()
    {
        // Arrange
        MarkupString content = new MarkupString("<h1>Heading</h1>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.ShowTableOfContents, true));

        // Assert
        IEnumerable<IElement> svgElements = cut.FindAll("svg");
        svgElements.ShouldNotBeEmpty();

        foreach (IElement svg in svgElements)
        {
            svg.GetAttribute("aria-hidden").ShouldBe("true");
        }
    }

    [Fact]
    public void ShouldNotRenderTocOrSlideoverWhenContentIsNull()
    {
        // Arrange & Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, null)
            .Add(p => p.IsLoading, false));

        // Assert
        cut.FindAll(".neba-document-container").ShouldBeEmpty();
        cut.FindAll(".neba-document-toc").ShouldBeEmpty();
        cut.FindAll(".neba-document-slideover").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldNotRenderTocOrSlideoverWhenIsLoadingIsTrue()
    {
        // Arrange
        MarkupString content = new MarkupString("<p>Content</p>");

        // Act
        IRenderedComponent<NebaDocument> cut = Render<NebaDocument>(parameters => parameters
            .Add(p => p.Content, content)
            .Add(p => p.IsLoading, true));

        // Assert
        cut.FindAll(".neba-document-container").ShouldBeEmpty();
        cut.FindAll(".neba-document-toc").ShouldBeEmpty();
        cut.FindAll(".neba-document-slideover").ShouldBeEmpty();
    }
}
