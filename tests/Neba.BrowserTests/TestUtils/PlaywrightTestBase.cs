using System.Diagnostics;
using Microsoft.Playwright;
using Xunit;

namespace Neba.BrowserTests.TestUtils;

/// <summary>
/// Base class for all Playwright browser tests.
/// Provides browser setup, configuration, and common utilities.
/// </summary>
public abstract class PlaywrightTestBase : IAsyncLifetime
{
    protected IPlaywright? Playwright { get; private set; }
    protected IBrowser? Browser { get; private set; }
    protected IBrowserContext? Context { get; private set; }
    protected IPage? Page { get; private set; }

    /// <summary>
    /// The base URL for the application under test.
    /// Override in derived classes or set via environment variable.
    /// </summary>
    protected virtual string BaseUrl => Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "https://localhost:5001";

    /// <summary>
    /// The browser type to use for tests.
    /// Override in derived classes for browser-specific tests.
    /// </summary>
    protected virtual string BrowserType => "chromium";

    /// <summary>
    /// Whether to run in headless mode.
    /// </summary>
    protected virtual bool Headless => !Debugger.IsAttached;

    /// <summary>
    /// Default viewport size for tests.
    /// Can be overridden per test using Page.SetViewportSizeAsync().
    /// </summary>
    protected virtual ViewportSize DefaultViewport => new() { Width = 1920, Height = 1080 };

    public virtual async ValueTask InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        Browser = BrowserType.ToLowerInvariant() switch
        {
            "chromium" => await Playwright.Chromium.LaunchAsync(new() { Headless = Headless }),
            "firefox" => await Playwright.Firefox.LaunchAsync(new() { Headless = Headless }),
            "webkit" => await Playwright.Webkit.LaunchAsync(new() { Headless = Headless }),
            _ => throw new ArgumentException($"Unknown browser type: {BrowserType}")
        };

        Context = await Browser.NewContextAsync(new()
        {
            ViewportSize = DefaultViewport,
            IgnoreHTTPSErrors = true, // Allow localhost with self-signed certs
        });

        // Enable tracing for debugging
        await Context.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        Page = await Context.NewPageAsync();
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (Context != null)
        {
            // Save trace on test failure (check if test failed via TestContext if available)
            try
            {
                var tracePath = Path.Combine(
                    Path.GetTempPath(),
                    $"trace-{GetType().Name}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip"
                );
                await Context.Tracing.StopAsync(new() { Path = tracePath });
            }
            catch
            {
                // Ignore trace save errors
            }

            await Context.CloseAsync();
        }

        if (Browser != null)
        {
            await Browser.CloseAsync();
        }

        Playwright?.Dispose();
    }

    /// <summary>
    /// Navigates to the specified path relative to BaseUrl.
    /// </summary>
    protected async Task NavigateToAsync(string path = "/")
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");

        var url = new Uri(new Uri(BaseUrl), path).ToString();
        await Page.GotoAsync(url, new() { WaitUntil = WaitUntilState.NetworkIdle });
    }

    /// <summary>
    /// Takes a screenshot for debugging purposes.
    /// </summary>
    protected async Task<byte[]> TakeScreenshotAsync()
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");
        return await Page.ScreenshotAsync();
    }

    /// <summary>
    /// Waits for Blazor Server to be ready by checking for the Blazor script.
    /// </summary>
    protected async Task WaitForBlazorAsync()
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");

        await Page.WaitForFunctionAsync(@"
            () => window.Blazor !== undefined && window.Blazor._internal !== undefined
        ");
    }

    /// <summary>
    /// Asserts that there is no horizontal scroll on the page.
    /// </summary>
    protected async Task AssertNoHorizontalScrollAsync()
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");

        var hasHorizontalScroll = await Page.EvaluateAsync<bool>(@"
            () => document.documentElement.scrollWidth > document.documentElement.clientWidth
        ");

        Assert.False(hasHorizontalScroll, "Page should not have horizontal scroll");
    }
}
