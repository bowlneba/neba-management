using Neba.Web.Server.Notifications;
using Neba.Web.Server.Services;

namespace Neba.UnitTests.Notifications;

public sealed class AlertServiceTests
{
    [Fact]
    public void CurrentAlert_InitiallyNull()
    {
        // Arrange & Act
        var service = new AlertService();

        // Assert
        service.CurrentAlert.ShouldBeNull();
    }

    [Fact]
    public void ShowNormal_SetsCurrentAlertWithNormalSeverity()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowNormal("Test message");

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Normal);
        service.CurrentAlert.Message.ShouldBe("Test message");
        service.CurrentAlert.Title.ShouldBeNull();
        service.CurrentAlert.PersistAcrossNavigation.ShouldBe(false);
        service.CurrentAlert.ValidationMessages.ShouldBeNull();
        service.CurrentAlert.Options.ShouldNotBeNull();
        service.CurrentAlert.Options.Variant.ShouldBe(AlertVariant.Filled);
        service.CurrentAlert.Options.ShowIcon.ShouldBe(true);
        service.CurrentAlert.Options.Dismissible.ShouldBe(true);
    }

    [Fact]
    public void ShowNormal_WithTitle_SetsTitle()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowNormal("Test message", "Test title");

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Normal);
        service.CurrentAlert.Message.ShouldBe("Test message");
        service.CurrentAlert.Title.ShouldBe("Test title");
    }

    [Fact]
    public void ShowNormal_WithConfiguration_AppliesConfiguration()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowNormal("Test message", configure: options =>
        {
            options.Variant = AlertVariant.Outlined;
            options.ShowIcon = false;
            options.Dismissible = false;
        });

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Options.Variant.ShouldBe(AlertVariant.Outlined);
        service.CurrentAlert.Options.ShowIcon.ShouldBe(false);
        service.CurrentAlert.Options.Dismissible.ShouldBe(false);
    }

    [Fact]
    public void ShowNormal_WithPersistAcrossNavigation_SetsPersistence()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowNormal("Test message", persistAcrossNavigation: true);

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.PersistAcrossNavigation.ShouldBe(true);
    }

    [Fact]
    public void ShowInfo_SetsCurrentAlertWithInfoSeverity()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowInfo("Info message");

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Info);
        service.CurrentAlert.Message.ShouldBe("Info message");
    }

    [Fact]
    public void ShowInfo_WithAllParameters_SetsAllProperties()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowInfo("Info message", "Info title", options =>
        {
            options.Variant = AlertVariant.Dense;
        }, persistAcrossNavigation: true);

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Info);
        service.CurrentAlert.Message.ShouldBe("Info message");
        service.CurrentAlert.Title.ShouldBe("Info title");
        service.CurrentAlert.PersistAcrossNavigation.ShouldBe(true);
        service.CurrentAlert.Options.Variant.ShouldBe(AlertVariant.Dense);
    }

    [Fact]
    public void ShowSuccess_SetsCurrentAlertWithSuccessSeverity()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowSuccess("Success message");

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Success);
        service.CurrentAlert.Message.ShouldBe("Success message");
    }

    [Fact]
    public void ShowSuccess_WithTitle_SetsTitle()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowSuccess("Success message", "Success!");

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Success);
        service.CurrentAlert.Message.ShouldBe("Success message");
        service.CurrentAlert.Title.ShouldBe("Success!");
    }

    [Fact]
    public void ShowWarning_SetsCurrentAlertWithWarningSeverity()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowWarning("Warning message");

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Warning);
        service.CurrentAlert.Message.ShouldBe("Warning message");
    }

    [Fact]
    public void ShowWarning_WithConfiguration_AppliesConfiguration()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowWarning("Warning message", configure: options =>
        {
            options.Variant = AlertVariant.Outlined;
        });

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Warning);
        service.CurrentAlert.Options.Variant.ShouldBe(AlertVariant.Outlined);
    }

    [Fact]
    public void ShowError_SetsCurrentAlertWithErrorSeverity()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowError("Error message");

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Error);
        service.CurrentAlert.Message.ShouldBe("Error message");
    }

    [Fact]
    public void ShowError_WithAllParameters_SetsAllProperties()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowError("Error message", "Error!", options =>
        {
            options.Dismissible = false;
        }, persistAcrossNavigation: true);

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Error);
        service.CurrentAlert.Message.ShouldBe("Error message");
        service.CurrentAlert.Title.ShouldBe("Error!");
        service.CurrentAlert.PersistAcrossNavigation.ShouldBe(true);
        service.CurrentAlert.Options.Dismissible.ShouldBe(false);
    }

    [Fact]
    public void ShowValidation_WithMessages_SetsValidationAlert()
    {
        // Arrange
        var service = new AlertService();
        var messages = new List<string> { "Field1 is required", "Field2 is invalid" };

        // Act
        service.ShowValidation(messages);

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Error);
        service.CurrentAlert.Message.ShouldBe(string.Empty);
        service.CurrentAlert.Title.ShouldBe("Please fix the following:");
        service.CurrentAlert.ValidationMessages.ShouldNotBeNull();
        service.CurrentAlert.ValidationMessages.Count.ShouldBe(2);
        service.CurrentAlert.ValidationMessages[0].ShouldBe("Field1 is required");
        service.CurrentAlert.ValidationMessages[1].ShouldBe("Field2 is invalid");
    }

    [Fact]
    public void ShowValidation_WithCustomTitle_UsesCustomTitle()
    {
        // Arrange
        var service = new AlertService();
        var messages = new List<string> { "Error 1", "Error 2" };

        // Act
        service.ShowValidation(messages, "Custom validation title");

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Title.ShouldBe("Custom validation title");
        service.CurrentAlert.ValidationMessages.ShouldNotBeNull();
        service.CurrentAlert.ValidationMessages.Count.ShouldBe(2);
    }

    [Fact]
    public void ShowValidation_WithTitleFirst_SetsValidationAlert()
    {
        // Arrange
        var service = new AlertService();
        var messages = new List<string> { "Error 1" };

        // Act
        service.ShowValidation("Title first", messages);

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Title.ShouldBe("Title first");
        service.CurrentAlert.ValidationMessages.ShouldNotBeNull();
        service.CurrentAlert.ValidationMessages.Count.ShouldBe(1);
    }

    [Fact]
    public void Clear_SetsCurrentAlertToNull()
    {
        // Arrange
        var service = new AlertService();
        service.ShowInfo("Test message");
        service.CurrentAlert.ShouldNotBeNull();

        // Act
        service.Clear();

        // Assert
        service.CurrentAlert.ShouldBeNull();
    }

    [Fact]
    public void Clear_WhenNoAlert_DoesNotThrow()
    {
        // Arrange
        var service = new AlertService();

        // Act & Assert
        Should.NotThrow(() => service.Clear());
        service.CurrentAlert.ShouldBeNull();
    }

    [Fact]
    public void OnChange_TriggeredWhenAlertShown()
    {
        // Arrange
        var service = new AlertService();
        var changeCount = 0;
        service.OnChange += (sender, args) => changeCount++;

        // Act
        service.ShowInfo("Test message");

        // Assert
        changeCount.ShouldBe(1);
    }

    [Fact]
    public void OnChange_TriggeredWhenAlertCleared()
    {
        // Arrange
        var service = new AlertService();
        service.ShowInfo("Test message");
        var changeCount = 0;
        service.OnChange += (sender, args) => changeCount++;

        // Act
        service.Clear();

        // Assert
        changeCount.ShouldBe(1);
    }

    [Fact]
    public void OnChange_TriggeredMultipleTimes()
    {
        // Arrange
        var service = new AlertService();
        var changeCount = 0;
        service.OnChange += (sender, args) => changeCount++;

        // Act
        service.ShowInfo("Message 1");
        service.ShowSuccess("Message 2");
        service.Clear();
        service.ShowError("Message 3");

        // Assert
        changeCount.ShouldBe(4);
    }

    [Fact]
    public void OnChange_SenderIsAlertService()
    {
        // Arrange
        var service = new AlertService();
        object? capturedSender = null;
        service.OnChange += (sender, args) => capturedSender = sender;

        // Act
        service.ShowInfo("Test message");

        // Assert
        capturedSender.ShouldBe(service);
    }

    [Fact]
    public void OnChange_MultipleSubscribers_AllNotified()
    {
        // Arrange
        var service = new AlertService();
        var count1 = 0;
        var count2 = 0;
        var count3 = 0;
        service.OnChange += (sender, args) => count1++;
        service.OnChange += (sender, args) => count2++;
        service.OnChange += (sender, args) => count3++;

        // Act
        service.ShowInfo("Test message");

        // Assert
        count1.ShouldBe(1);
        count2.ShouldBe(1);
        count3.ShouldBe(1);
    }

    [Fact]
    public void ShowMultipleAlerts_ReplacesCurrentAlert()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowInfo("First message");
        var firstAlert = service.CurrentAlert;
        service.ShowSuccess("Second message");

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.ShouldNotBe(firstAlert);
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Success);
        service.CurrentAlert.Message.ShouldBe("Second message");
    }

    [Fact]
    public async Task AlertOptions_OnCloseIconClicked_CanBeSet()
    {
        // Arrange
        var service = new AlertService();
        var clickHandlerCalled = false;

        // Act
        service.ShowInfo("Test message", configure: options =>
        {
            options.OnCloseIconClicked = () =>
            {
                clickHandlerCalled = true;
                return Task.CompletedTask;
            };
        });

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Options.OnCloseIconClicked.ShouldNotBeNull();

        // Verify the handler works
        await service.CurrentAlert.Options.OnCloseIconClicked.Invoke();
        clickHandlerCalled.ShouldBe(true);
    }

    [Fact]
    public void ShowNormal_WithAllVariants_SetsCorrectVariant()
    {
        // Arrange
        var service = new AlertService();

        // Act & Assert - Filled
        service.ShowNormal("Test", configure: o => o.Variant = AlertVariant.Filled);
        service.CurrentAlert!.Options.Variant.ShouldBe(AlertVariant.Filled);

        // Act & Assert - Outlined
        service.ShowNormal("Test", configure: o => o.Variant = AlertVariant.Outlined);
        service.CurrentAlert!.Options.Variant.ShouldBe(AlertVariant.Outlined);

        // Act & Assert - Dense
        service.ShowNormal("Test", configure: o => o.Variant = AlertVariant.Dense);
        service.CurrentAlert!.Options.Variant.ShouldBe(AlertVariant.Dense);
    }

    [Fact]
    public void ShowValidation_WithEmptyMessages_SetsEmptyList()
    {
        // Arrange
        var service = new AlertService();
        var messages = new List<string>();

        // Act
        service.ShowValidation(messages);

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.ValidationMessages.ShouldNotBeNull();
        service.CurrentAlert.ValidationMessages.Count.ShouldBe(0);
    }

    [Fact]
    public void AllSeverityMethods_SetCorrectSeverity()
    {
        // Arrange
        var service = new AlertService();

        // Normal
        service.ShowNormal("Test");
        service.CurrentAlert!.Severity.ShouldBe(NotifySeverity.Normal);

        // Info
        service.ShowInfo("Test");
        service.CurrentAlert!.Severity.ShouldBe(NotifySeverity.Info);

        // Success
        service.ShowSuccess("Test");
        service.CurrentAlert!.Severity.ShouldBe(NotifySeverity.Success);

        // Warning
        service.ShowWarning("Test");
        service.CurrentAlert!.Severity.ShouldBe(NotifySeverity.Warning);

        // Error
        service.ShowError("Test");
        service.CurrentAlert!.Severity.ShouldBe(NotifySeverity.Error);
    }
}
