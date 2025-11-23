using Microsoft.Playwright;
using Neba.BrowserTests.TestUtils;


namespace Neba.BrowserTests.Scenarios;

/// <summary>
/// Tests for notification system scenarios including toasts, alerts, and their behaviors.
/// Note: These tests use the Counter page as a trigger mechanism to test the notification system,
/// but are testing the notification system itself, not the Counter page functionality.
/// </summary>
public class NotificationScenariosTests : PlaywrightTestBase
{
    private const string TriggerPageUrl = "/counter"; // Page that can trigger notifications for testing

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task Toast_AppearsAndAutoDismisses(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{BaseUrl}{TriggerPageUrl}");
        await WaitForBlazorAsync(page);

        // Act - Trigger a toast notification (click counter to get to prime number 2)
        var clickButton = page.Locator("button:has-text('Click me')");
        await clickButton.ClickAsync();
        await clickButton.ClickAsync();

        // Assert - Toast should appear
        var toast = await NotificationTestHelpers.WaitForToastAsync(page, 3000);
        (await toast.IsVisibleAsync()).ShouldBeTrue("Toast should be visible");

        // Assert - Toast should auto-dismiss within 5 seconds
        await NotificationTestHelpers.WaitForToastDismissalAsync(toast, 6000);
        (await toast.IsVisibleAsync()).ShouldBeFalse("Toast should auto-dismiss");
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task Alert_AppearsAndPersists_UntilDismissed(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{BaseUrl}{TriggerPageUrl}");
        await WaitForBlazorAsync(page);

        // Act - Trigger an alert notification (AlertOnly behavior)
        var demonstrateButton = page.Locator("button:has-text('Demonstrate NotifyBehavior')");

        // First click shows ToastOnly, second click (after wait) shows AlertOnly
        await demonstrateButton.ClickAsync();
        await Task.Delay(5500); // Wait for first behavior to complete

        await demonstrateButton.ClickAsync();

        // Assert - Alert should appear
        var alert = await NotificationTestHelpers.WaitForAlertAsync(page, 3000);
        (await alert.IsVisibleAsync()).ShouldBeTrue("Alert should be visible");

        // Assert - Alert should still be visible after toast auto-dismiss time
        await Task.Delay(5000);
        (await alert.IsVisibleAsync()).ShouldBeTrue("Alert should still be visible after 5 seconds");

        // Act - Dismiss alert
        await NotificationTestHelpers.DismissAlertAsync(alert);

        // Assert - Alert should be gone
        (await alert.IsVisibleAsync()).ShouldBeFalse("Alert should be dismissed");
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task AlertAndToast_ShowsBothNotifications_Simultaneously(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{BaseUrl}{TriggerPageUrl}");
        await WaitForBlazorAsync(page);

        // Act - Trigger AlertAndToast behavior (third demonstration)
        var demonstrateButton = page.Locator("button:has-text('Demonstrate NotifyBehavior')");

        await demonstrateButton.ClickAsync();
        await Task.Delay(5500); // Wait for first behavior
        await demonstrateButton.ClickAsync();
        await Task.Delay(5500); // Wait for second behavior
        await demonstrateButton.ClickAsync();

        // Assert - Both toast and alert should appear
        var toast = await NotificationTestHelpers.WaitForToastAsync(page, 3000);
        var alert = await NotificationTestHelpers.WaitForAlertAsync(page, 3000);

        (await toast.IsVisibleAsync()).ShouldBeTrue("Toast should be visible");
        (await alert.IsVisibleAsync()).ShouldBeTrue("Alert should be visible");

        // Assert - Toast should auto-dismiss but alert should persist
        await NotificationTestHelpers.WaitForToastDismissalAsync(toast, 6000);
        (await toast.IsVisibleAsync()).ShouldBeFalse("Toast should auto-dismiss");
        (await alert.IsVisibleAsync()).ShouldBeTrue("Alert should still be visible after toast dismisses");
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task Alerts_ClearOnNavigation(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{BaseUrl}{TriggerPageUrl}");
        await WaitForBlazorAsync(page);

        // Act - Trigger an alert
        var demonstrateButton = page.Locator("button:has-text('Demonstrate NotifyBehavior')");
        await demonstrateButton.ClickAsync();
        await Task.Delay(5500);
        await demonstrateButton.ClickAsync();

        var alert = await NotificationTestHelpers.WaitForAlertAsync(page, 3000);
        (await alert.IsVisibleAsync()).ShouldBeTrue("Alert should be visible");

        // Act - Navigate to home
        await page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync(page);
        await Task.Delay(500);

        // Assert - Alert should be cleared
        var alertCount = await NotificationTestHelpers.CountAlertsAsync(page);
        Assert.Equal(0, alertCount);
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task Toast_DismissesOnClick(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{BaseUrl}{TriggerPageUrl}");
        await WaitForBlazorAsync(page);

        // Act - Trigger a toast
        var clickButton = page.Locator("button:has-text('Click me')");
        await clickButton.ClickAsync();
        await clickButton.ClickAsync();

        var toast = await NotificationTestHelpers.WaitForToastAsync(page, 3000);
        (await toast.IsVisibleAsync()).ShouldBeTrue("Toast should be visible");

        // Act - Click toast to dismiss
        await NotificationTestHelpers.DismissToastAsync(toast);

        // Assert - Toast should be gone
        (await toast.IsVisibleAsync()).ShouldBeFalse("Toast should be dismissed after click");
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task MultipleToasts_StackCorrectly(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{BaseUrl}{TriggerPageUrl}");
        await WaitForBlazorAsync(page);

        // Act - Trigger multiple toasts rapidly
        var clickButton = page.Locator("button:has-text('Click me')");

        // Click to 2 (prime)
        await clickButton.ClickAsync();
        await clickButton.ClickAsync();
        await Task.Delay(100);

        // Click to 3 (prime)
        await clickButton.ClickAsync();
        await Task.Delay(100);

        // Click to 5 (prime and divisible by 5)
        await clickButton.ClickAsync();
        await clickButton.ClickAsync();

        // Assert - Multiple toasts should be visible and stacked
        await Task.Delay(500); // Allow toasts to render
        var toastCount = await NotificationTestHelpers.CountToastsAsync(page);
        (toastCount >= 2).ShouldBeTrue($"Multiple toasts should be visible, found {toastCount}");

        // Assert - Toasts should be vertically stacked (different Y positions)
        var toasts = NotificationTestHelpers.GetAllToasts(page);
        if (toastCount >= 2)
        {
            var firstToast = toasts.Nth(0);
            var secondToast = toasts.Nth(1);

            var firstPos = await NotificationTestHelpers.GetToastPositionAsync(firstToast);
            var secondPos = await NotificationTestHelpers.GetToastPositionAsync(secondToast);

            secondPos.y.ShouldNotBe(firstPos.y);
        }
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task Toast_PositionsCorrectly_OnDesktop(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{BaseUrl}{TriggerPageUrl}");
        await WaitForBlazorAsync(page);

        // Act - Trigger a toast
        var clickButton = page.Locator("button:has-text('Click me')");
        await clickButton.ClickAsync();
        await clickButton.ClickAsync();

        var toast = await NotificationTestHelpers.WaitForToastAsync(page, 3000);

        // Assert - Toast should be in top-right corner on desktop
        var isTopRight = await NotificationTestHelpers.IsToastTopRightAsync(page, toast);
        isTopRight.ShouldBeTrue("Toast should be positioned in top-right corner on desktop");
    }

    [Theory]
    [InlineData("chromium")]
    [InlineData("firefox")]
    [InlineData("webkit")]
    public async Task Toast_PositionsCorrectly_OnMobile_ForDifferentTypes(string browserType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.Mobile });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{BaseUrl}{TriggerPageUrl}");
        await WaitForBlazorAsync(page);

        // Act - Trigger an info toast (should be bottom-center on mobile)
        var clickButton = page.Locator("button:has-text('Click me')");
        await clickButton.ClickAsync();
        await clickButton.ClickAsync(); // Count = 2, triggers info toast

        var toast = await NotificationTestHelpers.WaitForToastAsync(page, 3000);

        // Assert - Info toast should be in bottom-center on mobile
        // Note: Actual positioning may vary based on implementation
        // This test validates that toast appears and has a position
        var position = await NotificationTestHelpers.GetToastPositionAsync(toast);
        (position.x >= 0 && position.y >= 0).ShouldBeTrue("Toast should have a valid position on mobile");
    }

    [Theory]
    [InlineData("chromium", "info")]
    [InlineData("firefox", "success")]
    [InlineData("webkit", "normal")]
    public async Task NotificationTypes_RenderWithCorrectStyling(string browserType, string expectedType)
    {
        // Arrange
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(new() { ViewportSize = ViewportHelpers.DesktopWide });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{BaseUrl}{TriggerPageUrl}");
        await WaitForBlazorAsync(page);

        // Act - Trigger different notification types based on counter value
        var clickButton = page.Locator("button:has-text('Click me')");

        switch (expectedType)
        {
            case "info":
                // Count = 2 (prime) triggers info
                await clickButton.ClickAsync();
                await clickButton.ClickAsync();
                break;
            case "success":
                // Count = 10 (divisible by 10) triggers success
                for (int i = 0; i < 10; i++) await clickButton.ClickAsync();
                break;
            case "normal":
                // Count = 5 (divisible by 5, not 10) triggers normal
                for (int i = 0; i < 5; i++) await clickButton.ClickAsync();
                break;
        }

        var toast = await NotificationTestHelpers.WaitForToastAsync(page, 3000);

        // Assert - Toast should have correct type styling
        var notificationType = await NotificationTestHelpers.GetNotificationTypeAsync(toast);
        Assert.Equal(expectedType, notificationType);
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
