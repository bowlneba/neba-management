using Neba.Web.Server.Notifications;

namespace Neba.UnitTests.Ui.Server.Notifications;
[Trait("Category", "Unit")]
[Trait("Component", "Ui.Server.Notifications")]

public sealed class NotificationServiceTests
{
    [Fact(DisplayName = "Publishes notification with info severity")]
    public void Info_PublishesNotificationWithInfoSeverity()
    {
        // Arrange
        using var service = new NotificationService();
        NotificationPayload? receivedPayload = null;
        service.Notifications.Subscribe(payload => receivedPayload = payload);

        // Act
        service.Info("Test message");

        // Assert
        receivedPayload.ShouldNotBeNull();
        receivedPayload.Severity.ShouldBe(NotifySeverity.Info);
        receivedPayload.Message.ShouldBe("Test message");
        receivedPayload.Title.ShouldBeNull();
        receivedPayload.Behavior.ShouldBe(NotifyBehavior.ToastOnly);
    }

    [Fact(DisplayName = "Publishes info notification with title and behavior")]
    public void Info_WithTitleAndBehavior_PublishesNotificationWithAllProperties()
    {
        // Arrange
        using var service = new NotificationService();
        NotificationPayload? receivedPayload = null;
        service.Notifications.Subscribe(payload => receivedPayload = payload);

        // Act
        service.Info("Test message", "Test title", NotifyBehavior.AlertAndToast);

        // Assert
        receivedPayload.ShouldNotBeNull();
        receivedPayload.Severity.ShouldBe(NotifySeverity.Info);
        receivedPayload.Message.ShouldBe("Test message");
        receivedPayload.Title.ShouldBe("Test title");
        receivedPayload.Behavior.ShouldBe(NotifyBehavior.AlertAndToast);
    }

    [Fact(DisplayName = "Publishes notification with success severity")]
    public void Success_PublishesNotificationWithSuccessSeverity()
    {
        // Arrange
        using var service = new NotificationService();
        NotificationPayload? receivedPayload = null;
        service.Notifications.Subscribe(payload => receivedPayload = payload);

        // Act
        service.Success("Operation completed");

        // Assert
        receivedPayload.ShouldNotBeNull();
        receivedPayload.Severity.ShouldBe(NotifySeverity.Success);
        receivedPayload.Message.ShouldBe("Operation completed");
        receivedPayload.Title.ShouldBeNull();
        receivedPayload.Behavior.ShouldBe(NotifyBehavior.ToastOnly);
    }

    [Fact(DisplayName = "Publishes success notification with title and behavior")]
    public void Success_WithTitleAndBehavior_PublishesNotificationWithAllProperties()
    {
        // Arrange
        using var service = new NotificationService();
        NotificationPayload? receivedPayload = null;
        service.Notifications.Subscribe(payload => receivedPayload = payload);

        // Act
        service.Success("Item saved", "Success", NotifyBehavior.AlertOnly);

        // Assert
        receivedPayload.ShouldNotBeNull();
        receivedPayload.Severity.ShouldBe(NotifySeverity.Success);
        receivedPayload.Message.ShouldBe("Item saved");
        receivedPayload.Title.ShouldBe("Success");
        receivedPayload.Behavior.ShouldBe(NotifyBehavior.AlertOnly);
    }

    [Fact(DisplayName = "Publishes notification with warning severity")]
    public void Warning_PublishesNotificationWithWarningSeverity()
    {
        // Arrange
        using var service = new NotificationService();
        NotificationPayload? receivedPayload = null;
        service.Notifications.Subscribe(payload => receivedPayload = payload);

        // Act
        service.Warning("Check this out");

        // Assert
        receivedPayload.ShouldNotBeNull();
        receivedPayload.Severity.ShouldBe(NotifySeverity.Warning);
        receivedPayload.Message.ShouldBe("Check this out");
        receivedPayload.Title.ShouldBeNull();
        receivedPayload.Behavior.ShouldBe(NotifyBehavior.ToastOnly);
    }

    [Fact(DisplayName = "Publishes warning notification with title and behavior")]
    public void Warning_WithTitleAndBehavior_PublishesNotificationWithAllProperties()
    {
        // Arrange
        using var service = new NotificationService();
        NotificationPayload? receivedPayload = null;
        service.Notifications.Subscribe(payload => receivedPayload = payload);

        // Act
        service.Warning("Disk space low", "Warning", NotifyBehavior.AlertAndToast);

        // Assert
        receivedPayload.ShouldNotBeNull();
        receivedPayload.Severity.ShouldBe(NotifySeverity.Warning);
        receivedPayload.Message.ShouldBe("Disk space low");
        receivedPayload.Title.ShouldBe("Warning");
        receivedPayload.Behavior.ShouldBe(NotifyBehavior.AlertAndToast);
    }

    [Fact(DisplayName = "Publishes notification with error severity")]
    public void Error_PublishesNotificationWithErrorSeverity()
    {
        // Arrange
        using var service = new NotificationService();
        NotificationPayload? receivedPayload = null;
        service.Notifications.Subscribe(payload => receivedPayload = payload);

        // Act
        service.Error("Something went wrong");

        // Assert
        receivedPayload.ShouldNotBeNull();
        receivedPayload.Severity.ShouldBe(NotifySeverity.Error);
        receivedPayload.Message.ShouldBe("Something went wrong");
        receivedPayload.Title.ShouldBeNull();
        receivedPayload.Behavior.ShouldBe(NotifyBehavior.ToastOnly);
    }

    [Fact(DisplayName = "Publishes error notification with title and behavior")]
    public void Error_WithTitleAndBehavior_PublishesNotificationWithAllProperties()
    {
        // Arrange
        using var service = new NotificationService();
        NotificationPayload? receivedPayload = null;
        service.Notifications.Subscribe(payload => receivedPayload = payload);

        // Act
        service.Error("Connection failed", "Error", NotifyBehavior.None);

        // Assert
        receivedPayload.ShouldNotBeNull();
        receivedPayload.Severity.ShouldBe(NotifySeverity.Error);
        receivedPayload.Message.ShouldBe("Connection failed");
        receivedPayload.Title.ShouldBe("Error");
        receivedPayload.Behavior.ShouldBe(NotifyBehavior.None);
    }

    [Fact(DisplayName = "Publishes notification with normal severity")]
    public void Normal_PublishesNotificationWithNormalSeverity()
    {
        // Arrange
        using var service = new NotificationService();
        NotificationPayload? receivedPayload = null;
        service.Notifications.Subscribe(payload => receivedPayload = payload);

        // Act
        service.Normal("Just a regular message");

        // Assert
        receivedPayload.ShouldNotBeNull();
        receivedPayload.Severity.ShouldBe(NotifySeverity.Normal);
        receivedPayload.Message.ShouldBe("Just a regular message");
        receivedPayload.Title.ShouldBeNull();
        receivedPayload.Behavior.ShouldBe(NotifyBehavior.ToastOnly);
    }

    [Fact(DisplayName = "Publishes normal notification with title and behavior")]
    public void Normal_WithTitleAndBehavior_PublishesNotificationWithAllProperties()
    {
        // Arrange
        using var service = new NotificationService();
        NotificationPayload? receivedPayload = null;
        service.Notifications.Subscribe(payload => receivedPayload = payload);

        // Act
        service.Normal("Regular update", "Update", NotifyBehavior.AlertOnly);

        // Assert
        receivedPayload.ShouldNotBeNull();
        receivedPayload.Severity.ShouldBe(NotifySeverity.Normal);
        receivedPayload.Message.ShouldBe("Regular update");
        receivedPayload.Title.ShouldBe("Update");
        receivedPayload.Behavior.ShouldBe(NotifyBehavior.AlertOnly);
    }

    [Fact(DisplayName = "Publishes validation failure with default behavior")]
    public void ValidationFailure_PublishesNotificationWithDefaultBehavior()
    {
        // Arrange
        using var service = new NotificationService();
        NotificationPayload? receivedPayload = null;
        service.Notifications.Subscribe(payload => receivedPayload = payload);

        // Act
        service.ValidationFailure("Email is required");

        // Assert
        receivedPayload.ShouldNotBeNull();
        receivedPayload.Severity.ShouldBe(NotifySeverity.Error);
        receivedPayload.Message.ShouldBe("Email is required");
        receivedPayload.Title.ShouldBe("Validation Failed");
        receivedPayload.Behavior.ShouldBe(NotifyBehavior.AlertAndToast);
        receivedPayload.Persist.ShouldBe(false);
        receivedPayload.Code.ShouldBe("VALIDATION_FAILURE");
    }

    [Fact(DisplayName = "Publishes validation failure with custom behavior")]
    public void ValidationFailure_WithOverrideBehavior_UsesProvidedBehavior()
    {
        // Arrange
        using var service = new NotificationService();
        NotificationPayload? receivedPayload = null;
        service.Notifications.Subscribe(payload => receivedPayload = payload);

        // Act
        service.ValidationFailure("Password too short", NotifyBehavior.ToastOnly);

        // Assert
        receivedPayload.ShouldNotBeNull();
        receivedPayload.Severity.ShouldBe(NotifySeverity.Error);
        receivedPayload.Message.ShouldBe("Password too short");
        receivedPayload.Title.ShouldBe("Validation Failed");
        receivedPayload.Behavior.ShouldBe(NotifyBehavior.ToastOnly);
        receivedPayload.Code.ShouldBe("VALIDATION_FAILURE");
    }

    [Fact(DisplayName = "Publishes custom notification payload")]
    public void Publish_PublishesCustomPayload()
    {
        // Arrange
        using var service = new NotificationService();
        NotificationPayload? receivedPayload = null;
        service.Notifications.Subscribe(payload => receivedPayload = payload);

        var customPayload = new NotificationPayload(
            NotifySeverity.Warning,
            "Custom message",
            "Custom title",
            NotifyBehavior.AlertAndToast,
            Persist: true,
            Code: "CUSTOM_CODE",
            Metadata: new { Id = 123 }
        );

        // Act
        service.Publish(customPayload);

        // Assert
        receivedPayload.ShouldNotBeNull();
        receivedPayload.ShouldBe(customPayload);
        receivedPayload.Severity.ShouldBe(NotifySeverity.Warning);
        receivedPayload.Message.ShouldBe("Custom message");
        receivedPayload.Title.ShouldBe("Custom title");
        receivedPayload.Behavior.ShouldBe(NotifyBehavior.AlertAndToast);
        receivedPayload.Persist.ShouldBe(true);
        receivedPayload.Code.ShouldBe("CUSTOM_CODE");
        receivedPayload.Metadata.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Allows multiple subscribers to notification stream")]
    public void Notifications_AllowsMultipleSubscribers()
    {
        // Arrange
        using var service = new NotificationService();
        NotificationPayload? received1 = null;
        NotificationPayload? received2 = null;
        NotificationPayload? received3 = null;

        service.Notifications.Subscribe(payload => received1 = payload);
        service.Notifications.Subscribe(payload => received2 = payload);
        service.Notifications.Subscribe(payload => received3 = payload);

        // Act
        service.Info("Broadcast message");

        // Assert
        received1.ShouldNotBeNull();
        received2.ShouldNotBeNull();
        received3.ShouldNotBeNull();
        received1.Message.ShouldBe("Broadcast message");
        received2.Message.ShouldBe("Broadcast message");
        received3.Message.ShouldBe("Broadcast message");
    }

    [Fact(DisplayName = "Publishes multiple notifications in order")]
    public void Notifications_PublishesMultipleNotificationsInOrder()
    {
        // Arrange
        using var service = new NotificationService();
        var receivedPayloads = new List<NotificationPayload>();
        service.Notifications.Subscribe(payload => receivedPayloads.Add(payload));

        // Act
        service.Info("First");
        service.Success("Second");
        service.Warning("Third");
        service.Error("Fourth");
        service.Normal("Fifth");

        // Assert
        receivedPayloads.Count.ShouldBe(5);
        receivedPayloads[0].Message.ShouldBe("First");
        receivedPayloads[0].Severity.ShouldBe(NotifySeverity.Info);
        receivedPayloads[1].Message.ShouldBe("Second");
        receivedPayloads[1].Severity.ShouldBe(NotifySeverity.Success);
        receivedPayloads[2].Message.ShouldBe("Third");
        receivedPayloads[2].Severity.ShouldBe(NotifySeverity.Warning);
        receivedPayloads[3].Message.ShouldBe("Fourth");
        receivedPayloads[3].Severity.ShouldBe(NotifySeverity.Error);
        receivedPayloads[4].Message.ShouldBe("Fifth");
        receivedPayloads[4].Severity.ShouldBe(NotifySeverity.Normal);
    }

    [Fact(DisplayName = "Throws ObjectDisposedException when publishing after disposal")]
    public void Publish_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var service = new NotificationService();
        service.Dispose();

        var payload = new NotificationPayload(NotifySeverity.Info, "Test");

        // Act & Assert
        Should.Throw<ObjectDisposedException>(() => service.Publish(payload));
    }

    [Fact(DisplayName = "Completes notification stream on disposal")]
    public void Dispose_CompletesNotificationStream()
    {
        // Arrange
        var service = new NotificationService();
        bool completed = false;
        service.Notifications.Subscribe(
            onNext: _ => { },
            onCompleted: () => completed = true
        );

        // Act
        service.Dispose();

        // Assert
        completed.ShouldBe(true);
    }

    [Fact(DisplayName = "Multiple dispose calls do not throw")]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Arrange
        var service = new NotificationService();

        // Act & Assert
        service.Dispose();
        Should.NotThrow(() => service.Dispose());
        Should.NotThrow(() => service.Dispose());
    }

    [Fact(DisplayName = "Publishing before subscribers does not throw")]
    public void Notifications_BeforeAnySubscribers_DoesNotThrow()
    {
        // Arrange
        using var service = new NotificationService();

        // Act & Assert
        Should.NotThrow(() => service.Info("Test message"));
        Should.NotThrow(() => service.Success("Test message"));
        Should.NotThrow(() => service.Warning("Test message"));
        Should.NotThrow(() => service.Error("Test message"));
        Should.NotThrow(() => service.Normal("Test message"));
    }

    [Fact(DisplayName = "Unsubscribing stops receiving notifications")]
    public void Subscription_Unsubscribe_StopsReceivingNotifications()
    {
        // Arrange
        using var service = new NotificationService();
        int receivedCount = 0;
        IDisposable subscription = service.Notifications.Subscribe(_ => receivedCount++);

        // Act
        service.Info("First");
        subscription.Dispose();
        service.Info("Second");

        // Assert
        receivedCount.ShouldBe(1);
    }

    [Fact(DisplayName = "Publishes notifications with different behaviors correctly")]
    public void Notifications_WithDifferentBehaviors_PublishesCorrectly()
    {
        // Arrange
        using var service = new NotificationService();
        var receivedPayloads = new List<NotificationPayload>();
        service.Notifications.Subscribe(payload => receivedPayloads.Add(payload));

        // Act
        service.Info("AlertOnly", behavior: NotifyBehavior.AlertOnly);
        service.Success("ToastOnly", behavior: NotifyBehavior.ToastOnly);
        service.Warning("AlertAndToast", behavior: NotifyBehavior.AlertAndToast);
        service.Error("None", behavior: NotifyBehavior.None);

        // Assert
        receivedPayloads.Count.ShouldBe(4);
        receivedPayloads[0].Behavior.ShouldBe(NotifyBehavior.AlertOnly);
        receivedPayloads[1].Behavior.ShouldBe(NotifyBehavior.ToastOnly);
        receivedPayloads[2].Behavior.ShouldBe(NotifyBehavior.AlertAndToast);
        receivedPayloads[3].Behavior.ShouldBe(NotifyBehavior.None);
    }
}
