using Microsoft.Playwright;
using Neba.BrowserTests.TestUtils;


namespace Neba.BrowserTests.Navigation;

/// <summary>
/// Tests for navigation functionality including menus, links, and keyboard navigation.
/// </summary>
public class NavigationTests : PlaywrightTestBase
{
    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task HomePage_LoadsSuccessfully(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();

        // Act
        await page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync(page);

        // Assert - Page should load and contain expected elements
        var title = await page.TitleAsync();
        title.ShouldNotBeNull();

        // Assert - Main navigation elements should be present
        var logo = page.Locator("a[href='/'], img[alt*='NEBA']").First;
        await logo.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        (await logo.IsVisibleAsync()).ShouldBeTrue("NEBA logo should be visible");

        var nav = page.Locator("nav").First;
        (await nav.IsVisibleAsync()).ShouldBeTrue("Navigation bar should be visible");

        var footer = page.Locator("footer").First;
        (await footer.IsVisibleAsync()).ShouldBeTrue("Footer should be visible");
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task DropdownMenus_ExpandAndCollapse_OnClick(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync(page);

        // Find dropdown buttons (History, Testing menus)
        var dropdownButtons = page.Locator("[data-action='toggle-dropdown']");
        var count = await dropdownButtons.CountAsync();

        // Assert - At least one dropdown should exist
        (count > 0).ShouldBeTrue("At least one dropdown menu should exist");

        for (int i = 0; i < count; i++)
        {
            var dropdownButton = dropdownButtons.Nth(i);

            // Act - Click to expand dropdown
            await dropdownButton.ClickAsync();
            await Task.Delay(200); // Allow animation

            // Assert - aria-expanded should be true
            var ariaExpanded = await dropdownButton.GetAttributeAsync("aria-expanded");
            ariaExpanded.ShouldBe("true", $"Dropdown {i} should be expanded after click");

            // Assert - Dropdown content should be visible
            var dropdownId = await dropdownButton.GetAttributeAsync("aria-controls");
            if (!string.IsNullOrEmpty(dropdownId))
            {
                var dropdownContent = page.Locator($"#{dropdownId}");
                var isVisible = await dropdownContent.IsVisibleAsync();
                isVisible.ShouldBeTrue($"Dropdown {i} content should be visible");
            }

            // Act - Click again to collapse
            await dropdownButton.ClickAsync();
            await Task.Delay(200); // Allow animation

            // Assert - aria-expanded should be false
            var ariaExpandedClosed = await dropdownButton.GetAttributeAsync("aria-expanded");
            ariaExpandedClosed.ShouldBe("false", $"Dropdown {i} should be collapsed after second click");
        }
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task HamburgerMenu_OpensAndCloses_OnMobile(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.Mobile });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync(page);

        var hamburgerMenu = page.Locator("[data-action='toggle-menu']");

        // Assert - Hamburger should be visible on mobile
        await hamburgerMenu.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        (await hamburgerMenu.IsVisibleAsync()).ShouldBeTrue("Hamburger menu should be visible on mobile");

        // Act - Click to open
        await hamburgerMenu.ClickAsync();
        await Task.Delay(300); // Allow animation

        // Assert - Menu should be open
        var ariaExpanded = await hamburgerMenu.GetAttributeAsync("aria-expanded");
        ariaExpanded.ShouldBe("true", "Mobile menu should be expanded");

        // Act - Click to close
        await hamburgerMenu.ClickAsync();
        await Task.Delay(300); // Allow animation

        // Assert - Menu should be closed
        var ariaExpandedClosed = await hamburgerMenu.GetAttributeAsync("aria-expanded");
        ariaExpandedClosed.ShouldBe("false", "Mobile menu should be collapsed");
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task KeyboardNavigation_Works_ThroughoutSite(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync(page);

        // Act - Tab through interactive elements
        await page.Keyboard.PressAsync("Tab");
        await Task.Delay(100);

        var firstFocusedElement = await page.EvaluateAsync<string>("document.activeElement?.tagName");
        firstFocusedElement.ShouldNotBeNull();
        firstFocusedElement.ShouldNotBe("BODY"); // Should focus on an actual element, not body

        // Act - Continue tabbing
        for (int i = 0; i < 5; i++)
        {
            await page.Keyboard.PressAsync("Tab");
            await Task.Delay(100);
        }

        // Assert - Should have moved focus to different elements
        var currentFocusedElement = await page.EvaluateAsync<string>("document.activeElement?.tagName");
        currentFocusedElement.ShouldNotBeNull();

        // Act - Test Escape key (should close any open dropdowns)
        var dropdownButton = page.Locator("[data-action='toggle-dropdown']").First;
        if (await dropdownButton.IsVisibleAsync())
        {
            await dropdownButton.ClickAsync();
            await Task.Delay(200);

            await page.Keyboard.PressAsync("Escape");
            await Task.Delay(200);

            var ariaExpanded = await dropdownButton.GetAttributeAsync("aria-expanded");
            ariaExpanded.ShouldBe("false", "Escape key should close dropdown");
        }
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task ClickOutside_ClosesDropdowns(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync(page);

        var dropdownButton = page.Locator("[data-action='toggle-dropdown']").First;

        if (await dropdownButton.IsVisibleAsync())
        {
            // Act - Open dropdown
            await dropdownButton.ClickAsync();
            await Task.Delay(200);

            // Assert - Dropdown is open
            var ariaExpanded = await dropdownButton.GetAttributeAsync("aria-expanded");
            ariaExpanded.ShouldBe("true", "Dropdown should be open");

            // Act - Click outside (on logo or top of page)
            await page.Mouse.ClickAsync(10, 10);
            await Task.Delay(300);

            // Assert - Dropdown should be closed
            var ariaExpandedAfter = await dropdownButton.GetAttributeAsync("aria-expanded");
            ariaExpandedAfter.ShouldBe("false", "Dropdown should close when clicking outside");
        }
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task LogoClick_NavigatesToHomePage(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync(page);

        // Find the logo link
        var logoLink = page.Locator("a[href='/']").First;
        await logoLink.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });

        // Act - Click logo
        await logoLink.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Should be on home page
        var url = page.Url;
        url.ShouldEndWith("/");
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
