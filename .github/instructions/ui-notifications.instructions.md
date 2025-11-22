# NEBA UI Notifications — Implementation Instructions

## Goals (explicit)

1. Provide a robust, consistent notification system with five tiers: `Normal`, `Info`, `Success`, `Warning`, `Error`.
2. Provide two delivery mechanisms: **Alert** (inline, persistent) and **Toast** (ephemeral, floating).
3. Default behavior for UI form-submission validation failure: **show both Toast + Alert**.
4. Provide a controlled override mechanism using a form wrapper `<NebaForm>` that passes a cascading `NotificationPolicy`.
5. Keep domain logic free of UI concerns: notifications are part of the application/UI layer and must be triggered by application/service results or domain events (via adapters), not domain entities.
6. Accessibility: ARIA roles, screen-reader announcements, keyboard focus where needed.

---

## High-Level Architecture

- **Domain Layer (DDD):** No UI references. Domain services emit result objects and domain events.
- **Application Layer:** Interprets domain results and maps them to `NotificationPayload`s when appropriate.
- **UI Layer (Blazor):** Contains components and the `NotificationService`. Receives `NotificationPayload`s and renders Alerts/Toasts.
- **Infrastructure Layer:** Optional: persistence or event bus for persisted notifications or cross-tab sync.

---

## Notification Model

### Enumerations

```csharp
public enum NotifySeverity { Normal, Info, Success, Warning, Error }
public enum NotifyBehavior { AlertOnly, ToastOnly, AlertAndToast, None }
```

### Notification Payload

```csharp
public record NotificationPayload(NotifySeverity Severity, string Title, string Message, bool Persist = false, string? Code = null, object? Metadata = null);
```

`Code` can be used to attach a stable machine-readable identifier (e.g. `ERR_VALIDATION_001`). `Persist` for Toast means keep until dismissed (rare).

---

## NotificationService Contract

```csharp
public interface INotificationService
{
    void Info(string message, string? title = null);
    void Success(string message, string? title = null);
    void Warning(string message, string? title = null);
    void Error(string message, string? title = null);
    void Normal(string message, string? title = null);

    // Form-specific
    void ValidationFailure(string message, NotifyBehavior? overrideBehavior = null);

    // Lowest level
    void Publish(NotificationPayload payload);

    // For UI to observe
    IObservable<NotificationPayload> Notifications { get; }
}
```

**Implementation notes:**
- Implementation should use a small in-memory broker with `Subject<NotificationPayload>` (or equivalent) to allow components to subscribe.
- In Blazor Server/wasm, choose the correct concurrent primitives (e.g., `System.Reactive` or `EventHandler` + thread-safe queue).

---

## Form Wrapper: `<NebaForm>` (Cascading Parameter)

**Responsibilities:**
- Run UI field validation (if using `EditForm` / `FluentValidation` / manual validation hooks) when submit is invoked.
- If field validation fails on submit, determine behavior via cascading `NotificationPolicy`.
- Trigger notifications via `INotificationService.ValidationFailure(...)`.
- If field validation passes, call provided submit handler; handle result mapping to notifications.

**API (usage)**

```razor
<NebaForm NotificationBehavior="NotifyBehavior.AlertAndToast" OnValidSubmit="HandleSubmit">
  <!-- Input fields -->
</NebaForm>
```

**Cascading value**
- The form wrapper provides `NotificationPolicy` as `CascadingValue` to children.

**Default policy:** `NotifyBehavior.AlertAndToast`.

---

## UI Components

### `NebaAlert.razor` (inline alert)

**Props:**
- `NotifySeverity Severity { get; set; }`
- `string Message { get; set; }`
- `string? Title { get; set; }`
- `EventCallback OnDismiss { get; set; }` (optional)

**Behavior:**
- Render a visually distinct block within page flow.
- Use semantic HTML: `<div role="alert">` for errors/warnings, `<div role="status">` for info/success.
- Include an accessible dismiss button when dismissible.

**Tailwind mapping (examples):**
- Error: `bg-red-50 border-red-400 text-red-800` + icon
- Warning: `bg-yellow-50 border-yellow-400 text-yellow-800`
- Success: `bg-green-50 border-green-400 text-green-800`
- Info: `bg-blue-50 border-blue-400 text-blue-800`
- Normal: `bg-gray-50 border-gray-300 text-gray-800`

### `NebaToast.razor` (toast popup)

**Props:**
- `NotifySeverity Severity`
- `string Message` + `string? Title`
- `TimeSpan Duration` (default 4s)
- `bool PauseOnHover` (true)
- `bool Dismissible` (true)

**Behavior:**
- Toasts appear stacked top-right (or top-center if preferred); newest on top.
- Each toast auto-dismisses after `Duration` unless `Persist=true`.
- Provide direct focus on toast when it appears for screen readers (use `aria-live="polite"` or `assertive` depending on severity).
- Allow manual keyboard dismissal (Esc) and click dismissal.

**Stack manager:** a `ToastManager` component must render the active toasts from `INotificationService.Notifications` and handle lifecycle.

---

## Accessibility

- Use `role="alert"` for errors; `role="status"` for info/success. For toasts, use `aria-live="polite"` for mild messages and `aria-live="assertive"` for Errors.
- Ensure toasts are reachable via keyboard (focusable) and dismissable via `Esc`.
- Screen-reader announcement: when toast appears, set focus or use an offscreen `aria-live` container to announce.

---

## DI and Registration (Minimal)

```csharp
// Startup / Program.cs
builder.Services.AddScoped<INotificationService, NotificationService>();
```

**Notes:** Scoped is appropriate for Blazor Server circuits, ensuring each user session has its own notification state.

---

## Toast Placement & Behavior Specification

The NEBA application uses a unified toast notification system with responsive placement that adapts to screen size for optimal visibility and ergonomics.

### Toast Placement Rules

Toast placement is **automatically responsive** and handled via CSS media queries—no configuration needed.

#### Desktop Placement (≥ 768px viewport)
All toasts—regardless of severity—are placed in the **top-right** corner.

**Rationale:**
- Matches enterprise UX conventions (AWS, Azure, Jira, GitHub, Slack)
- Avoids covering main forms, inputs, or actions
- Minimal intrusion, visually predictable
- Consistent with NEBA's design system

#### Mobile Placement (< 768px viewport)
All toasts are placed at **bottom-center** of the viewport.

**Rationale:**
- Easier thumb access for dismissal
- Avoids covering page headers and navigation
- Stays above mobile OS chrome (home indicator, keyboard)
- Better ergonomics for touch interaction

**CSS Implementation:**
```css
/* Mobile: bottom-center */
.neba-toast-container {
  position: fixed;
  bottom: 1rem;
  left: 50%;
  transform: translateX(-50%);
}

/* Desktop: top-right */
@media (min-width: 768px) {
  .neba-toast-container {
    top: 1rem;
    right: 1rem;
    bottom: auto;
    left: auto;
    transform: none;
  }
}
```

### Toast Stacking & Display

- **Multiple toasts stack vertically** (newest on top)
- Maximum of **5 concurrent toasts** (configurable via `ToastManager.MaxToasts`)
- Older toasts removed via FIFO when limit exceeded
- Each toast animates independently

### Duration Rules

- **Default duration: 4 seconds**
- All toasts auto-dismiss unless `Persist=true` in payload
- Timer pauses on hover (desktop)
- Timer pauses on touch-and-hold (mobile)

### Interaction Requirements

- **Tap/click to dismiss** via X button
- **Swipe gestures** supported on touch devices (via hover pause)
- Interactions outside toast pass through to UI
- Toasts do not block scrolling or other gestures

### Accessibility

- **`aria-live="polite"`** for Success/Info/Normal
- **`aria-live="assertive"`** for Error/Warning
- Must meet WCAG AA contrast ratios
- Non-modal behavior (no focus trap)

### Visual Behavior

- **Fade-in animation:** 0.3s with scale effect
- **Fade-out animation:** 0.2s on dismiss
- Respects `prefers-reduced-motion` user preference
- Box shadow for depth without modal heaviness
- Rounded corners: `--neba-radius-lg` (0.5rem)
- Desktop width: 300-500px
- Mobile width: responsive (min 300px)

### Mobile-Specific Adjustments

- Bottom-center placement avoids keyboard obstruction
- Safe-area padding respected
- Toasts elevate above OS chrome automatically

## Tailwind Design Tokens (suggested)

Create CSS utility classes or a `neba-theme` partial to centralize color variables. Use `xl` rounded corners for alerts/cards. Keep icons consistent.

Example mapping (already given above) should be implemented via Tailwind classes.

---

## Error Handling Semantics (mapping from App -> UI)

- **Client-side field validation (UI):** field indicators (red border) + on submit: `ValidationFailure` → default: Alert + Toast.
- **Application-level domain violations (e.g., business rule):** map to `NotifySeverity.Warning` or `Error` depending on rule and deliver via Alert (if correction required) and optional Toast.
- **Infrastructure failures (e.g., network/persistence):** `NotifySeverity.Error` → Toast + possible Alert if user can act.

Always include a machine-readable `Code` in payloads for analytics and deterministic handling.

---

## Behavior Examples (Minimal Code Snippets)

### `NotifyBehavior` enum

```csharp
public enum NotifyBehavior { AlertOnly, ToastOnly, AlertAndToast, None }
```

### Validation failure helper

```csharp
public class FormNotificationHelper
{
    private readonly INotificationService _notificationService;
    public FormNotificationHelper(INotificationService s) => _notificationService = s;

    public void OnValidationFailure(NotifyBehavior behavior, string message = "Please fix the errors highlighted below.")
    {
        if (behavior == NotifyBehavior.AlertOnly || behavior == NotifyBehavior.AlertAndToast)
            _notificationService.Publish(new NotificationPayload(NotifySeverity.Error, "Submission failed", message));

        if (behavior == NotifyBehavior.ToastOnly || behavior == NotifyBehavior.AlertAndToast)
            _notificationService.Error(message);
    }
}
```

---

## Unit Testing Guidance (xUnit + Moq + Shouldly)

- Tests should validate `NotificationService` publishes correct payloads for each public method.
- Tests should validate `NebaForm` respects `NotificationBehavior` cascading policy.

**Example test names** (follow your naming convention):

```
NotificationService_Publish_ShouldEmitPayload_WhenCalled
FormNotificationHelper_ShouldPublishAlertAndToast_WhenBehaviorIsAlertAndToast
NebaForm_ShouldUseGlobalDefault_WhenNoOverrideProvided
NebaForm_ShouldUseOverride_WhenNotificationBehaviorProvided
ToastManager_ShouldAutoDismiss_ToastsAfterDuration
NebaAlert_ShouldRenderCorrectClasses_ForEachSeverity
```

**Minimal xUnit example skeleton**

```csharp
public class NotificationServiceTests
{
    [Fact]
    public void Publish_ShouldEmitPayload_WhenCalled()
    {
        // Arrange
        var sut = new NotificationService();
        var received = new List<NotificationPayload>();
        using var sub = sut.Notifications.Subscribe(p => received.Add(p));

        // Act
        sut.Publish(new NotificationPayload(NotifySeverity.Info, "Title", "Message"));

        // Assert
        received.Count.ShouldBe(1);
        received[0].Severity.ShouldBe(NotifySeverity.Info);
    }
}
```

---

## Copilot Prompt (single, full specification)

Use this prompt verbatim with Copilot in the project repository to generate the implementation. It contains the info needed to build components, services, DI, and tests.

```
Implement a UI notification system for a Blazor + Tailwind project following Clean Architecture and DDD.

Requirements:
- Provide five severity tiers: Normal, Info, Success, Warning, Error.
- Provide two delivery mechanisms: Alert (inline persistent) and Toast (ephemeral popup).
- Default behavior for form submit UI validation failure: show both Toast and Alert.
- Use a form wrapper `<NebaForm NotificationBehavior="...">` that provides a cascading NotificationPolicy to children. Default policy is AlertAndToast.
- Provide `INotificationService` with methods Info/Success/Warning/Error/Normal/Publish and an observable `Notifications` stream.
- Implement `NebaAlert.razor`, `NebaToast.razor`, and a `ToastManager` to render active toasts.
- Implement `NotificationService` (in-memory publish-subscribe), DI registration, and test coverage using xUnit, Moq, and Shouldly.
- Include accessible ARIA roles and keyboard interactions for toasts (Esc to dismiss) and focus management.
- Expose tailwind class mappings for each severity and a small theme file.

Produce:
1. Component files (razor + minimal code-behind) for NebaAlert, NebaToast, NebaForm, ToastManager.
2. Interfaces and concrete NotificationService with thread-safe publish-subscribe semantics.
3. DI registration snippet for Program.cs.
4. 4–6 unit tests with xUnit + Moq + Shouldly (test names must use Method_ShouldExpectedResult_WhenCondition format).
5. A short README section describing usage and example markup.
```

---

## Deliverables

- `ui-notifications.instructions.md` (this file)
- After implementation, expect files:
  - `UI/Components/NebaAlert.razor`
  - `UI/Components/NebaToast.razor`
  - `UI/Components/ToastManager.razor`
  - `UI/Forms/NebaForm.razor`
  - `Application/Services/NotificationService.cs`
  - `Tests/NotificationServiceTests.cs`, `Tests/NebaFormTests.cs`
  - `wwwroot/css/neba_theme.css` or Tailwind partials

---

## Notes & Rationale (short)

- Defaulting to both toast + alert for failed form submissions is an enterprise UX best practice: toast ensures immediate visibility; alert ensures persistent instructional guidance.
- Cascading `NotificationPolicy` via a single `NebaForm` preserves developer ergonomics while keeping a single enforcement point.
- Keeping notifications in the application/UI layer avoids domain pollution and preserves testability.

---

_End of specification._

