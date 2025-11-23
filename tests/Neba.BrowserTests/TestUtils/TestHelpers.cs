using Microsoft.Playwright;

namespace Neba.BrowserTests.TestUtils;

/// <summary>
/// Common helper methods for Playwright tests.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Waits for an element to be visible and returns it.
    /// </summary>
    public static async Task<ILocator> WaitForElementAsync(IPage page, string selector, int timeoutMs = 5000)
    {
        var element = page.Locator(selector);
        await element.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        return element;
    }

    /// <summary>
    /// Waits for an element by test ID.
    /// </summary>
    public static async Task<ILocator> WaitForTestIdAsync(IPage page, string testId, int timeoutMs = 5000)
    {
        return await WaitForElementAsync(page, $"[data-testid='{testId}']", timeoutMs);
    }

    /// <summary>
    /// Clicks an element and waits for navigation if expected.
    /// </summary>
    public static async Task ClickAndWaitAsync(ILocator element, bool waitForNavigation = false)
    {
        await element.ClickAsync();
    }

    /// <summary>
    /// Checks if an element is visible without throwing.
    /// </summary>
    public static async Task<bool> IsVisibleAsync(IPage page, string selector)
    {
        try
        {
            var element = page.Locator(selector);
            return await element.IsVisibleAsync();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the computed style property value for an element.
    /// </summary>
    public static async Task<string?> GetComputedStyleAsync(ILocator element, string property)
    {
        return await element.EvaluateAsync<string?>($@"
            (el, prop) => window.getComputedStyle(el).getPropertyValue(prop)
        ", property);
    }

    /// <summary>
    /// Simulates keyboard navigation with Tab key.
    /// </summary>
    public static async Task TabAsync(IPage page)
    {
        await page.Keyboard.PressAsync("Tab");
    }

    /// <summary>
    /// Simulates pressing Enter key.
    /// </summary>
    public static async Task PressEnterAsync(IPage page)
    {
        await page.Keyboard.PressAsync("Enter");
    }

    /// <summary>
    /// Simulates pressing Escape key.
    /// </summary>
    public static async Task PressEscapeAsync(IPage page)
    {
        await page.Keyboard.PressAsync("Escape");
    }

    /// <summary>
    /// Simulates pressing Space key.
    /// </summary>
    public static async Task PressSpaceAsync(IPage page)
    {
        await page.Keyboard.PressAsync("Space");
    }

    /// <summary>
    /// Gets the active (focused) element.
    /// </summary>
    public static async Task<ILocator?> GetActiveElementAsync(IPage page)
    {
        var handle = await page.EvaluateHandleAsync("() => document.activeElement");
        return handle as ILocator;
    }

    /// <summary>
    /// Waits for a specific amount of time.
    /// Use sparingly - prefer waiting for specific conditions.
    /// </summary>
    public static async Task WaitAsync(int milliseconds)
    {
        await Task.Delay(milliseconds);
    }

    /// <summary>
    /// Clicks outside of a specific element (useful for testing click-outside-to-close).
    /// </summary>
    public static async Task ClickOutsideAsync(IPage page, ILocator element)
    {
        // Get element bounding box
        var box = await element.BoundingBoxAsync();
        if (box == null) throw new InvalidOperationException("Element has no bounding box");

        // Click in top-left corner of page, away from element
        await page.Mouse.ClickAsync(10, 10);
    }

    /// <summary>
    /// Scrolls an element into view.
    /// </summary>
    public static async Task ScrollIntoViewAsync(ILocator element)
    {
        await element.ScrollIntoViewIfNeededAsync();
    }

    /// <summary>
    /// Gets all text content from an element, including children.
    /// </summary>
    public static async Task<string?> GetTextContentAsync(ILocator element)
    {
        return await element.TextContentAsync();
    }

    /// <summary>
    /// Checks if an element has a specific CSS class.
    /// </summary>
    public static async Task<bool> HasClassAsync(ILocator element, string className)
    {
        var classes = await element.GetAttributeAsync("class");
        return classes?.Split(' ').Contains(className) ?? false;
    }

    /// <summary>
    /// Gets the value of a data attribute.
    /// </summary>
    public static async Task<string?> GetDataAttributeAsync(ILocator element, string attributeName)
    {
        return await element.GetAttributeAsync($"data-{attributeName}");
    }

    /// <summary>
    /// Waits for an animation to complete.
    /// </summary>
    public static async Task WaitForAnimationAsync(ILocator element)
    {
        await element.EvaluateAsync(@"
            el => {
                return Promise.all(
                    el.getAnimations().map(animation => animation.finished)
                );
            }
        ");
    }
}
