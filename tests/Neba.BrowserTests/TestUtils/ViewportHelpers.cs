using Microsoft.Playwright;

namespace Neba.BrowserTests.TestUtils;

/// <summary>
/// Helper class for viewport and responsive testing.
/// Provides standard viewport sizes matching the application's breakpoints.
/// </summary>
public static class ViewportHelpers
{
    /// <summary>
    /// Mobile viewport (375x667 - iPhone SE size)
    /// Matches --neba-breakpoint-mobile: 767px
    /// </summary>
    public static ViewportSize Mobile => new() { Width = 375, Height = 667 };

    /// <summary>
    /// Tablet viewport (768x1024 - iPad portrait size)
    /// Matches --neba-breakpoint-tablet-min: 768px
    /// </summary>
    public static ViewportSize Tablet => new() { Width = 768, Height = 1024 };

    /// <summary>
    /// Tablet landscape viewport (1100x768)
    /// Just before navbar collapse breakpoint
    /// </summary>
    public static ViewportSize TabletLandscape => new() { Width = 1100, Height = 768 };

    /// <summary>
    /// Desktop tight viewport (1101x768)
    /// Just after navbar collapse breakpoint
    /// Matches --neba-breakpoint-desktop-tight-min: 1101px
    /// </summary>
    public static ViewportSize DesktopTight => new() { Width = 1101, Height = 768 };

    /// <summary>
    /// Desktop medium viewport (1280x720)
    /// Common laptop screen size
    /// </summary>
    public static ViewportSize DesktopMedium => new() { Width = 1280, Height = 720 };

    /// <summary>
    /// Desktop wide viewport (1920x1080)
    /// Full HD desktop size
    /// Matches --neba-breakpoint-desktop-wide-min: 1401px
    /// </summary>
    public static ViewportSize DesktopWide => new() { Width = 1920, Height = 1080 };

    /// <summary>
    /// Gets all standard viewport sizes for comprehensive testing.
    /// </summary>
    public static IEnumerable<ViewportSize> AllViewports => new[]
    {
        Mobile,
        Tablet,
        TabletLandscape,
        DesktopTight,
        DesktopMedium,
        DesktopWide
    };

    /// <summary>
    /// Gets viewport sizes for breakpoint transition testing.
    /// </summary>
    public static IEnumerable<ViewportSize> BreakpointViewports => new[]
    {
        Mobile,
        Tablet,
        TabletLandscape,
        DesktopTight,
        DesktopWide
    };
}
