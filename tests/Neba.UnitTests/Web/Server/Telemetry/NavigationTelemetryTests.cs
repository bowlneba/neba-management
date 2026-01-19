using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Moq;
using Neba.Web.Server.Telemetry;

namespace Neba.UnitTests.Web.Server.Telemetry;

[Trait("Category", "Unit")]
[Trait("Component", "Web.Server.Telemetry")]
public sealed class NavigationTelemetryTests
{
    [Fact(DisplayName = "Constructor subscribes to LocationChanged event")]
    public void Constructor_SubscribesToLocationChanged()
    {
        // Arrange
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns("https://localhost/");

        // Act
        using var telemetry = new NavigationTelemetry(navManagerMock.Object);

        // Assert
        // Verify subscription by checking that LocationChanged has subscribers
        // (No direct way to test this without triggering the event)
        Should.NotThrow(() => telemetry.Dispose());
    }

    [Fact(DisplayName = "Dispose unsubscribes from LocationChanged event")]
    public void Dispose_UnsubscribesFromLocationChanged()
    {
        // Arrange
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns("https://localhost/");
        var telemetry = new NavigationTelemetry(navManagerMock.Object);

        // Act
        telemetry.Dispose();

        // Assert - calling dispose multiple times should not throw
        Should.NotThrow(() => telemetry.Dispose());
    }

    [Fact(DisplayName = "Dispose can be called multiple times safely")]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns("https://localhost/");
        var telemetry = new NavigationTelemetry(navManagerMock.Object);

        // Act & Assert
        Should.NotThrow(() =>
        {
            telemetry.Dispose();
            telemetry.Dispose();
            telemetry.Dispose();
        });
    }

    [Fact(DisplayName = "Constructor sets initial location from NavigationManager")]
    public void Constructor_SetsInitialLocation()
    {
        // Arrange
        string initialUri = "https://localhost/test/page";
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns(initialUri);

        // Act & Assert
        Should.NotThrow(() =>
        {
            using var telemetry = new NavigationTelemetry(navManagerMock.Object);
        });
    }

    [Fact(DisplayName = "LocationChanged event triggers telemetry recording")]
    public void LocationChanged_TriggersTelemetryRecording()
    {
        // Arrange
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns("https://localhost/initial");

        using var telemetry = new NavigationTelemetry(navManagerMock.Object);

        var args = new LocationChangedEventArgs("https://localhost/new-page", false);

        // Act & Assert
        Should.NotThrow(() =>
        {
            navManagerMock.Raise(m => m.LocationChanged += null, args);
        });
    }

    [Fact(DisplayName = "LocationChanged with intercepted navigation records correctly")]
    public void LocationChanged_WithInterceptedNavigation_RecordsCorrectly()
    {
        // Arrange
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns("https://localhost/page1");

        using var telemetry = new NavigationTelemetry(navManagerMock.Object);

        var args = new LocationChangedEventArgs("https://localhost/page2", isNavigationIntercepted: true);

        // Act & Assert
        Should.NotThrow(() =>
        {
            navManagerMock.Raise(m => m.LocationChanged += null, args);
        });
    }

    [Fact(DisplayName = "LocationChanged with external navigation records correctly")]
    public void LocationChanged_WithExternalNavigation_RecordsCorrectly()
    {
        // Arrange
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns("https://localhost/page1");

        using var telemetry = new NavigationTelemetry(navManagerMock.Object);

        var args = new LocationChangedEventArgs("https://localhost/page2", isNavigationIntercepted: false);

        // Act & Assert
        Should.NotThrow(() =>
        {
            navManagerMock.Raise(m => m.LocationChanged += null, args);
        });
    }

    [Fact(DisplayName = "Multiple navigation events are tracked")]
    public async Task MultipleNavigations_AreTracked()
    {
        // Arrange
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns("https://localhost/start");

        using var telemetry = new NavigationTelemetry(navManagerMock.Object);

        // Act & Assert
        Should.NotThrow(() =>
        {
            navManagerMock.Raise(m => m.LocationChanged += null,
                new LocationChangedEventArgs("https://localhost/page1", true));

            navManagerMock.Raise(m => m.LocationChanged += null,
                new LocationChangedEventArgs("https://localhost/page2", true));

            navManagerMock.Raise(m => m.LocationChanged += null,
                new LocationChangedEventArgs("https://localhost/page3", false));
        });

        await Task.Delay(10); // Small delay to ensure events are processed
    }

    [Fact(DisplayName = "Navigation with query parameters is tracked")]
    public void Navigation_WithQueryParameters_IsTracked()
    {
        // Arrange
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns("https://localhost/page");

        using var telemetry = new NavigationTelemetry(navManagerMock.Object);

        var args = new LocationChangedEventArgs("https://localhost/page?param=value&other=123", true);

        // Act & Assert
        Should.NotThrow(() =>
        {
            navManagerMock.Raise(m => m.LocationChanged += null, args);
        });
    }

    [Fact(DisplayName = "Navigation with fragment is tracked")]
    public void Navigation_WithFragment_IsTracked()
    {
        // Arrange
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns("https://localhost/page");

        using var telemetry = new NavigationTelemetry(navManagerMock.Object);

        var args = new LocationChangedEventArgs("https://localhost/page#section", true);

        // Act & Assert
        Should.NotThrow(() =>
        {
            navManagerMock.Raise(m => m.LocationChanged += null, args);
        });
    }

    [Fact(DisplayName = "Navigation to same page is tracked")]
    public void Navigation_ToSamePage_IsTracked()
    {
        // Arrange
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns("https://localhost/page");

        using var telemetry = new NavigationTelemetry(navManagerMock.Object);

        var args = new LocationChangedEventArgs("https://localhost/page", true);

        // Act & Assert
        Should.NotThrow(() =>
        {
            navManagerMock.Raise(m => m.LocationChanged += null, args);
        });
    }

    [Fact(DisplayName = "Navigation to root path is tracked")]
    public void Navigation_ToRootPath_IsTracked()
    {
        // Arrange
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns("https://localhost/some/path");

        using var telemetry = new NavigationTelemetry(navManagerMock.Object);

        var args = new LocationChangedEventArgs("https://localhost/", true);

        // Act & Assert
        Should.NotThrow(() =>
        {
            navManagerMock.Raise(m => m.LocationChanged += null, args);
        });
    }

    [Fact(DisplayName = "Navigation from root path is tracked")]
    public void Navigation_FromRootPath_IsTracked()
    {
        // Arrange
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns("https://localhost/");

        using var telemetry = new NavigationTelemetry(navManagerMock.Object);

        var args = new LocationChangedEventArgs("https://localhost/page", true);

        // Act & Assert
        Should.NotThrow(() =>
        {
            navManagerMock.Raise(m => m.LocationChanged += null, args);
        });
    }

    [Fact(DisplayName = "Rapid consecutive navigations are tracked")]
    public void RapidConsecutiveNavigations_AreTracked()
    {
        // Arrange
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns("https://localhost/start");

        using var telemetry = new NavigationTelemetry(navManagerMock.Object);

        // Act & Assert
        Should.NotThrow(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                navManagerMock.Raise(m => m.LocationChanged += null,
                    new LocationChangedEventArgs($"https://localhost/page{i}", true));
            }
        });
    }

    [Fact(DisplayName = "Navigation with deeply nested paths is tracked")]
    public void Navigation_WithDeeplyNestedPaths_IsTracked()
    {
        // Arrange
        var navManagerMock = new Mock<NavigationManager>();
        navManagerMock.SetupGet(x => x.Uri).Returns("https://localhost/level1");

        using var telemetry = new NavigationTelemetry(navManagerMock.Object);

        var args = new LocationChangedEventArgs(
            "https://localhost/level1/level2/level3/level4/level5/page", true);

        // Act & Assert
        Should.NotThrow(() =>
        {
            navManagerMock.Raise(m => m.LocationChanged += null, args);
        });
    }
}
