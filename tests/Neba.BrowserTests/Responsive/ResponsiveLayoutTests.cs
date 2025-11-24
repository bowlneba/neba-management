using Microsoft.Playwright;
using Neba.BrowserTests.TestUtils;

namespace Neba.BrowserTests.Responsive;

/// <summary>
/// Tests for responsive layout behavior across different viewport sizes.
/// Validates that the application layout adapts correctly to mobile, tablet, and desktop viewports.
/// </summary>
public class ResponsiveLayoutTests : PlaywrightTestBase
{
    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task NavbarCollapses_AtTabletBreakpoint(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync(page);

        // Act - Start with desktop viewport, navbar should be expanded
        var hamburgerMenuDesktop = page.Locator("[data-action='toggle-menu']");
        var isVisibleDesktop = await hamburgerMenuDesktop.IsVisibleAsync();

        // Assert - Hamburger menu should NOT be visible on desktop
        isVisibleDesktop.ShouldBeFalse("Hamburger menu should not be visible on desktop viewport");

        // Act - Resize to tablet breakpoint (1100px)
        await page.SetViewportSizeAsync(ViewportHelpers.TabletLandscape.Width, ViewportHelpers.TabletLandscape.Height);
        await Task.Delay(300); // Allow transition time

        // Assert - Hamburger menu should be visible at tablet breakpoint
        var isVisibleTablet = await hamburgerMenuDesktop.IsVisibleAsync();
        isVisibleTablet.ShouldBeTrue("Hamburger menu should be visible at tablet breakpoint (1100px)");

        // Act - Resize to just above breakpoint (1101px)
        await page.SetViewportSizeAsync(ViewportHelpers.DesktopTight.Width, ViewportHelpers.DesktopTight.Height);
        await Task.Delay(300); // Allow transition time

        // Assert - Hamburger menu should NOT be visible just above breakpoint
        var isVisibleAboveBreakpoint = await hamburgerMenuDesktop.IsVisibleAsync();
        isVisibleAboveBreakpoint.ShouldBeFalse("Hamburger menu should not be visible just above breakpoint (1101px)");
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task MobileMenuToggle_OpensAndCloses_Correctly(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.Mobile });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync(page);

        var hamburgerMenu = page.Locator("[data-action='toggle-menu']");
        var navMenu = page.Locator("nav.mobile-menu, nav[role='navigation']").First;

        // Act - Click hamburger to open menu
        await hamburgerMenu.ClickAsync();
        await Task.Delay(300); // Allow animation

        // Assert - Menu should be visible and aria-expanded should be true
        var isOpen = await navMenu.IsVisibleAsync();
        var ariaExpanded = await hamburgerMenu.GetAttributeAsync("aria-expanded");
        isOpen.ShouldBeTrue("Navigation menu should be visible after clicking hamburger");
        ariaExpanded.ShouldBe("true");

        // Act - Click hamburger again to close menu
        await hamburgerMenu.ClickAsync();
        await Task.Delay(300); // Allow animation

        // Assert - Menu should be hidden and aria-expanded should be false
        var ariaExpandedClosed = await hamburgerMenu.GetAttributeAsync("aria-expanded");
        ariaExpandedClosed.ShouldBe("false");
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task NoHorizontalScroll_AtAllViewportSizes(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);

        foreach (var viewport in ViewportHelpers.AllViewports)
        {
            await using var context = await browser.NewContextAsync(new() { ViewportSize = viewport });
            var page = await context.NewPageAsync();

            // Act
            await page.GotoAsync(BaseUrl);
            await WaitForBlazorAsync(page);

            // Assert - No horizontal scroll at this viewport
            var hasHorizontalScroll = await page.EvaluateAsync<bool>(@"
                () => document.documentElement.scrollWidth > document.documentElement.clientWidth
            ");

            hasHorizontalScroll.ShouldBeFalse(
                $"Page should not have horizontal scroll at viewport {viewport.Width}x{viewport.Height}");

            await context.CloseAsync();
        }
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task SearchBar_TransitionsWidth_OnFocus(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync(page);

        var searchInput = page.Locator("input[type='search'], input[placeholder*='Search']").First;

        // Act - Get initial width
        var initialBox = await searchInput.BoundingBoxAsync();
        initialBox.ShouldNotBeNull();
        var initialWidth = initialBox.Width;

        // Act - Focus search bar
        await searchInput.FocusAsync();
        await Task.Delay(300); // Allow transition

        // Assert - Width should increase on focus
        var focusedBox = await searchInput.BoundingBoxAsync();
        focusedBox.ShouldNotBeNull();
        var focusedWidth = focusedBox.Width;

        focusedWidth.ShouldBeGreaterThan(initialWidth,
            $"Search bar width should increase on focus (initial: {initialWidth}px, focused: {focusedWidth}px)");

        // Act - Blur search bar
        await searchInput.BlurAsync();
        await Task.Delay(300); // Allow transition

        // Assert - Width should return to original
        var blurredBox = await searchInput.BoundingBoxAsync();
        blurredBox.ShouldNotBeNull();
        var blurredWidth = blurredBox.Width;

        Math.Abs(blurredWidth - initialWidth).ShouldBeLessThan(5,
            $"Search bar width should return to initial after blur (initial: {initialWidth}px, blurred: {blurredWidth}px)");
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task Footer_AdjustsLayout_ForMobileAndDesktop(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);

        // Test mobile layout
        await using var mobileContext = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.Mobile });
        var mobilePage = await mobileContext.NewPageAsync();
        await mobilePage.GotoAsync(BaseUrl);
        await WaitForBlazorAsync(mobilePage);

        var mobileFooter = mobilePage.Locator("footer").First;
        var mobileFooterBox = await mobileFooter.BoundingBoxAsync();
        mobileFooterBox.ShouldNotBeNull();

        // Test desktop layout
        await using var desktopContext = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var desktopPage = await desktopContext.NewPageAsync();
        await desktopPage.GotoAsync(BaseUrl);
        await WaitForBlazorAsync(desktopPage);

        var desktopFooter = desktopPage.Locator("footer").First;
        var desktopFooterBox = await desktopFooter.BoundingBoxAsync();
        desktopFooterBox.ShouldNotBeNull();

        // Assert - Footer should be visible on both viewports
        (await mobileFooter.IsVisibleAsync()).ShouldBeTrue("Footer should be visible on mobile");
        (await desktopFooter.IsVisibleAsync()).ShouldBeTrue("Footer should be visible on desktop");

        // Assert - Footer should contain expected content
        var footerText = await desktopFooter.TextContentAsync();
        footerText.ShouldNotBeNull();
        footerText.ShouldContain("New England Bowling Association", Case.Insensitive);
    }

    // Helper method to create browser for cross-browser testing
    private async Task<IBrowser> CreateBrowserAsync(string browserType)
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        return browserType.ToLowerInvariant() switch
        {
            "chromium" => await playwright.Chromium.LaunchAsync(new() { Headless = Headless }),
            "firefox" => await playwright.Firefox.LaunchAsync(new() { Headless = Headless }),
            "webkit" => await playwright.Webkit.LaunchAsync(new() { Headless = Headless }),
            _ => throw new ArgumentException($"Unknown browser type: {browserType}")
        };
    }

    // Helper to wait for Blazor on any page
    private static async Task WaitForBlazorAsync(IPage page)
    {
        await page.WaitForFunctionAsync(@"
            () => window.Blazor !== undefined && window.Blazor._internal !== undefined
        ");
    }
}
