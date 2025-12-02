using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Neba.Web.Server.Components;

namespace Neba.WebTests.Components;

public sealed class NebaLoadingIndicatorTests : TestContextWrapper
{
    private static void WaitForLoadingIndicator(IRenderedComponent<NebaLoadingIndicator> cut)
    {
        cut.WaitForState(() => cut.FindAll(".neba-loading-overlay").Count > 0, timeout: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ShouldNotRenderWhenIsVisibleIsFalse()
    {
        // Arrange & Act
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert
        cut.Markup.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldRenderWhenIsVisibleIsTrue()
    {
        // Arrange & Act
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0)); // No delay for testing

        // Assert - Wait for the component to render asynchronously
        WaitForLoadingIndicator(cut);
        IElement overlay = cut.Find(".neba-loading-overlay");
        overlay.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldRenderLoadingAnimation()
    {
        // Arrange & Act
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0)); // No delay for testing

        // Assert
        WaitForLoadingIndicator(cut);
        IElement waveElement = cut.Find(".neba-loading-wave");
        waveElement.ShouldNotBeNull();

        // Should have 5 wave bars
        IHtmlCollection<IElement> waveBars = waveElement.QuerySelectorAll("div");
        waveBars.Length.ShouldBe(5);
    }

    [Fact]
    public void ShouldNotRenderTextWhenNotProvided()
    {
        // Arrange & Act
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        cut.FindAll(".neba-loading-text").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldRenderTextWhenProvided()
    {
        // Arrange & Act
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0) // No delay for testing
            .Add(p => p.Text, "Loading data..."));

        // Assert
        WaitForLoadingIndicator(cut);
        IElement textElement = cut.Find(".neba-loading-text");
        textElement.TextContent.ShouldBe("Loading data...");
    }

    [Fact]
    public void ShouldNotRenderTextWhenEmptyString()
    {
        // Arrange & Act
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Text, string.Empty));

        // Assert
        cut.FindAll(".neba-loading-text").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldNotRenderTextWhenWhitespace()
    {
        // Arrange & Act
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Text, "   "));

        // Assert
        cut.FindAll(".neba-loading-text").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldUsePageScopeByDefault()
    {
        // Arrange & Act
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0)); // No delay for testing

        // Assert
        WaitForLoadingIndicator(cut);
        IElement overlay = cut.Find(".neba-loading-overlay");
        overlay.ClassList.ShouldContain("neba-loading-overlay-page");
        overlay.ClassList.ShouldNotContain("neba-loading-overlay-fullscreen");
    }

    [Fact]
    public void ShouldApplyPageScopeClass()
    {
        // Arrange & Act
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0) // No delay for testing
            .Add(p => p.Scope, LoadingIndicatorScope.Page));

        // Assert
        WaitForLoadingIndicator(cut);
        IElement overlay = cut.Find(".neba-loading-overlay");
        overlay.ClassList.ShouldContain("neba-loading-overlay-page");
        overlay.ClassList.ShouldNotContain("neba-loading-overlay-fullscreen");
    }

    [Fact]
    public void ShouldApplyFullScreenScopeClass()
    {
        // Arrange & Act
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0) // No delay for testing
            .Add(p => p.Scope, LoadingIndicatorScope.FullScreen));

        // Assert
        WaitForLoadingIndicator(cut);
        IElement overlay = cut.Find(".neba-loading-overlay");
        overlay.ClassList.ShouldContain("neba-loading-overlay-fullscreen");
        overlay.ClassList.ShouldNotContain("neba-loading-overlay-page");
    }

    [Fact]
    public void ShouldHandleOverlayClickWhenCallbackProvided()
    {
        // Arrange
        var clickedCount = 0;
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0) // No delay for testing
            .Add(p => p.OnOverlayClick, EventCallback.Factory.Create(this, () => clickedCount++)));

        // Act
        WaitForLoadingIndicator(cut);
        IElement overlay = cut.Find(".neba-loading-overlay");
        overlay.Click();

        // Assert
        clickedCount.ShouldBe(1);
    }

    [Fact]
    public void ShouldNotThrowWhenOverlayClickedWithoutCallback()
    {
        // Arrange
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0)); // No delay for testing

        // Act
        WaitForLoadingIndicator(cut);
        IElement overlay = cut.Find(".neba-loading-overlay");
        Action act = () => overlay.Click();

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void ShouldRenderContentWrapper()
    {
        // Arrange & Act
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0)); // No delay for testing

        // Assert
        WaitForLoadingIndicator(cut);
        IElement contentWrapper = cut.Find(".neba-loading-content");
        contentWrapper.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("Loading your data...")]
    [InlineData("Please wait")]
    [InlineData("Processing request")]
    public void ShouldRenderDifferentTextValues(string text)
    {
        // Arrange & Act
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0) // No delay for testing
            .Add(p => p.Text, text));

        // Assert
        WaitForLoadingIndicator(cut);
        IElement textElement = cut.Find(".neba-loading-text");
        textElement.TextContent.ShouldBe(text);
    }

    [Fact]
    public void ShouldHandleMultipleOverlayClicks()
    {
        // Arrange
        var clickedCount = 0;
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0) // No delay for testing
            .Add(p => p.OnOverlayClick, EventCallback.Factory.Create(this, () => clickedCount++)));

        WaitForLoadingIndicator(cut);
        IElement overlay = cut.Find(".neba-loading-overlay");

        // Act
        overlay.Click();
        overlay.Click();
        overlay.Click();

        // Assert
        clickedCount.ShouldBe(3);
    }
}
