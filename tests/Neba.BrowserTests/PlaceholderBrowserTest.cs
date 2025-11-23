using Microsoft.Playwright;

namespace Neba.BrowserTests;

public class PlaceholderBrowserTest
{
    [Fact]
    public async Task PlaywrightPlaceholderTest()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        // Set page content to a simple HTML
        const string html = "<html><body><h1 id='greeting'>Hello, Playwright!</h1></body></html>";
        await page.SetContentAsync(html);
        // Validate that the h1 exists and contains the expected text
        var header = await page.QuerySelectorAsync("#greeting");
        header.ShouldNotBeNull();
        var text = await header.InnerTextAsync();
        text.ShouldBe("Hello, Playwright!");
    }
}
