using System.Collections.ObjectModel;
using Neba.Web.Server.Notifications;

namespace Neba.WebTests.Services;

public sealed class AlertServiceTests
{
    private readonly AlertService _sut;

    public AlertServiceTests()
    {
        _sut = new AlertService();
    }

    [Fact]
    public void CurrentAlert_IsNullByDefault()
    {
        // Assert
        _sut.CurrentAlert.ShouldBeNull();
    }

    [Fact]
    public void ShowNormal_DisplaysNormalAlert()
    {
        // Arrange
        const string message = "Normal message";
        const string title = "Normal Title";

        // Act
        _sut.ShowNormal(message, title);

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        _sut.CurrentAlert.Severity.ShouldBe(NotifySeverity.Normal);
        _sut.CurrentAlert.Message.ShouldBe(message);
        _sut.CurrentAlert.Title.ShouldBe(title);
        _sut.CurrentAlert.PersistAcrossNavigation.ShouldBeFalse();
    }

    [Fact]
    public void ShowInfo_DisplaysInfoAlert()
    {
        // Arrange
        const string message = "Info message";

        // Act
        _sut.ShowInfo(message);

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        _sut.CurrentAlert.Severity.ShouldBe(NotifySeverity.Info);
        _sut.CurrentAlert.Message.ShouldBe(message);
        _sut.CurrentAlert.Title.ShouldBeNull();
    }

    [Fact]
    public void ShowSuccess_DisplaysSuccessAlert()
    {
        // Arrange
        const string message = "Success message";
        const string title = "Success";

        // Act
        _sut.ShowSuccess(message, title);

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        _sut.CurrentAlert.Severity.ShouldBe(NotifySeverity.Success);
        _sut.CurrentAlert.Message.ShouldBe(message);
        _sut.CurrentAlert.Title.ShouldBe(title);
    }

    [Fact]
    public void ShowWarning_DisplaysWarningAlert()
    {
        // Arrange
        const string message = "Warning message";

        // Act
        _sut.ShowWarning(message);

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        _sut.CurrentAlert.Severity.ShouldBe(NotifySeverity.Warning);
        _sut.CurrentAlert.Message.ShouldBe(message);
    }

    [Fact]
    public void ShowError_DisplaysErrorAlert()
    {
        // Arrange
        const string message = "Error message";
        const string title = "Error";

        // Act
        _sut.ShowError(message, title);

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        _sut.CurrentAlert.Severity.ShouldBe(NotifySeverity.Error);
        _sut.CurrentAlert.Message.ShouldBe(message);
        _sut.CurrentAlert.Title.ShouldBe(title);
    }

    [Fact]
    public void ShowAlert_CanConfigureOptions()
    {
        // Arrange
        const string message = "Configured alert";

        // Act
        _sut.ShowInfo(message, configure: options =>
        {
            options.Dismissible = true;
            options.ShowIcon = true;
        });

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        _sut.CurrentAlert.Options.Dismissible.ShouldBeTrue();
        _sut.CurrentAlert.Options.ShowIcon.ShouldBeTrue();
    }

    [Fact]
    public void ShowAlert_CanPersistAcrossNavigation()
    {
        // Arrange
        const string message = "Persistent alert";

        // Act
        _sut.ShowSuccess(message, persistAcrossNavigation: true);

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        _sut.CurrentAlert.PersistAcrossNavigation.ShouldBeTrue();
    }

    [Fact]
    public void ShowValidation_DisplaysValidationMessages()
    {
        // Arrange
        ReadOnlyCollection<string> messages = new List<string> { "Field is required", "Invalid format" }.AsReadOnly();
        const string title = "Validation Errors";

        // Act
        _sut.ShowValidation(messages, title);

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        _sut.CurrentAlert.Severity.ShouldBe(NotifySeverity.Error);
        _sut.CurrentAlert.Title.ShouldBe(title);
        _sut.CurrentAlert!.ValidationMessages.ShouldBe(messages);
        _sut.CurrentAlert.Message.ShouldBeEmpty();
    }

    [Fact]
    public void ShowValidation_WithDefaultTitle()
    {
        // Arrange
        ReadOnlyCollection<string> messages = new List<string> { "Field is required" }.AsReadOnly();

        // Act
        _sut.ShowValidation(messages);

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        _sut.CurrentAlert.Title.ShouldBe("Please fix the following:");
    }

    [Fact]
    public void ShowValidation_AlternateSignature()
    {
        // Arrange
        ReadOnlyCollection<string> messages = new List<string> { "Error 1", "Error 2" }.AsReadOnly();
        const string title = "Custom Title";

        // Act
        _sut.ShowValidation(title, messages);

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        _sut.CurrentAlert.Title.ShouldBe(title);
        _sut.CurrentAlert!.ValidationMessages.ShouldBe(messages);
    }

    [Fact]
    public void Clear_RemovesCurrentAlert()
    {
        // Arrange
        _sut.ShowInfo("Test message");
        _sut.CurrentAlert.ShouldNotBeNull();

        // Act
        _sut.Clear();

        // Assert
        _sut.CurrentAlert.ShouldBeNull();
    }

    [Fact]
    public void ShowAlert_ReplacesExistingAlert()
    {
        // Arrange
        _sut.ShowInfo("First alert");

        // Act
        _sut.ShowWarning("Second alert");

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        _sut.CurrentAlert.Severity.ShouldBe(NotifySeverity.Warning);
        _sut.CurrentAlert.Message.ShouldBe("Second alert");
    }

    [Fact]
    public void OnChange_IsTriggeredWhenAlertIsShown()
    {
        // Arrange
        bool changeEventTriggered = false;
        _sut.OnChange += (sender, args) => changeEventTriggered = true;

        // Act
        _sut.ShowInfo("Test message");

        // Assert
        changeEventTriggered.ShouldBeTrue();
    }

    [Fact]
    public void OnChange_IsTriggeredWhenAlertIsCleared()
    {
        // Arrange
        _sut.ShowInfo("Test message");
        bool changeEventTriggered = false;
        _sut.OnChange += (sender, args) => changeEventTriggered = true;

        // Act
        _sut.Clear();

        // Assert
        changeEventTriggered.ShouldBeTrue();
    }

    [Fact]
    public void OnChange_IsTriggeredMultipleTimes()
    {
        // Arrange
        int eventCount = 0;
        _sut.OnChange += (sender, args) => eventCount++;

        // Act
        _sut.ShowInfo("First");
        _sut.ShowWarning("Second");
        _sut.Clear();
        _sut.ShowError("Third");

        // Assert
        eventCount.ShouldBe(4);
    }

    [Theory]
    [InlineData(NotifySeverity.Normal)]
    [InlineData(NotifySeverity.Info)]
    [InlineData(NotifySeverity.Success)]
    [InlineData(NotifySeverity.Warning)]
    [InlineData(NotifySeverity.Error)]
    public void AllSeverityTypes_CanBeDisplayed(NotifySeverity severity)
    {
        // Arrange & Act
        switch (severity)
        {
            case NotifySeverity.Normal:
                _sut.ShowNormal("Test");
                break;
            case NotifySeverity.Info:
                _sut.ShowInfo("Test");
                break;
            case NotifySeverity.Success:
                _sut.ShowSuccess("Test");
                break;
            case NotifySeverity.Warning:
                _sut.ShowWarning("Test");
                break;
            case NotifySeverity.Error:
                _sut.ShowError("Test");
                break;
        }

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        _sut.CurrentAlert.Severity.ShouldBe(severity);
    }

    [Fact]
    public void AlertOptions_DefaultValues()
    {
        // Arrange & Act
        _sut.ShowInfo("Test");

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        _sut.CurrentAlert.Options.ShouldNotBeNull();
    }

    [Fact]
    public void MultipleSubscribers_AllReceiveNotifications()
    {
        // Arrange
        int subscriber1Count = 0;
        int subscriber2Count = 0;
        _sut.OnChange += (sender, args) => subscriber1Count++;
        _sut.OnChange += (sender, args) => subscriber2Count++;

        // Act
        _sut.ShowInfo("Test");
        _sut.Clear();

        // Assert
        subscriber1Count.ShouldBe(2);
        subscriber2Count.ShouldBe(2);
    }

    [Fact]
    public void Clear_WhenNoAlertIsDisplayed_DoesNotThrow()
    {
        // Arrange
        _sut.CurrentAlert.ShouldBeNull();

        // Act & Assert - Should not throw
        _sut.Clear();
        _sut.CurrentAlert.ShouldBeNull();
    }

    [Fact]
    public void ShowAlert_WithEmptyMessage_StillCreatesAlert()
    {
        // Arrange & Act
        _sut.ShowInfo(string.Empty);

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        _sut.CurrentAlert.Message.ShouldBeEmpty();
    }

    [Fact]
    public void ShowValidation_WithEmptyMessageList_StillCreatesAlert()
    {
        // Arrange
        ReadOnlyCollection<string> emptyMessages = new List<string>().AsReadOnly();

        // Act
        _sut.ShowValidation(emptyMessages);

        // Assert
        _sut.CurrentAlert.ShouldNotBeNull();
        AlertItem alert = _sut.CurrentAlert;
        alert.ShouldNotBeNull();
        alert.ValidationMessages!.Count.ShouldBe(0);
    }
}
