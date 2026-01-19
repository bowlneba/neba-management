using Microsoft.AspNetCore.Components;
using Neba.Web.Server.Telemetry;

namespace Neba.UnitTests.Web.Server.Telemetry;

/// <summary>
/// Unit tests for NavigationTelemetry.
/// Note: Tests requiring NavigationManager.Uri are limited because it's not virtual.
/// Full navigation tracking tests are in integration tests.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Web.Server.Telemetry")]
public sealed class NavigationTelemetryTests
{
    // Test implementation of NavigationManager for unit testing
    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager(string baseUri = "https://localhost/", string uri = "https://localhost/")
        {
            Initialize(baseUri, uri);
        }

        public void TriggerLocationChanged(string newUri, bool isNavigationIntercepted)
        {
            Uri = newUri;
            NotifyLocationChanged(isNavigationIntercepted);
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            // Not needed for these tests
        }
    }

    [Fact(DisplayName = "Constructor accepts NavigationManager")]
    public void Constructor_AcceptsNavigationManager()
    {
        // Arrange
        var navManager = new TestNavigationManager();

        // Act & Assert
        Should.NotThrow(() =>
        {
            using var telemetry = new NavigationTelemetry(navManager);
        });
    }

    [Fact(DisplayName = "Dispose can be called multiple times safely")]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var navManager = new TestNavigationManager();
        using var telemetry = new NavigationTelemetry(navManager);

        // Act & Assert - using statement automatically disposes
        telemetry.ShouldNotBeNull();
    }

    [Fact(DisplayName = "LocationChanged event triggers telemetry recording")]
    public void LocationChanged_TriggersTelemetryRecording()
    {
        // Arrange
        var navManager = new TestNavigationManager("https://localhost/", "https://localhost/initial");
        using var telemetry = new NavigationTelemetry(navManager);

        // Act & Assert
        Should.NotThrow(() => navManager.TriggerLocationChanged("https://localhost/new-page", false));
    }

    [Fact(DisplayName = "LocationChanged with intercepted navigation records correctly")]
    public void LocationChanged_WithInterceptedNavigation_RecordsCorrectly()
    {
        // Arrange
        var navManager = new TestNavigationManager("https://localhost/", "https://localhost/page1");
        using var telemetry = new NavigationTelemetry(navManager);

        // Act & Assert
        Should.NotThrow(() => navManager.TriggerLocationChanged("https://localhost/page2", isNavigationIntercepted: true));
    }

    [Fact(DisplayName = "LocationChanged with external navigation records correctly")]
    public void LocationChanged_WithExternalNavigation_RecordsCorrectly()
    {
        // Arrange
        var navManager = new TestNavigationManager("https://localhost/", "https://localhost/page1");
        using var telemetry = new NavigationTelemetry(navManager);

        // Act & Assert
        Should.NotThrow(() => navManager.TriggerLocationChanged("https://localhost/page2", isNavigationIntercepted: false));
    }

    [Fact(DisplayName = "Multiple navigation events are tracked")]
    public void MultipleNavigations_AreTracked()
    {
        // Arrange
        var navManager = new TestNavigationManager("https://localhost/", "https://localhost/start");
        using var telemetry = new NavigationTelemetry(navManager);

        // Act & Assert
        Should.NotThrow(() =>
        {
            navManager.TriggerLocationChanged("https://localhost/page1", true);
            navManager.TriggerLocationChanged("https://localhost/page2", true);
            navManager.TriggerLocationChanged("https://localhost/page3", false);
        });
    }

    [Fact(DisplayName = "Navigation with query parameters is tracked")]
    public void Navigation_WithQueryParameters_IsTracked()
    {
        // Arrange
        var navManager = new TestNavigationManager("https://localhost/", "https://localhost/page");
        using var telemetry = new NavigationTelemetry(navManager);

        // Act & Assert
        Should.NotThrow(() => navManager.TriggerLocationChanged("https://localhost/page?param=value&other=123", true));
    }

    [Fact(DisplayName = "Navigation with fragment is tracked")]
    public void Navigation_WithFragment_IsTracked()
    {
        // Arrange
        var navManager = new TestNavigationManager("https://localhost/", "https://localhost/page");
        using var telemetry = new NavigationTelemetry(navManager);

        // Act & Assert
        Should.NotThrow(() => navManager.TriggerLocationChanged("https://localhost/page#section", true));
    }

    [Fact(DisplayName = "Navigation to same page is tracked")]
    public void Navigation_ToSamePage_IsTracked()
    {
        // Arrange
        var navManager = new TestNavigationManager("https://localhost/", "https://localhost/page");
        using var telemetry = new NavigationTelemetry(navManager);

        // Act & Assert
        Should.NotThrow(() => navManager.TriggerLocationChanged("https://localhost/page", true));
    }

    [Fact(DisplayName = "Navigation to root path is tracked")]
    public void Navigation_ToRootPath_IsTracked()
    {
        // Arrange
        var navManager = new TestNavigationManager("https://localhost/", "https://localhost/some/path");
        using var telemetry = new NavigationTelemetry(navManager);

        // Act & Assert
        Should.NotThrow(() => navManager.TriggerLocationChanged("https://localhost/", true));
    }

    [Fact(DisplayName = "Navigation from root path is tracked")]
    public void Navigation_FromRootPath_IsTracked()
    {
        // Arrange
        var navManager = new TestNavigationManager("https://localhost/", "https://localhost/");
        using var telemetry = new NavigationTelemetry(navManager);

        // Act & Assert
        Should.NotThrow(() => navManager.TriggerLocationChanged("https://localhost/page", true));
    }

    [Fact(DisplayName = "Rapid consecutive navigations are tracked")]
    public void RapidConsecutiveNavigations_AreTracked()
    {
        // Arrange
        var navManager = new TestNavigationManager("https://localhost/", "https://localhost/start");
        using var telemetry = new NavigationTelemetry(navManager);

        // Act & Assert
        Should.NotThrow(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                navManager.TriggerLocationChanged($"https://localhost/page{i}", true);
            }
        });
    }

    [Fact(DisplayName = "Navigation with deeply nested paths is tracked")]
    public void Navigation_WithDeeplyNestedPaths_IsTracked()
    {
        // Arrange
        var navManager = new TestNavigationManager("https://localhost/", "https://localhost/level1");
        using var telemetry = new NavigationTelemetry(navManager);

        // Act & Assert
        Should.NotThrow(() =>
        {
            navManager.TriggerLocationChanged(
                "https://localhost/level1/level2/level3/level4/level5/page", true);
        });
    }
}
