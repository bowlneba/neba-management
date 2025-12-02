using Microsoft.AspNetCore.Components;
using Neba.Web.Server.Notifications;
using Neba.Web.Server.Services;

namespace Neba.Web.Server.Pages.TestHarness;

/// <summary>
/// Test harness page for triggering various notification types for Playwright testing.
/// Only accessible in Development or Test environments.
/// </summary>
public partial class TestNotificationsPage : ComponentBase
{
    [Inject]
    private INotificationService NotificationService { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment HostEnv { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private bool IsTestEnvironment =>
        HostEnv.EnvironmentName == "Development" ||
        HostEnv.EnvironmentName == "Test";

    private void TriggerErrorToast()
    {
        NotificationService.Error("This is a test error toast message");
    }

    private void TriggerWarningToast()
    {
        NotificationService.Warning("This is a test warning toast message");
    }

    private void TriggerSuccessToast()
    {
        NotificationService.Success("This is a test success toast message");
    }

    private void TriggerInfoToast()
    {
        NotificationService.Info("This is a test info toast message");
    }

    private void TriggerNormalToast()
    {
        NotificationService.Normal("This is a test normal toast message");
    }

    private void TriggerErrorAlert()
    {
        NotificationService.Error("This is a test error alert", "Error", NotifyBehavior.AlertOnly);
    }

    private void TriggerWarningAlert()
    {
        NotificationService.Warning("This is a test warning alert", "Warning", NotifyBehavior.AlertOnly);
    }

    private void TriggerSuccessAlert()
    {
        NotificationService.Success("This is a test success alert", "Success", NotifyBehavior.AlertOnly);
    }

    private void TriggerInfoAlert()
    {
        NotificationService.Info("This is a test info alert", "Information", NotifyBehavior.AlertOnly);
    }

    private void TriggerNormalAlert()
    {
        NotificationService.Normal("This is a test normal alert", "Notice", NotifyBehavior.AlertOnly);
    }

    private void TriggerValidationFailure()
    {
        NotificationService.ValidationFailure("Email is required. Password must be at least 8 characters.");
    }

    private void TriggerCustomAlertAndToast()
    {
        NotificationService.Warning("This message appears as both an alert and a toast", "Combined Notification", NotifyBehavior.AlertAndToast);
    }
}
