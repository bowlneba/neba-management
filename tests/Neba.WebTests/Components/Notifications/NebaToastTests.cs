using Bunit;
using Microsoft.AspNetCore.Components;
using Neba.Web.Server.Notifications;

namespace Neba.WebTests.Components.Notifications;

[Trait("Category", "Web")]
[Trait("Component", "Components.Notifications")]

public sealed class NebaToastTests : TestContextWrapper
{
    [Fact]
    public void ShouldRenderToastWithMessage()
    {
        // Arrange
        NotificationPayload payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        AngleSharp.Dom.IElement messageElement = cut.Find(".neba-toast-message");
        messageElement.TextContent.ShouldBe("Test message");
    }

    [Fact]
    public void ShouldRenderToastWithTitleAndMessage()
    {
        // Arrange
        NotificationPayload payload = new NotificationPayload(NotifySeverity.Warning, "Test message", "Test Title");

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        AngleSharp.Dom.IElement titleElement = cut.Find(".neba-toast-title");
        titleElement.TextContent.ShouldBe("Test Title");

        AngleSharp.Dom.IElement messageElement = cut.Find(".neba-toast-message");
        messageElement.TextContent.ShouldBe("Test message");
    }

    [Fact]
    public void ShouldNotRenderTitleWhenNotProvided()
    {
        // Arrange
        NotificationPayload payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        cut.FindAll(".neba-toast-title").ShouldBeEmpty();
    }

    [Theory(DisplayName = "Applies correct severity class for toast")]
    [InlineData(NotifySeverity.Error, "error", TestDisplayName = "Error severity applies error class")]
    [InlineData(NotifySeverity.Warning, "warning", TestDisplayName = "Warning severity applies warning class")]
    [InlineData(NotifySeverity.Success, "success", TestDisplayName = "Success severity applies success class")]
    [InlineData(NotifySeverity.Info, "info", TestDisplayName = "Info severity applies info class")]
    [InlineData(NotifySeverity.Normal, "normal", TestDisplayName = "Normal severity applies normal class")]
    public void ShouldApplyCorrectSeverityClass(NotifySeverity severity, string expectedClass)
    {
        // Arrange
        NotificationPayload payload = new NotificationPayload(severity, "Test message");

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        AngleSharp.Dom.IElement toastElement = cut.Find(".neba-toast");
        toastElement.ClassList.ShouldContain($"neba-toast-{expectedClass}");
    }

    [Theory(DisplayName = "Sets correct ARIA live value for toast based on severity")]
    [InlineData(NotifySeverity.Error, "assertive", TestDisplayName = "Error severity sets assertive aria-live")]
    [InlineData(NotifySeverity.Warning, "assertive", TestDisplayName = "Warning severity sets assertive aria-live")]
    [InlineData(NotifySeverity.Success, "polite", TestDisplayName = "Success severity sets polite aria-live")]
    [InlineData(NotifySeverity.Info, "polite", TestDisplayName = "Info severity sets polite aria-live")]
    [InlineData(NotifySeverity.Normal, "polite", TestDisplayName = "Normal severity sets polite aria-live")]
    public void ShouldSetCorrectAriaLive(NotifySeverity severity, string expectedAriaLive)
    {
        // Arrange
        NotificationPayload payload = new NotificationPayload(severity, "Test message");

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        AngleSharp.Dom.IElement toastElement = cut.Find(".neba-toast");
        toastElement.GetAttribute("aria-live").ShouldBe(expectedAriaLive);
    }

    [Fact]
    public void ShouldRenderDismissButton()
    {
        // Arrange
        NotificationPayload payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        AngleSharp.Dom.IElement dismissButton = cut.Find("button.neba-toast-dismiss");
        dismissButton.ShouldNotBeNull();
        dismissButton.GetAttribute("aria-label").ShouldBe("Dismiss notification");
    }

    [Fact]
    public void ShouldInvokeOnDismissWhenDismissButtonClicked()
    {
        // Arrange
        NotificationPayload payload = new NotificationPayload(NotifySeverity.Info, "Test message");
        bool dismissCalled = false;

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload)
            .Add(p => p.OnDismiss, EventCallback.Factory.Create(this, () => dismissCalled = true)));

        AngleSharp.Dom.IElement dismissButton = cut.Find("button.neba-toast-dismiss");
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
        NotificationPayload payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        AngleSharp.Dom.IElement dismissButton = cut.Find("button.neba-toast-dismiss");
        dismissButton.Click();

        // Wait for state change
        cut.WaitForState(() => cut.Find(".neba-toast").ClassList.Contains("dismissing"), timeout: TimeSpan.FromMilliseconds(100));

        // Assert
        AngleSharp.Dom.IElement toastElement = cut.Find(".neba-toast");
        toastElement.ClassList.ShouldContain("dismissing");
    }

    [Fact]
    public void ShouldRenderNebaIconComponent()
    {
        // Arrange
        NotificationPayload payload = new NotificationPayload(NotifySeverity.Success, "Test message");

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        IRenderedComponent<NebaIcon> iconComponent = cut.FindComponent<NebaIcon>();
        iconComponent.ShouldNotBeNull();
        iconComponent.Instance.Severity.ShouldBe(NotifySeverity.Success);
    }

    [Fact]
    public void ShouldAcceptCustomDuration()
    {
        // Arrange
        NotificationPayload payload = new NotificationPayload(NotifySeverity.Info, "Test message");
        TimeSpan customDuration = TimeSpan.FromSeconds(10);

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload)
            .Add(p => p.Duration, customDuration));

        // Assert
        cut.Instance.Duration.ShouldBe(customDuration);
    }

    [Fact]
    public void ShouldUseDefaultDurationWhenNotSpecified()
    {
        // Arrange
        NotificationPayload payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        cut.Instance.Duration.ShouldBe(TimeSpan.FromSeconds(4));
    }

    [Fact]
    public void ShouldEnablePauseOnHoverByDefault()
    {
        // Arrange
        NotificationPayload payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload));

        // Assert
        cut.Instance.PauseOnHover.ShouldBeTrue();
    }

    [Fact]
    public void ShouldAllowDisablingPauseOnHover()
    {
        // Arrange
        NotificationPayload payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload)
            .Add(p => p.PauseOnHover, false));

        // Assert
        cut.Instance.PauseOnHover.ShouldBeFalse();
    }

    [Fact]
    public void ShouldHandleMouseEnterWithoutError()
    {
        // Arrange
        NotificationPayload payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload)
            .Add(p => p.PauseOnHover, true));

        AngleSharp.Dom.IElement toastElement = cut.Find(".neba-toast");

        // Assert - should not throw
        Action act = () => toastElement.MouseEnter();
        act.ShouldNotThrow();
    }

    [Fact]
    public void ShouldHandleMouseLeaveWithoutError()
    {
        // Arrange
        NotificationPayload payload = new NotificationPayload(NotifySeverity.Info, "Test message");

        // Act
        IRenderedComponent<NebaToast> cut = Render<NebaToast>(parameters => parameters
            .Add(p => p.Payload, payload)
            .Add(p => p.PauseOnHover, true));

        AngleSharp.Dom.IElement toastElement = cut.Find(".neba-toast");
        toastElement.MouseEnter();

        // Assert - should not throw
        Action act = () => toastElement.MouseLeave();
        act.ShouldNotThrow();
    }
}
