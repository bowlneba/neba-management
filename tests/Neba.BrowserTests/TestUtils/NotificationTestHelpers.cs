using Microsoft.Playwright;

namespace Neba.BrowserTests.TestUtils;

/// <summary>
/// Helper methods for testing the notification system.
/// These helpers trigger notifications through the UI to test the real notification flow.
/// </summary>
public static class NotificationTestHelpers
{
    /// <summary>
    /// Waits for a toast notification to appear.
    /// </summary>
    public static async Task<ILocator> WaitForToastAsync(IPage page, int timeoutMs = 5000)
    {
        var toast = page.Locator(".toast-item").First;
        await toast.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        return toast;
    }

    /// <summary>
    /// Waits for an alert notification to appear.
    /// </summary>
    public static async Task<ILocator> WaitForAlertAsync(IPage page, int timeoutMs = 5000)
    {
        var alert = page.Locator(".alert-item").First;
        await alert.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        return alert;
    }

    /// <summary>
    /// Gets all visible toast notifications.
    /// </summary>
    public static ILocator GetAllToasts(IPage page)
    {
        return page.Locator(".toast-item");
    }

    /// <summary>
    /// Gets all visible alert notifications.
    /// </summary>
    public static ILocator GetAllAlerts(IPage page)
    {
        return page.Locator(".alert-item");
    }

    /// <summary>
    /// Waits for a toast to disappear (auto-dismiss).
    /// </summary>
    public static async Task WaitForToastDismissalAsync(ILocator toast, int timeoutMs = 5000)
    {
        await toast.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = timeoutMs });
    }

    /// <summary>
    /// Dismisses a toast by clicking it.
    /// </summary>
    public static async Task DismissToastAsync(ILocator toast)
    {
        await toast.ClickAsync();
        await toast.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    /// <summary>
    /// Dismisses an alert by clicking its close button.
    /// </summary>
    public static async Task DismissAlertAsync(ILocator alert)
    {
        var closeButton = alert.Locator("button[aria-label='Close']").Or(alert.Locator(".alert-close"));
        await closeButton.ClickAsync();
        await alert.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    /// <summary>
    /// Gets the notification type/severity from CSS classes.
    /// Returns: "normal", "info", "success", "warning", or "error"
    /// </summary>
    public static async Task<string> GetNotificationTypeAsync(ILocator notification)
    {
        var classes = await notification.GetAttributeAsync("class") ?? "";

        if (classes.Contains("info")) return "info";
        if (classes.Contains("success")) return "success";
        if (classes.Contains("warning")) return "warning";
        if (classes.Contains("error")) return "error";
        return "normal";
    }

    /// <summary>
    /// Gets the position of a toast notification.
    /// Returns viewport-relative coordinates.
    /// </summary>
    public static async Task<(double x, double y)> GetToastPositionAsync(ILocator toast)
    {
        var box = await toast.BoundingBoxAsync();
        if (box == null) throw new InvalidOperationException("Toast has no bounding box");

        return (box.X, box.Y);
    }

    /// <summary>
    /// Checks if a toast is positioned in the top-right corner (desktop).
    /// </summary>
    public static async Task<bool> IsToastTopRightAsync(IPage page, ILocator toast)
    {
        var viewportSize = page.ViewportSize;
        if (viewportSize == null) return false;

        var position = await GetToastPositionAsync(toast);

        // Top-right means X is close to right edge, Y is close to top
        return position.x > viewportSize.Width * 0.7 && position.y < 100;
    }

    /// <summary>
    /// Checks if a toast is positioned in the top-center (mobile errors/warnings).
    /// </summary>
    public static async Task<bool> IsToastTopCenterAsync(IPage page, ILocator toast)
    {
        var viewportSize = page.ViewportSize;
        if (viewportSize == null) return false;

        var position = await GetToastPositionAsync(toast);
        var box = await toast.BoundingBoxAsync();
        if (box == null) return false;

        var toastCenter = position.x + (box.Width / 2);
        var viewportCenter = viewportSize.Width / 2;

        // Top-center means horizontally centered, Y is close to top
        return Math.Abs(toastCenter - viewportCenter) < 50 && position.y < 100;
    }

    /// <summary>
    /// Checks if a toast is positioned in the bottom-center (mobile success/info).
    /// </summary>
    public static async Task<bool> IsToastBottomCenterAsync(IPage page, ILocator toast)
    {
        var viewportSize = page.ViewportSize;
        if (viewportSize == null) return false;

        var position = await GetToastPositionAsync(toast);
        var box = await toast.BoundingBoxAsync();
        if (box == null) return false;

        var toastCenter = position.x + (box.Width / 2);
        var viewportCenter = viewportSize.Width / 2;

        // Bottom-center means horizontally centered, Y is close to bottom
        return Math.Abs(toastCenter - viewportCenter) < 50 && position.y > viewportSize.Height * 0.7;
    }

    /// <summary>
    /// Counts the number of visible toasts.
    /// </summary>
    public static async Task<int> CountToastsAsync(IPage page)
    {
        return await GetAllToasts(page).CountAsync();
    }

    /// <summary>
    /// Counts the number of visible alerts.
    /// </summary>
    public static async Task<int> CountAlertsAsync(IPage page)
    {
        return await GetAllAlerts(page).CountAsync();
    }

    /// <summary>
    /// Gets the message text from a notification.
    /// </summary>
    public static async Task<string?> GetNotificationMessageAsync(ILocator notification)
    {
        var messageElement = notification.Locator(".notification-message, .toast-message, .alert-message").First;
        return await messageElement.TextContentAsync();
    }

    /// <summary>
    /// Gets the title text from a notification (if present).
    /// </summary>
    public static async Task<string?> GetNotificationTitleAsync(ILocator notification)
    {
        try
        {
            var titleElement = notification.Locator(".notification-title, .toast-title, .alert-title").First;
            return await titleElement.TextContentAsync();
        }
        catch
        {
            return null;
        }
    }
}
