using Neba.Web.Server.Notifications;

namespace Neba.UnitTests.Ui.Server.Notifications;
[Trait("Category", "Unit")]
[Trait("Component", "Ui.Server.Notifications")]

public sealed class AlertServiceTests
{
    [Fact(DisplayName = "Current alert is initially null")]
    public void CurrentAlert_InitiallyNull()
    {
        // Arrange & Act
        var service = new AlertService();

        // Assert
        service.CurrentAlert.ShouldBeNull();
    }

    [Fact(DisplayName = "ShowNormal sets current alert with normal severity")]
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

    [Fact(DisplayName = "ShowNormal with title sets the title property")]
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

    [Fact(DisplayName = "ShowNormal applies configuration options correctly")]
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

    [Fact(DisplayName = "ShowNormal with persist flag sets persistence correctly")]
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

    [Fact(DisplayName = "ShowInfo sets current alert with info severity")]
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

    [Fact(DisplayName = "ShowInfo with all parameters sets all properties")]
    public void ShowInfo_WithAllParameters_SetsAllProperties()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowInfo("Info message", "Info title", options => options.Variant = AlertVariant.Dense, persistAcrossNavigation: true);

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Info);
        service.CurrentAlert.Message.ShouldBe("Info message");
        service.CurrentAlert.Title.ShouldBe("Info title");
        service.CurrentAlert.PersistAcrossNavigation.ShouldBe(true);
        service.CurrentAlert.Options.Variant.ShouldBe(AlertVariant.Dense);
    }

    [Fact(DisplayName = "ShowSuccess sets current alert with success severity")]
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

    [Fact(DisplayName = "ShowSuccess with title sets the title property")]
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

    [Fact(DisplayName = "ShowWarning sets current alert with warning severity")]
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

    [Fact(DisplayName = "ShowWarning applies configuration options correctly")]
    public void ShowWarning_WithConfiguration_AppliesConfiguration()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowWarning("Warning message", configure: options => options.Variant = AlertVariant.Outlined);

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Warning);
        service.CurrentAlert.Options.Variant.ShouldBe(AlertVariant.Outlined);
    }

    [Fact(DisplayName = "ShowError sets current alert with error severity")]
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

    [Fact(DisplayName = "ShowError with all parameters sets all properties")]
    public void ShowError_WithAllParameters_SetsAllProperties()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowError("Error message", "Error!", options => options.Dismissible = false, persistAcrossNavigation: true);

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Error);
        service.CurrentAlert.Message.ShouldBe("Error message");
        service.CurrentAlert.Title.ShouldBe("Error!");
        service.CurrentAlert.PersistAcrossNavigation.ShouldBe(true);
        service.CurrentAlert.Options.Dismissible.ShouldBe(false);
    }

    [Fact(DisplayName = "ShowValidation with messages sets validation alert")]
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

    [Fact(DisplayName = "ShowValidation with custom title uses the provided title")]
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

    [Fact(DisplayName = "ShowValidation with title first parameter sets validation alert")]
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

    [Fact(DisplayName = "Clear sets current alert to null")]
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

    [Fact(DisplayName = "Clear does not throw when no alert exists")]
    public void Clear_WhenNoAlert_DoesNotThrow()
    {
        // Arrange
        var service = new AlertService();

        // Act & Assert
        Should.NotThrow(() => service.Clear());
        service.CurrentAlert.ShouldBeNull();
    }

    [Fact(DisplayName = "OnChange event is triggered when alert is shown")]
    public void OnChange_TriggeredWhenAlertShown()
    {
        // Arrange
        var service = new AlertService();
        int changeCount = 0;
        service.OnChange += (_, _) => changeCount++;

        // Act
        service.ShowInfo("Test message");

        // Assert
        changeCount.ShouldBe(1);
    }

    [Fact(DisplayName = "OnChange event is triggered when alert is cleared")]
    public void OnChange_TriggeredWhenAlertCleared()
    {
        // Arrange
        var service = new AlertService();
        service.ShowInfo("Test message");
        int changeCount = 0;
        service.OnChange += (_, _) => changeCount++;

        // Act
        service.Clear();

        // Assert
        changeCount.ShouldBe(1);
    }

    [Fact(DisplayName = "OnChange event is triggered multiple times for multiple operations")]
    public void OnChange_TriggeredMultipleTimes()
    {
        // Arrange
        var service = new AlertService();
        int changeCount = 0;
        service.OnChange += (_, _) => changeCount++;

        // Act
        service.ShowInfo("Message 1");
        service.ShowSuccess("Message 2");
        service.Clear();
        service.ShowError("Message 3");

        // Assert
        changeCount.ShouldBe(4);
    }

    [Fact(DisplayName = "OnChange event sender is the AlertService instance")]
    public void OnChange_SenderIsAlertService()
    {
        // Arrange
        var service = new AlertService();
        object? capturedSender = null;
        service.OnChange += (sender, _) => capturedSender = sender;

        // Act
        service.ShowInfo("Test message");

        // Assert
        capturedSender.ShouldBe(service);
    }

    [Fact(DisplayName = "OnChange event notifies all subscribers")]
    public void OnChange_MultipleSubscribers_AllNotified()
    {
        // Arrange
        var service = new AlertService();
        int count1 = 0;
        int count2 = 0;
        int count3 = 0;
        service.OnChange += (_, _) => count1++;
        service.OnChange += (_, _) => count2++;
        service.OnChange += (_, _) => count3++;

        // Act
        service.ShowInfo("Test message");

        // Assert
        count1.ShouldBe(1);
        count2.ShouldBe(1);
        count3.ShouldBe(1);
    }

    [Fact(DisplayName = "Showing multiple alerts replaces the current alert")]
    public void ShowMultipleAlerts_ReplacesCurrentAlert()
    {
        // Arrange
        var service = new AlertService();

        // Act
        service.ShowInfo("First message");
        AlertItem? firstAlert = service.CurrentAlert;
        service.ShowSuccess("Second message");

        // Assert
        service.CurrentAlert.ShouldNotBeNull();
        service.CurrentAlert.ShouldNotBe(firstAlert);
        service.CurrentAlert.Severity.ShouldBe(NotifySeverity.Success);
        service.CurrentAlert.Message.ShouldBe("Second message");
    }

    [Fact(DisplayName = "Alert options OnCloseIconClicked handler can be set")]
    public async Task AlertOptions_OnCloseIconClicked_CanBeSet()
    {
        // Arrange
        var service = new AlertService();
        bool clickHandlerCalled = false;

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

    [Fact(DisplayName = "ShowNormal with all variants sets the correct variant")]
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

    [Fact(DisplayName = "ShowValidation with empty messages sets empty list")]
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

    [Fact(DisplayName = "All severity methods set the correct severity")]
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
