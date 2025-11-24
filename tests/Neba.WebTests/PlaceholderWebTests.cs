using Bunit;
using Neba.Web.Server.Components.Notifications;
using Neba.Web.Server.Services;

namespace Neba.WebTests;

/// <summary>
/// Placeholder bUnit tests for basic component rendering.
/// Replace with actual component tests as the application grows.
/// </summary>
public class PlaceholderWebTests
{
    [Fact]
    public void NebaIcon_RendersSuccessIcon_WhenSeverityIsSuccess()
    {
        // Arrange
        using var ctx = new BunitContext();

        // Act
        var cut = ctx.Render<NebaIcon>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Success));

        // Assert
        var svg = cut.Find("svg");
        svg.ShouldNotBeNull();
        svg.GetAttribute("aria-hidden").ShouldBe("true");
    }

    [Theory]
    [InlineData(NotifySeverity.Success)]
    [InlineData(NotifySeverity.Error)]
    [InlineData(NotifySeverity.Warning)]
    [InlineData(NotifySeverity.Info)]
    [InlineData(NotifySeverity.Normal)]
    public void NebaIcon_RendersSvg_ForAllSeverityLevels(NotifySeverity severity)
    {
        // Arrange
        using var ctx = new BunitContext();

        // Act
        var cut = ctx.Render<NebaIcon>(parameters => parameters
            .Add(p => p.Severity, severity));

        // Assert
        var svg = cut.Find("svg");
        svg.ShouldNotBeNull("Icon should render an SVG element");
        svg.GetAttribute("width").ShouldBe("20");
        svg.GetAttribute("height").ShouldBe("20");
    }

    [Fact]
    public void NebaIcon_AppliesCssClass_WhenProvided()
    {
        // Arrange
        using var ctx = new BunitContext();
        const string expectedClass = "custom-icon-class";

        // Act
        var cut = ctx.Render<NebaIcon>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Info)
            .Add(p => p.CssClass, expectedClass));

        // Assert
        var svg = cut.Find("svg");
        svg.ClassList.ShouldContain(expectedClass);
    }
}
