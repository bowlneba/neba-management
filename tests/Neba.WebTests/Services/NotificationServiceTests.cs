using System.Reactive.Linq;
using Neba.Web.Server.Notifications;

namespace Neba.WebTests.Services;

public sealed class NotificationServiceTests : IDisposable
{
    private readonly NotificationService _sut;
    private readonly List<NotificationPayload> _receivedNotifications;
    private readonly IDisposable _subscription;

    public NotificationServiceTests()
    {
        _sut = new NotificationService();
        _receivedNotifications = [];
        _subscription = _sut.Notifications.Subscribe(notification => _receivedNotifications.Add(notification));
    }

    [Fact]
    public void Info_PublishesInfoNotification()
    {
        // Arrange
        const string message = "Info message";
        const string title = "Info Title";

        // Act
        _sut.Info(message, title);

        // Assert
        _receivedNotifications.Count.ShouldBe(1);
        NotificationPayload notification = _receivedNotifications[0];
        notification.Severity.ShouldBe(NotifySeverity.Info);
        notification.Message.ShouldBe(message);
        notification.Title.ShouldBe(title);
        notification.Behavior.ShouldBe(NotifyBehavior.ToastOnly);
    }

    [Fact]
    public void Success_PublishesSuccessNotification()
    {
        // Arrange
        const string message = "Success message";

        // Act
        _sut.Success(message);

        // Assert
        _receivedNotifications.Count.ShouldBe(1);
        NotificationPayload notification = _receivedNotifications[0];
        notification.Severity.ShouldBe(NotifySeverity.Success);
        notification.Message.ShouldBe(message);
        notification.Title.ShouldBeNull();
    }

    [Fact]
    public void Warning_PublishesWarningNotification()
    {
        // Arrange
        const string message = "Warning message";
        const string title = "Warning";

        // Act
        _sut.Warning(message, title);

        // Assert
        _receivedNotifications.Count.ShouldBe(1);
        NotificationPayload notification = _receivedNotifications[0];
        notification.Severity.ShouldBe(NotifySeverity.Warning);
        notification.Message.ShouldBe(message);
        notification.Title.ShouldBe(title);
    }

    [Fact]
    public void Error_PublishesErrorNotification()
    {
        // Arrange
        const string message = "Error message";

        // Act
        _sut.Error(message);

        // Assert
        _receivedNotifications.Count.ShouldBe(1);
        NotificationPayload notification = _receivedNotifications[0];
        notification.Severity.ShouldBe(NotifySeverity.Error);
        notification.Message.ShouldBe(message);
    }

    [Fact]
    public void Normal_PublishesNormalNotification()
    {
        // Arrange
        const string message = "Normal message";

        // Act
        _sut.Normal(message);

        // Assert
        _receivedNotifications.Count.ShouldBe(1);
        NotificationPayload notification = _receivedNotifications[0];
        notification.Severity.ShouldBe(NotifySeverity.Normal);
        notification.Message.ShouldBe(message);
    }

    [Fact]
    public void Info_UsesCustomBehavior_WhenProvided()
    {
        // Arrange
        const string message = "Alert message";

        // Act
        _sut.Info(message, behavior: NotifyBehavior.AlertOnly);

        // Assert
        _receivedNotifications.Count.ShouldBe(1);
        _receivedNotifications[0].Behavior.ShouldBe(NotifyBehavior.AlertOnly);
    }

    [Fact]
    public void ValidationFailure_PublishesErrorWithDefaultBehavior()
    {
        // Arrange
        const string message = "Validation error";

        // Act
        _sut.ValidationFailure(message);

        // Assert
        _receivedNotifications.Count.ShouldBe(1);
        NotificationPayload notification = _receivedNotifications[0];
        notification.Severity.ShouldBe(NotifySeverity.Error);
        notification.Message.ShouldBe(message);
        notification.Title.ShouldBe("Validation Failed");
        notification.Behavior.ShouldBe(NotifyBehavior.AlertAndToast);
        notification.Persist.ShouldBeFalse();
        notification.Code.ShouldBe("VALIDATION_FAILURE");
    }

    [Fact]
    public void ValidationFailure_UsesOverrideBehavior_WhenProvided()
    {
        // Arrange
        const string message = "Validation error";

        // Act
        _sut.ValidationFailure(message, NotifyBehavior.ToastOnly);

        // Assert
        _receivedNotifications.Count.ShouldBe(1);
        _receivedNotifications[0].Behavior.ShouldBe(NotifyBehavior.ToastOnly);
    }

    [Fact]
    public void Publish_PublishesCustomPayload()
    {
        // Arrange
        var payload = new NotificationPayload(
            NotifySeverity.Warning,
            "Custom message",
            "Custom title",
            NotifyBehavior.AlertAndToast,
            Persist: true,
            Code: "CUSTOM_CODE");

        // Act
        _sut.Publish(payload);

        // Assert
        _receivedNotifications.Count.ShouldBe(1);
        NotificationPayload received = _receivedNotifications[0];
        received.Severity.ShouldBe(NotifySeverity.Warning);
        received.Message.ShouldBe("Custom message");
        received.Title.ShouldBe("Custom title");
        received.Behavior.ShouldBe(NotifyBehavior.AlertAndToast);
        received.Persist.ShouldBeTrue();
        received.Code.ShouldBe("CUSTOM_CODE");
    }

    [Fact]
    public void Publish_ThrowsObjectDisposedException_WhenServiceIsDisposed()
    {
        // Arrange
        _sut.Dispose();
        var payload = new NotificationPayload(NotifySeverity.Info, "Test", null, NotifyBehavior.ToastOnly);

        // Act & Assert
        Should.Throw<ObjectDisposedException>(() => _sut.Publish(payload));
    }

    [Fact]
    public void MultipleSubscribers_ReceiveAllNotifications()
    {
        // Arrange
        var subscriber1Notifications = new List<NotificationPayload>();
        var subscriber2Notifications = new List<NotificationPayload>();

        using IDisposable subscription1 = _sut.Notifications.Subscribe(n => subscriber1Notifications.Add(n));
        using IDisposable subscription2 = _sut.Notifications.Subscribe(n => subscriber2Notifications.Add(n));

        // Act
        _sut.Info("Message 1");
        _sut.Success("Message 2");

        // Assert
        subscriber1Notifications.Count.ShouldBe(2);
        subscriber2Notifications.Count.ShouldBe(2);
    }

    [Fact]
    public void Notifications_CanBeFiltered()
    {
        // Arrange
        var errorNotifications = new List<NotificationPayload>();
        using IDisposable subscription = _sut.Notifications
            .Where(n => n.Severity == NotifySeverity.Error)
            .Subscribe(n => errorNotifications.Add(n));

        // Act
        _sut.Info("Info message");
        _sut.Error("Error message 1");
        _sut.Warning("Warning message");
        _sut.Error("Error message 2");

        // Assert
        errorNotifications.Count.ShouldBe(2);
        errorNotifications.All(n => n.Severity == NotifySeverity.Error).ShouldBeTrue();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
#pragma warning disable S3966 // Disabled to allow multiple calls to Dispose in test
        // Act
        _sut.Dispose();
        _sut.Dispose();

        // Assert - Should not throw
        true.ShouldBeTrue();

#pragma warning restore S3966
    }

    [Fact]
    public void Notifications_StopAfterDispose()
    {
        // Arrange
        var receivedAfterDispose = new List<NotificationPayload>();
        using IDisposable subscription = _sut.Notifications.Subscribe(n => receivedAfterDispose.Add(n));

        // Act
        _sut.Info("Before dispose");
        _sut.Dispose();

        // Assert - No exception should be thrown, subscription should complete
        receivedAfterDispose.Count.ShouldBe(1);
        receivedAfterDispose[0].Message.ShouldBe("Before dispose");
    }

    [Theory]
    [InlineData(NotifySeverity.Info)]
    [InlineData(NotifySeverity.Success)]
    [InlineData(NotifySeverity.Warning)]
    [InlineData(NotifySeverity.Error)]
    [InlineData(NotifySeverity.Normal)]
    public void AllSeverityTypes_CanBePublished(NotifySeverity severity)
    {
        // Arrange
        var payload = new NotificationPayload(severity, "Test message", null, NotifyBehavior.ToastOnly);

        // Act
        _sut.Publish(payload);

        // Assert
        _receivedNotifications.Count.ShouldBe(1);
        _receivedNotifications[0].Severity.ShouldBe(severity);
    }

    [Theory]
    [InlineData(NotifyBehavior.ToastOnly)]
    [InlineData(NotifyBehavior.AlertOnly)]
    [InlineData(NotifyBehavior.AlertAndToast)]
    [InlineData(NotifyBehavior.None)]
    public void AllBehaviorTypes_CanBePublished(NotifyBehavior behavior)
    {
        // Arrange
        var payload = new NotificationPayload(NotifySeverity.Info, "Test message", null, behavior);

        // Act
        _sut.Publish(payload);

        // Assert
        _receivedNotifications.Count.ShouldBe(1);
        _receivedNotifications[0].Behavior.ShouldBe(behavior);
    }

    public void Dispose()
    {
        _subscription?.Dispose();
        _sut?.Dispose();
    }
}
