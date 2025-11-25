using Bunit;
using Microsoft.AspNetCore.Components;
using Neba.Web.Server.Components.Notifications;
using Neba.Web.Server.Services;
using Shouldly;

namespace Neba.WebTests.Components.Notifications;

public sealed class NebaToastTests : TestContextWrapper
{
    [Fact]
    public void ShouldRenderToastWithMessage()
    {
        // Arrange
        var payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        var messageElement = cut.Find(".neba-toast-message");
        messageElement.TextContent.ShouldBe("Test message");
    }

    [Fact]
    public void ShouldRenderToastWithTitleAndMessage()
    {
        // Arrange
        var payload = new NotificationPayload(NotifySeverity.Warning, "Test message", "Test Title");

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        var titleElement = cut.Find(".neba-toast-title");
        titleElement.TextContent.ShouldBe("Test Title");

        var messageElement = cut.Find(".neba-toast-message");
        messageElement.TextContent.ShouldBe("Test message");
    }

    [Fact]
    public void ShouldNotRenderTitleWhenNotProvided()
    {
        // Arrange
        var payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        cut.FindAll(".neba-toast-title").ShouldBeEmpty();
    }

    [Theory]
    [InlineData(NotifySeverity.Error, "error")]
    [InlineData(NotifySeverity.Warning, "warning")]
    [InlineData(NotifySeverity.Success, "success")]
    [InlineData(NotifySeverity.Info, "info")]
    [InlineData(NotifySeverity.Normal, "normal")]
    public void ShouldApplyCorrectSeverityClass(NotifySeverity severity, string expectedClass)
    {
        // Arrange
        var payload = new NotificationPayload(severity, "Test message");

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        var toastElement = cut.Find(".neba-toast");
        toastElement.ClassList.ShouldContain($"neba-toast-{expectedClass}");
    }

    [Theory]
    [InlineData(NotifySeverity.Error, "assertive")]
    [InlineData(NotifySeverity.Warning, "assertive")]
    [InlineData(NotifySeverity.Success, "polite")]
    [InlineData(NotifySeverity.Info, "polite")]
    [InlineData(NotifySeverity.Normal, "polite")]
    public void ShouldSetCorrectAriaLive(NotifySeverity severity, string expectedAriaLive)
    {
        // Arrange
        var payload = new NotificationPayload(severity, "Test message");

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        var toastElement = cut.Find(".neba-toast");
        toastElement.GetAttribute("aria-live").ShouldBe(expectedAriaLive);
    }

    [Fact]
    public void ShouldRenderDismissButton()
    {
        // Arrange
        var payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        var dismissButton = cut.Find("button.neba-toast-dismiss");
        dismissButton.ShouldNotBeNull();
        dismissButton.GetAttribute("aria-label").ShouldBe("Dismiss notification");
    }

    [Fact]
    public void ShouldInvokeOnDismissWhenDismissButtonClicked()
    {
        // Arrange
        var payload = new NotificationPayload(NotifySeverity.Info, "Test message");
        var dismissCalled = false;

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload)
            .Add(p => p.OnDismiss, EventCallback.Factory.Create(this, () => dismissCalled = true)));

        var dismissButton = cut.Find("button.neba-toast-dismiss");
        dismissButton.Click();

        // Allow time for async dismiss operation
        cut.WaitForState(() => dismissCalled, timeout: TimeSpan.FromSeconds(1));

        // Assert
        dismissCalled.ShouldBeTrue();
    }

    [Fact]
    public void ShouldAddDismissingClassWhenDismissed()
    {
        // Arrange
        var payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        var dismissButton = cut.Find("button.neba-toast-dismiss");
        dismissButton.Click();

        // Wait for state change
        cut.WaitForState(() => cut.Find(".neba-toast").ClassList.Contains("dismissing"), timeout: TimeSpan.FromMilliseconds(100));

        // Assert
        var toastElement = cut.Find(".neba-toast");
        toastElement.ClassList.ShouldContain("dismissing");
    }

    [Fact]
    public void ShouldRenderNebaIconComponent()
    {
        // Arrange
        var payload = new NotificationPayload(NotifySeverity.Success, "Test message");

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        var iconComponent = cut.FindComponent<NebaIcon>();
        iconComponent.ShouldNotBeNull();
        iconComponent.Instance.Severity.ShouldBe(NotifySeverity.Success);
    }

    [Fact]
    public void ShouldAcceptCustomDuration()
    {
        // Arrange
        var payload = new NotificationPayload(NotifySeverity.Info, "Test message");
        var customDuration = TimeSpan.FromSeconds(10);

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload)
            .Add(p => p.Duration, customDuration));

        // Assert
        cut.Instance.Duration.ShouldBe(customDuration);
    }

    [Fact]
    public void ShouldUseDefaultDurationWhenNotSpecified()
    {
        // Arrange
        var payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        cut.Instance.Duration.ShouldBe(TimeSpan.FromSeconds(4));
    }

    [Fact]
    public void ShouldEnablePauseOnHoverByDefault()
    {
        // Arrange
        var payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        cut.Instance.PauseOnHover.ShouldBeTrue();
    }

    [Fact]
    public void ShouldAllowDisablingPauseOnHover()
    {
        // Arrange
        var payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload)
            .Add(p => p.PauseOnHover, false));

        // Assert
        cut.Instance.PauseOnHover.ShouldBeFalse();
    }

    [Fact]
    public void ShouldHandleMouseEnterWithoutError()
    {
        // Arrange
        var payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload)
            .Add(p => p.PauseOnHover, true));

        var toastElement = cut.Find(".neba-toast");

        // Assert - should not throw
        var act = () => toastElement.MouseEnter();
        act.ShouldNotThrow();
    }

    [Fact]
    public void ShouldHandleMouseLeaveWithoutError()
    {
        // Arrange
        var payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        var cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload)
            .Add(p => p.PauseOnHover, true));

        var toastElement = cut.Find(".neba-toast");
        toastElement.MouseEnter();

        // Assert - should not throw
        var act = () => toastElement.MouseLeave();
        act.ShouldNotThrow();
    }
}
