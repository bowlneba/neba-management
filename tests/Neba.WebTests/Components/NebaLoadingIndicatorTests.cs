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
        int clickedCount = 0;
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
        int clickedCount = 0;
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

    [Fact]
    public void ShouldContinueDisplayingWhenAlreadyVisibleAndRemainsVisible()
    {
        // Arrange - Start with IsVisible=true
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0)); // No delay for testing

        // Wait for initial render
        WaitForLoadingIndicator(cut);
        IElement initialOverlay = cut.Find(".neba-loading-overlay");
        initialOverlay.ShouldNotBeNull();

        // Act - Update parameters with IsVisible still true (simulates indicator already showing)
        cut.Render(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0));

        // Assert - Indicator should still be visible, not hidden
        IElement overlayAfterUpdate = cut.Find(".neba-loading-overlay");
        overlayAfterUpdate.ShouldNotBeNull();
    }

    [Fact]
    public void HandleLoadingFinished_ShouldHideIndicator_WhenLoadingFinishesBeforeDelayExpires()
    {
        // Arrange - Start with IsVisible=true and a delay
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 500)); // 500ms delay

        // Act - Immediately set IsVisible to false before delay expires
        cut.Render(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert - Indicator should not be shown (delay was cancelled)
        cut.FindAll(".neba-loading-overlay").ShouldBeEmpty();
    }

    [Fact]
    public void HandleLoadingFinished_ShouldWaitMinimumDisplayTime_WhenLoadingFinishesQuicklyAfterShown()
    {
        // Arrange - Start with IsVisible=true, no delay, but with minimum display time
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0) // No delay
            .Add(p => p.MinimumDisplayMs, 500)); // 500ms minimum display

        // Wait for indicator to show
        WaitForLoadingIndicator(cut);
        cut.Find(".neba-loading-overlay").ShouldNotBeNull();

        // Act - Set IsVisible to false immediately after it appears
        cut.Render(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert - Indicator should still be visible (waiting for minimum display time)
        cut.Find(".neba-loading-overlay").ShouldNotBeNull();

        // Wait for minimum display time to elapse
        cut.WaitForState(() => cut.FindAll(".neba-loading-overlay").Count == 0, timeout: TimeSpan.FromSeconds(2));

        // Assert - Indicator should now be hidden
        cut.FindAll(".neba-loading-overlay").ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleLoadingFinished_ShouldHideImmediately_WhenMinimumDisplayTimeAlreadyElapsed()
    {
        // Arrange - Start with IsVisible=true, no delay, with minimum display time
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0) // No delay
            .Add(p => p.MinimumDisplayMs, 100)); // 100ms minimum display

        // Wait for indicator to show
        WaitForLoadingIndicator(cut);
        cut.Find(".neba-loading-overlay").ShouldNotBeNull();

        // Wait for minimum display time to elapse
        await Task.Delay(150); // Wait longer than MinimumDisplayMs

        // Act - Set IsVisible to false after minimum time has elapsed
        cut.Render(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert - Indicator should be hidden immediately (no waiting needed)
        await cut.WaitForStateAsync(() => cut.FindAll(".neba-loading-overlay").Count == 0, timeout: TimeSpan.FromSeconds(1));
        cut.FindAll(".neba-loading-overlay").ShouldBeEmpty();
    }

    [Fact]
    public void HandleLoadingFinished_ShouldCleanupState_WhenIndicatorNeverShown()
    {
        // Arrange - Start with IsVisible=false
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert - No indicator should be rendered
        cut.FindAll(".neba-loading-overlay").ShouldBeEmpty();

        // Act - Set IsVisible to false again (cleanup should handle gracefully)
        cut.Render(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert - Still no indicator
        cut.FindAll(".neba-loading-overlay").ShouldBeEmpty();
    }

    [Fact]
    public void HandleLoadingFinished_ShouldCancelPreviousMinimumDisplayTimer_WhenCalledMultipleTimes()
    {
        // Arrange - Start with IsVisible=true, no delay, with minimum display time
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0)
            .Add(p => p.MinimumDisplayMs, 1000)); // 1 second minimum display

        // Wait for indicator to show
        WaitForLoadingIndicator(cut);

        // Act - Toggle IsVisible false then true then false quickly
        cut.Render(parameters => parameters
            .Add(p => p.IsVisible, false));

        cut.Render(parameters => parameters
            .Add(p => p.IsVisible, true));

        cut.Render(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert - Should eventually hide without throwing exceptions
        cut.WaitForState(() => cut.FindAll(".neba-loading-overlay").Count == 0, timeout: TimeSpan.FromSeconds(3));
        cut.FindAll(".neba-loading-overlay").ShouldBeEmpty();
    }

    [Fact]
    public void HandleLoadingFinished_ShouldHandleZeroMinimumDisplayTime()
    {
        // Arrange - Start with IsVisible=true, no delay, zero minimum display time
        IRenderedComponent<NebaLoadingIndicator> cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.DelayMs, 0)
            .Add(p => p.MinimumDisplayMs, 0)); // Zero minimum display

        // Wait for indicator to show
        WaitForLoadingIndicator(cut);

        // Act - Set IsVisible to false
        cut.Render(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert - Should hide immediately
        cut.WaitForState(() => cut.FindAll(".neba-loading-overlay").Count == 0, timeout: TimeSpan.FromSeconds(1));
        cut.FindAll(".neba-loading-overlay").ShouldBeEmpty();
    }
}
