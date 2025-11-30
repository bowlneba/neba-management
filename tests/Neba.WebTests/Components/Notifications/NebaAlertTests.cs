using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Neba.Web.Server.Notifications;

namespace Neba.WebTests.Components.Notifications;

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

    [Theory]
    [InlineData(NotifySeverity.Error, "error")]
    [InlineData(NotifySeverity.Warning, "warning")]
    [InlineData(NotifySeverity.Success, "success")]
    [InlineData(NotifySeverity.Info, "info")]
    [InlineData(NotifySeverity.Normal, "normal")]
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

    [Theory]
    [InlineData(AlertVariant.Filled, "filled")]
    [InlineData(AlertVariant.Outlined, "outlined")]
    [InlineData(AlertVariant.Dense, "dense")]
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

    [Theory]
    [InlineData(NotifySeverity.Error, "alert")]
    [InlineData(NotifySeverity.Warning, "alert")]
    [InlineData(NotifySeverity.Success, "status")]
    [InlineData(NotifySeverity.Info, "status")]
    [InlineData(NotifySeverity.Normal, "status")]
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
        var dismissCalled = false;
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
        var closeIconClicked = false;
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
        var dismissCalled = false;
        var closeIconClicked = false;
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
        var validationMessages = new List<string>
        {
            "Email is required",
            "Password must be at least 8 characters"
        };

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
        var validationMessages = new List<string> { "Validation error 1" };

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
