using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Neba.Web.Server.Notifications;

namespace Neba.WebTests.Components.Notifications;
[Trait("Category", "Web")]
[Trait("Component", "Components.Notifications")]

public sealed class NebaAlertTests : TestContextWrapper
{
    [Fact]
    public void ShouldRenderAlertWithMessage()
    {
        // Arrange & Act
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Info)
            .Add(p => p.Message, "Test message"));

        // Assert
        IElement contentElement = cut.Find(".neba-alert-content");
        contentElement.TextContent.ShouldContain("Test message");
    }

    [Fact]
    public void ShouldRenderAlertWithTitleAndMessage()
    {
        // Arrange & Act
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Warning)
            .Add(p => p.Message, "Test message")
            .Add(p => p.Title, "Test Title"));

        // Assert
        IElement titleElement = cut.Find(".neba-alert-title");
        titleElement.TextContent.ShouldBe("Test Title");

        IElement contentElement = cut.Find(".neba-alert-content");
        contentElement.TextContent.ShouldContain("Test message");
    }

    [Fact]
    public void ShouldNotRenderTitleWhenNotProvided()
    {
        // Arrange & Act
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Info)
            .Add(p => p.Message, "Test message"));

        // Assert
        cut.FindAll(".neba-alert-title").ShouldBeEmpty();
    }

    [Theory(DisplayName = "Applies correct severity class for alert")]
    [InlineData(NotifySeverity.Error, "error", TestDisplayName = "Error severity applies error class")]
    [InlineData(NotifySeverity.Warning, "warning", TestDisplayName = "Warning severity applies warning class")]
    [InlineData(NotifySeverity.Success, "success", TestDisplayName = "Success severity applies success class")]
    [InlineData(NotifySeverity.Info, "info", TestDisplayName = "Info severity applies info class")]
    [InlineData(NotifySeverity.Normal, "normal", TestDisplayName = "Normal severity applies normal class")]
    public void ShouldApplyCorrectSeverityClass(NotifySeverity severity, string expectedClass)
    {
        // Arrange & Act
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, severity)
            .Add(p => p.Message, "Test message"));

        // Assert
        IElement alertElement = cut.Find(".neba-alert");
        alertElement.ClassList.ShouldContain($"neba-alert-{expectedClass}");
    }

    [Theory(DisplayName = "Applies correct variant class for alert")]
    [InlineData(AlertVariant.Filled, "filled", TestDisplayName = "Filled variant applies filled class")]
    [InlineData(AlertVariant.Outlined, "outlined", TestDisplayName = "Outlined variant applies outlined class")]
    [InlineData(AlertVariant.Dense, "dense", TestDisplayName = "Dense variant applies dense class")]
    public void ShouldApplyCorrectVariantClass(AlertVariant variant, string expectedClass)
    {
        // Arrange & Act
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Info)
            .Add(p => p.Message, "Test message")
            .Add(p => p.Variant, variant));

        // Assert
        IElement alertElement = cut.Find(".neba-alert");
        alertElement.ClassList.ShouldContain($"neba-alert-{expectedClass}");
    }

    [Fact]
    public void ShouldUseFilledVariantByDefault()
    {
        // Arrange & Act
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Info)
            .Add(p => p.Message, "Test message"));

        // Assert
        IElement alertElement = cut.Find(".neba-alert");
        alertElement.ClassList.ShouldContain("neba-alert-filled");
    }

    [Theory(DisplayName = "Sets correct ARIA role for alert based on severity")]
    [InlineData(NotifySeverity.Error, "alert", TestDisplayName = "Error severity sets alert role")]
    [InlineData(NotifySeverity.Warning, "alert", TestDisplayName = "Warning severity sets alert role")]
    [InlineData(NotifySeverity.Success, "status", TestDisplayName = "Success severity sets status role")]
    [InlineData(NotifySeverity.Info, "status", TestDisplayName = "Info severity sets status role")]
    [InlineData(NotifySeverity.Normal, "status", TestDisplayName = "Normal severity sets status role")]
    public void ShouldSetCorrectAriaRole(NotifySeverity severity, string expectedRole)
    {
        // Arrange & Act
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, severity)
            .Add(p => p.Message, "Test message"));

        // Assert
        IElement alertElement = cut.Find(".neba-alert");
        alertElement.GetAttribute("role").ShouldBe(expectedRole);
    }

    [Fact]
    public void ShouldRenderIconByDefault()
    {
        // Arrange & Act
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Success)
            .Add(p => p.Message, "Test message"));

        // Assert
        IRenderedComponent<NebaIcon> iconComponent = cut.FindComponent<NebaIcon>();
        iconComponent.ShouldNotBeNull();
        iconComponent.Instance.Severity.ShouldBe(NotifySeverity.Success);
    }

    [Fact]
    public void ShouldNotRenderIconWhenShowIconIsFalse()
    {
        // Arrange & Act
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Info)
            .Add(p => p.Message, "Test message")
            .Add(p => p.ShowIcon, false));

        // Assert
        cut.FindAll(".neba-alert-icon").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldNotRenderCloseButtonByDefault()
    {
        // Arrange & Act
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Info)
            .Add(p => p.Message, "Test message"));

        // Assert
        cut.FindAll(".neba-alert-close").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldRenderCloseButtonWhenDismissible()
    {
        // Arrange & Act
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Info)
            .Add(p => p.Message, "Test message")
            .Add(p => p.Dismissible, true));

        // Assert
        IElement closeButton = cut.Find("button.neba-alert-close");
        closeButton.ShouldNotBeNull();
        closeButton.GetAttribute("aria-label").ShouldBe("Dismiss alert");
    }

    [Fact]
    public void ShouldInvokeOnDismissWhenCloseButtonClicked()
    {
        // Arrange
        bool dismissCalled = false;
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Info)
            .Add(p => p.Message, "Test message")
            .Add(p => p.Dismissible, true)
            .Add(p => p.OnDismiss, EventCallback.Factory.Create(this, () => dismissCalled = true)));

        // Act
        IElement closeButton = cut.Find("button.neba-alert-close");
        closeButton.Click();

        // Assert
        dismissCalled.ShouldBeTrue();
    }

    [Fact]
    public void ShouldInvokeOnCloseIconClickedWhenCloseButtonClicked()
    {
        // Arrange
        bool closeIconClicked = false;
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Info)
            .Add(p => p.Message, "Test message")
            .Add(p => p.Dismissible, true)
            .Add(p => p.OnCloseIconClicked, EventCallback.Factory.Create(this, () => closeIconClicked = true)));

        // Act
        IElement closeButton = cut.Find("button.neba-alert-close");
        closeButton.Click();

        // Assert
        closeIconClicked.ShouldBeTrue();
    }

    [Fact]
    public void ShouldInvokeBothCallbacksWhenCloseButtonClicked()
    {
        // Arrange
        bool dismissCalled = false;
        bool closeIconClicked = false;
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Info)
            .Add(p => p.Message, "Test message")
            .Add(p => p.Dismissible, true)
            .Add(p => p.OnDismiss, EventCallback.Factory.Create(this, () => dismissCalled = true))
            .Add(p => p.OnCloseIconClicked, EventCallback.Factory.Create(this, () => closeIconClicked = true)));

        // Act
        IElement closeButton = cut.Find("button.neba-alert-close");
        closeButton.Click();

        // Assert
        dismissCalled.ShouldBeTrue();
        closeIconClicked.ShouldBeTrue();
    }

    [Fact]
    public void ShouldRenderValidationMessagesWhenProvided()
    {
        // Arrange
        List<string> validationMessages =
        [
            "Email is required",
            "Password must be at least 8 characters"
        ];

        // Act
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Error)
            .Add(p => p.Message, "Validation failed")
            .Add(p => p.ValidationMessages, validationMessages));

        // Assert
        IElement listElement = cut.Find("ul.neba-alert-validation-list");
        listElement.ShouldNotBeNull();

        IReadOnlyList<IElement> listItems = cut.FindAll("li");
        listItems.Count.ShouldBe(2);
        listItems[0].TextContent.ShouldBe("Email is required");
        listItems[1].TextContent.ShouldBe("Password must be at least 8 characters");
    }

    [Fact]
    public void ShouldRenderMessageWhenValidationMessagesEmpty()
    {
        // Arrange & Act
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Error)
            .Add(p => p.Message, "Test message")
            .Add(p => p.ValidationMessages, new List<string>()));

        // Assert
        cut.FindAll("ul.neba-alert-validation-list").ShouldBeEmpty();
        IElement contentElement = cut.Find(".neba-alert-content");
        contentElement.TextContent.ShouldContain("Test message");
    }

    [Fact]
    public void ShouldPreferValidationMessagesOverMessage()
    {
        // Arrange
        List<string> validationMessages = ["Validation error 1"];

        // Act
        IRenderedComponent<NebaAlert> cut = Render<NebaAlert>(parameters => parameters
            .Add(p => p.Severity, NotifySeverity.Error)
            .Add(p => p.Message, "Generic message")
            .Add(p => p.ValidationMessages, validationMessages));

        // Assert
        IElement listElement = cut.Find("ul.neba-alert-validation-list");
        listElement.ShouldNotBeNull();
        listElement.TextContent.ShouldContain("Validation error 1");

        // The generic message div should not be rendered when validation messages are present
        IElement contentElement = cut.Find(".neba-alert-content");
        contentElement.InnerHtml.ShouldNotContain("Generic message");
    }
}
