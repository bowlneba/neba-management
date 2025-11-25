using Bunit;
using Microsoft.AspNetCore.Components;
using Neba.Web.Server.Components;
using Shouldly;

namespace Neba.WebTests.Components;

public sealed class NebaLoadingIndicatorTests : TestContextWrapper
{
    [Fact]
    public void ShouldNotRenderWhenIsVisibleIsFalse()
    {
        // Arrange & Act
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert
        cut.Markup.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldRenderWhenIsVisibleIsTrue()
    {
        // Arrange & Act
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        var overlay = cut.Find(".neba-loading-overlay");
        overlay.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldRenderLoadingAnimation()
    {
        // Arrange & Act
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        var waveElement = cut.Find(".neba-loading-wave");
        waveElement.ShouldNotBeNull();

        // Should have 5 wave bars
        var waveBars = waveElement.QuerySelectorAll("div");
        waveBars.Length.ShouldBe(5);
    }

    [Fact]
    public void ShouldNotRenderTextWhenNotProvided()
    {
        // Arrange & Act
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        cut.FindAll(".neba-loading-text").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldRenderTextWhenProvided()
    {
        // Arrange & Act
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Text, "Loading data..."));

        // Assert
        var textElement = cut.Find(".neba-loading-text");
        textElement.TextContent.ShouldBe("Loading data...");
    }

    [Fact]
    public void ShouldNotRenderTextWhenEmptyString()
    {
        // Arrange & Act
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Text, string.Empty));

        // Assert
        cut.FindAll(".neba-loading-text").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldNotRenderTextWhenWhitespace()
    {
        // Arrange & Act
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Text, "   "));

        // Assert
        cut.FindAll(".neba-loading-text").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldUsePageScopeByDefault()
    {
        // Arrange & Act
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        var overlay = cut.Find(".neba-loading-overlay");
        overlay.ClassList.ShouldContain("neba-loading-overlay-page");
        overlay.ClassList.ShouldNotContain("neba-loading-overlay-fullscreen");
    }

    [Fact]
    public void ShouldApplyPageScopeClass()
    {
        // Arrange & Act
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Scope, LoadingIndicatorScope.Page));

        // Assert
        var overlay = cut.Find(".neba-loading-overlay");
        overlay.ClassList.ShouldContain("neba-loading-overlay-page");
        overlay.ClassList.ShouldNotContain("neba-loading-overlay-fullscreen");
    }

    [Fact]
    public void ShouldApplyFullScreenScopeClass()
    {
        // Arrange & Act
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Scope, LoadingIndicatorScope.FullScreen));

        // Assert
        var overlay = cut.Find(".neba-loading-overlay");
        overlay.ClassList.ShouldContain("neba-loading-overlay-fullscreen");
        overlay.ClassList.ShouldNotContain("neba-loading-overlay-page");
    }

    [Fact]
    public void ShouldHandleOverlayClickWhenCallbackProvided()
    {
        // Arrange
        var clickedCount = 0;
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.OnOverlayClick, EventCallback.Factory.Create(this, () => clickedCount++)));

        // Act
        var overlay = cut.Find(".neba-loading-overlay");
        overlay.Click();

        // Assert
        clickedCount.ShouldBe(1);
    }

    [Fact]
    public void ShouldNotThrowWhenOverlayClickedWithoutCallback()
    {
        // Arrange
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Act
        var overlay = cut.Find(".neba-loading-overlay");
        var act = () => overlay.Click();

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void ShouldRenderContentWrapper()
    {
        // Arrange & Act
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        var contentWrapper = cut.Find(".neba-loading-content");
        contentWrapper.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("Loading your data...")]
    [InlineData("Please wait")]
    [InlineData("Processing request")]
    public void ShouldRenderDifferentTextValues(string text)
    {
        // Arrange & Act
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Text, text));

        // Assert
        var textElement = cut.Find(".neba-loading-text");
        textElement.TextContent.ShouldBe(text);
    }

    [Fact]
    public void ShouldHandleMultipleOverlayClicks()
    {
        // Arrange
        var clickedCount = 0;
        var cut = Render<NebaLoadingIndicator>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.OnOverlayClick, EventCallback.Factory.Create(this, () => clickedCount++)));

        var overlay = cut.Find(".neba-loading-overlay");

        // Act
        overlay.Click();
        overlay.Click();
        overlay.Click();

        // Assert
        clickedCount.ShouldBe(3);
    }
}
