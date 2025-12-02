# NEBA Notification System

This document provides practical guidance for using the NEBA notification system in your Blazor components and services.

## Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [Notification Types](#notification-types)
- [Using Toasts](#using-toasts)
- [Using Alerts](#using-alerts)
- [Validation Notifications](#validation-notifications)
- [Best Practices](#best-practices)
- [API Reference](#api-reference)

---

## Overview

The NEBA notification system provides two complementary ways to communicate with users:

- **Toasts**: Ephemeral, floating notifications for instant feedback (e.g., "Saved successfully!")
- **Alerts**: Persistent, inline notifications for actionable issues (e.g., form validation errors)

Both mechanisms support five severity levels: **Normal**, **Info**, **Success**, **Warning**, and **Error**.

### When to Use Each

| Use Case | Mechanism | Example |
|----------|-----------|---------|
| Confirming a successful action | Toast | "League created successfully" |
| Quick informational message | Toast | "Changes auto-saved" |
| Form validation errors | Alert + Toast | "Please fix the following errors" |
| Persistent warnings | Alert | "Your session expires in 5 minutes" |
| Business rule violations | Alert | "Cannot delete league with active teams" |

---

## Quick Start

### 1. Setup (Already Done)

The notification system is already configured in [MainLayout.razor](Neba.Web.Server/Layout/MainLayout.razor). Both `ToastManager` and `AlertContainer` are included in the layout.

### 2. Inject the Services

In your Razor component or service, inject the notification service:

```razor
@inject INotificationService NotificationService
```

Or for direct alert control:

```razor
@inject AlertService AlertService
```

### 3. Show Your First Notification

```csharp
// Simple success toast
NotificationService.Success("League created successfully!");

// Error toast
NotificationService.Error("Failed to save changes");

// Alert that stays on screen
NotificationService.Warning(
    "Your session will expire soon",
    behavior: NotifyBehavior.AlertOnly
);
```

---

## Notification Types

### Severity Levels

```csharp
NotifySeverity.Normal   // Gray - neutral information
NotifySeverity.Info     // Blue - informational messages
NotifySeverity.Success  // Green - successful operations
NotifySeverity.Warning  // Orange - warnings that need attention
NotifySeverity.Error    // Red - errors requiring action
```

### Delivery Behaviors

```csharp
NotifyBehavior.ToastOnly       // Ephemeral popup (default)
NotifyBehavior.AlertOnly       // Persistent inline alert
NotifyBehavior.AlertAndToast   // Both (e.g., validation failures)
NotifyBehavior.None            // No notification
```

---

## Using Toasts

Toasts are perfect for quick, ephemeral feedback that doesn't require user action.

### Basic Toast Examples

```csharp
// Info toast (blue)
NotificationService.Info("League schedule updated");

// Success toast (green)
NotificationService.Success("Team roster saved");

// Warning toast (orange)
NotificationService.Warning("Connection unstable");

// Error toast (red)
NotificationService.Error("Failed to load data");

// Normal toast (gray)
NotificationService.Normal("Background task completed");
```

### Toasts with Titles

```csharp
NotificationService.Success(
    message: "Your changes have been saved",
    title: "Success"
);

NotificationService.Error(
    message: "Unable to connect to server",
    title: "Network Error"
);
```

### Toast Behavior

- **Duration**: Auto-dismisses after 3-4 seconds
- **Position**:
  - Desktop (≥768px): Top-right corner
  - Mobile (<768px): Top-center for errors/warnings, bottom-center for others
- **Interaction**: Click/tap or swipe to dismiss early
- **Queue**: New toasts replace the current one; extras are queued

---

## Using Alerts

Alerts are inline, persistent notifications that remain visible until dismissed or corrected.

### Using AlertService

```csharp
@inject AlertService AlertService

// Show error alert
AlertService.ShowError("Unable to process your request");

// Show warning with title
AlertService.ShowWarning(
    message: "This action cannot be undone",
    title: "Are you sure?"
);

// Show success alert
AlertService.ShowSuccess("League created successfully");

// Show info alert
AlertService.ShowInfo(
    message: "New features available in settings",
    title: "What's New"
);

// Clear the current alert
AlertService.Clear();
```

### Using NotificationService for Alerts

You can also use `INotificationService` with `AlertOnly` behavior:

```csharp
NotificationService.Error(
    message: "Payment method expired",
    title: "Action Required",
    behavior: NotifyBehavior.AlertOnly
);
```

### Alert Configuration

Customize alert appearance and behavior:

```csharp
AlertService.ShowError(
    message: "Invalid input detected",
    title: "Validation Error",
    configure: options =>
    {
        options.Dismissible = true;
        options.ShowIcon = true;
        options.Variant = AlertVariant.Filled;
    }
);
```

### Alert Placement

Add the `<AlertContainer>` component where you want alerts to appear:

```razor
<AlertContainer DefaultDismissible="true"
                DefaultShowIcon="true"
                DefaultVariant="AlertVariant.Filled" />
```

Alerts automatically clear on navigation to a new page.

---

## Validation Notifications

### Form Validation with Both Alert and Toast

The default for validation failures is to show both an alert and a toast:

```csharp
NotificationService.ValidationFailure(
    "Please correct the errors before submitting"
);
```

### Multiple Validation Errors

For multiple validation messages, use `AlertService.ShowValidation()`:

```csharp
var validationErrors = new[]
{
    "League name is required",
    "Start date must be in the future",
    "Minimum 4 teams required"
};

AlertService.ShowValidation(
    messages: validationErrors,
    title: "Please fix the following:"
);
```

This renders as:

```console
Please fix the following:
• League name is required
• Start date must be in the future
• Minimum 4 teams required
```

### Override Validation Behavior

```csharp
// Show validation error only as alert (no toast)
NotificationService.ValidationFailure(
    "Form contains errors",
    overrideBehavior: NotifyBehavior.AlertOnly
);
```

---

## Best Practices

### 1. Choose the Right Mechanism

```csharp
// ✅ Good: Toast for success confirmation
NotificationService.Success("Saved!");

// ❌ Bad: Alert for success (too persistent)
NotificationService.Success("Saved!", behavior: NotifyBehavior.AlertOnly);

// ✅ Good: Alert for validation errors
AlertService.ShowValidation(errors);

// ❌ Bad: Toast-only for validation (not persistent enough)
NotificationService.Error("Fix errors"); // Missing context
```

### 2. Provide Clear, Actionable Messages

```csharp
// ✅ Good: Specific and actionable
NotificationService.Error(
    "Unable to delete league 'Summer 2024' because it has active teams",
    title: "Delete Failed"
);

// ❌ Bad: Vague and unhelpful
NotificationService.Error("An error occurred");
```

### 3. Use Appropriate Severity

```csharp
// ✅ Good: Warning for non-critical issues
NotificationService.Warning("Some statistics may be delayed");

// ❌ Bad: Error for non-critical issues
NotificationService.Error("Statistics delayed"); // Too severe
```

### 4. Avoid Notification Spam

```csharp
// ✅ Good: Single notification for batch operation
NotificationService.Success($"{count} teams imported successfully");

// ❌ Bad: Notification per item in a loop
foreach (var team in teams)
{
    NotificationService.Success($"{team.Name} imported"); // Spammy!
}
```

### 5. Combine When Appropriate

```csharp
// ✅ Good: Alert + Toast for validation
NotificationService.ValidationFailure(
    "Please correct the highlighted fields"
);
// User gets immediate toast feedback + persistent alert for reference
```

---

## API Reference

### INotificationService

Located in [Services/INotificationService.cs](Neba.Web.Server/Services/INotificationService.cs)

```csharp
void Info(string message, string? title = null, NotifyBehavior behavior = NotifyBehavior.ToastOnly);
void Success(string message, string? title = null, NotifyBehavior behavior = NotifyBehavior.ToastOnly);
void Warning(string message, string? title = null, NotifyBehavior behavior = NotifyBehavior.ToastOnly);
void Error(string message, string? title = null, NotifyBehavior behavior = NotifyBehavior.ToastOnly);
void Normal(string message, string? title = null, NotifyBehavior behavior = NotifyBehavior.ToastOnly);
void ValidationFailure(string message, NotifyBehavior? overrideBehavior = null);
void Publish(NotificationPayload payload);
IObservable<NotificationPayload> Notifications { get; }
```

### AlertService

Located in [Services/AlertService.cs](Neba.Web.Server/Services/AlertService.cs)

```csharp
void ShowNormal(string message, string? title = null, Action<AlertOptions>? configure = null);
void ShowInfo(string message, string? title = null, Action<AlertOptions>? configure = null);
void ShowSuccess(string message, string? title = null, Action<AlertOptions>? configure = null);
void ShowWarning(string message, string? title = null, Action<AlertOptions>? configure = null);
void ShowError(string message, string? title = null, Action<AlertOptions>? configure = null);
void ShowValidation(string[] messages, string? title = "Please fix the following:");
void Clear();
```

### NotificationPayload

Located in [Services/NotificationModels.cs](Neba.Web.Server/Services/NotificationModels.cs)

```csharp
public record NotificationPayload(
    NotifySeverity Severity,
    string Message,
    string? Title = null,
    NotifyBehavior Behavior = NotifyBehavior.ToastOnly,
    bool Persist = false,
    string? Code = null,
    object? Metadata = null
);
```

### AlertOptions

```csharp
public class AlertOptions
{
    public AlertVariant Variant { get; set; } = AlertVariant.Filled;
    public bool ShowIcon { get; set; } = true;
    public bool Dismissible { get; set; } = true;
    public Func<Task>? OnCloseIconClicked { get; set; }
}
```

### AlertVariant

```csharp
public enum AlertVariant
{
    Filled,    // Solid background
    Outlined,  // Border with transparent background
    Subtle     // Light background
}
```

---

## Components

### ToastManager

Located in [Notifications/ToastManager.razor](Neba.Web.Server/Notifications/ToastManager.razor)

Automatically included in `MainLayout.razor`. Manages toast display, queuing, and auto-dismiss behavior.

**Parameters:**

- `PauseOnHover` (bool): Whether to pause auto-dismiss timer on hover (default: true)

### AlertContainer

Located in [Notifications/AlertContainer.razor](Neba.Web.Server/Notifications/AlertContainer.razor)

Subscribe to `AlertService` and display persistent alerts.

**Parameters:**

- `DefaultVariant` (AlertVariant): Default visual style (default: Filled)
- `DefaultShowIcon` (bool): Whether to show severity icons (default: true)
- `DefaultDismissible` (bool): Whether alerts can be dismissed (default: true)

**Behavior:**

- Automatically clears alerts on navigation
- Only one alert visible at a time

### NebaAlert

Located in [Notifications/NebaAlert.razor](Neba.Web.Server/Notifications/NebaAlert.razor)

The underlying alert component. Use `AlertService` instead of using this directly.

### NebaToast

Located in [Notifications/NebaToast.razor](Neba.Web.Server/Notifications/NebaToast.razor)

The underlying toast component. Use `NotificationService` instead of using this directly.

---

## Examples

### Example 1: Save Operation

```csharp
try
{
    await LeagueService.SaveAsync(league);
    NotificationService.Success("League saved successfully");
}
catch (Exception ex)
{
    NotificationService.Error(
        $"Failed to save league: {ex.Message}",
        title: "Save Failed"
    );
}
```

### Example 2: Form Validation

```csharp
private async Task HandleValidSubmit()
{
    var errors = ValidateForm();

    if (errors.Any())
    {
        AlertService.ShowValidation(errors.ToArray());
        NotificationService.Error("Please fix validation errors");
        return;
    }

    await SubmitForm();
    NotificationService.Success("Form submitted successfully");
}
```

### Example 3: Background Task

```csharp
NotificationService.Info("Import started in background");

await Task.Run(async () =>
{
    var result = await ImportService.ImportTeamsAsync(file);

    if (result.Success)
    {
        NotificationService.Success(
            $"Successfully imported {result.Count} teams",
            title: "Import Complete"
        );
    }
    else
    {
        NotificationService.Error(
            result.ErrorMessage,
            title: "Import Failed",
            behavior: NotifyBehavior.AlertAndToast
        );
    }
});
```

### Example 4: Persistent Warning

```csharp
protected override void OnInitialized()
{
    if (SessionService.ExpiresInMinutes < 5)
    {
        AlertService.ShowWarning(
            message: "Your session will expire soon. Save your work.",
            title: "Session Expiring",
            configure: opts => opts.Dismissible = false
        );
    }
}
```

---

## Architecture Notes

- **Service Lifetime**: Both `INotificationService` and `AlertService` are scoped per Blazor circuit
- **Reactive Pattern**: `INotificationService` uses System.Reactive (`IObservable<T>`) for pub/sub
- **Component Communication**: Components subscribe to the observable stream and update UI reactively
- **Thread Safety**: Safe for use in Blazor Server's single-threaded circuit context

---

## Related Files

- **Specification**: [.github/instructions/ui-notifications.instructions.md](../../.github/instructions/ui-notifications.instructions.md)
- **Services**: [Neba.Web.Server/Services/](Neba.Web.Server/Services/)
- **Components**: [Neba.Web.Server/Notifications/](Neba.Web.Server/Notifications/)
- **Styling**: [Neba.Web.Server/wwwroot/neba_theme.css](Neba.Web.Server/wwwroot/neba_theme.css)

---

## Need Help?

If you encounter issues or need clarification:

1. Check the [specification document](../../.github/instructions/ui-notifications.instructions.md) for design decisions
2. Review component implementations in [Notifications/](Neba.Web.Server/Notifications/)
3. Examine service implementations in [Services/](Neba.Web.Server/Services/)
