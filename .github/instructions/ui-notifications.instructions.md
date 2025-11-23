# NEBA UI Notifications — Specification

> Authoritative, conflict-free notification guidelines for Alerts and Toasts in the NEBA UI. This replaces all prior versions.

---

## 1. Purpose & Goals

The NEBA notification system must:

1. Provide five severity tiers: **Normal**, **Info**, **Success**, **Warning**, **Error**.
2. Provide two delivery mechanisms:
   - **Alert** (inline, persistent, contextual)
   - **Toast** (ephemeral, floating)
3. Ensure **persistent, actionable issues** use Alerts.
4. Ensure **instant, ephemeral feedback** uses Toasts.
5. Enforce consistent rules for placement, behavior, accessibility, and severity.
6. Keep domain/UI concerns separated (DDD): notifications originate from the application layer, never the domain.
7. Enable `<NebaForm>` to regulate validation-driven notifications.

---

## 2. System Architecture

### **Domain Layer (DDD)**

- Emits domain events and result objects.
- Contains **no** UI notification logic.

### **Application Layer**

- Maps domain outcomes → `NotificationPayload` objects.
- Determines severity and behavior based on context.

### **UI Layer (Blazor)**

- Renders Alerts and Toasts.
- Hosts `NotificationService`, `ToastManager`, `NebaAlert`, `NebaToast`, `NebaForm`.

### **Infrastructure Layer (Optional)**

- Supports persisted notifications or cross-tab/state sync.

---

## 3. Notification Model

### **Severity Levels**

```console
Normal < Info < Success < Warning < Error
```

Higher severity always overrides lower when conflicts arise.

### **Behavior Modes**

```console
AlertOnly
ToastOnly
AlertAndToast
None
```

### **Payload**

```csharp
NotificationPayload(
    Severity,
    Title,
    Message,
    Persist = false,
    Code = null,
    Metadata = null
)
```

- `Code` is a machine-stable identifier.
- `Persist` applies only to Toasts (rare).

---

## 4. NotificationService Contract

The service publishes notifications and exposes an observable stream.

### **Interface**

```csharp
void Info(...)
void Success(...)
void Warning(...)
void Error(...)
void Normal(...)
void ValidationFailure(...)
void Publish(NotificationPayload payload)
IObservable<NotificationPayload> Notifications
```

- Implementation uses an in-memory broker.
- Scoped lifetime per Blazor circuit.

---

## 5. The Notification Decision Framework

### **Alerts** (inline, persistent)

Use Alerts when:

- The user must **take action** to fix something.
- The issue persists until corrected.
- Additional context is required.
- Form-level validation fails.
- A business rule fails.
- A domain/application error needs user guidance.

### **Toasts** (ephemeral, floating)

Use Toasts when:

- Feedback is instant and does not require user action.
- A success confirmation is needed.
- Minor info messages.
- Non-blocking warnings.
- Application-level events occur.

### **Combined Alert + Toast**

Use both when:

- Validation fails on form submission.
- You must signal *immediate failure* AND provide *persistent guidance*.

---

## 6. Alert Specification

Alerts are the persistent, actionable, inline messaging system.

### **Alert Placement**

- Alerts appear **at the top of the page section or form they relate to**.
- Alerts are part of normal layout flow.
- Only **one alert** may be visible at a time per region.

### **Alert Severity Precedence**

If multiple alert conditions exist:

- **Highest severity wins**.
- Lower severities must be grouped **inside** the alert body.

### **Alert Grouping for Validation**

Validation errors must be shown as:

```console
Please fix the following:
• Field A is required
• Field B must be numeric
• Field C must be in the future
```

Not as multiple alerts.

### **Alert Persistence Rules**

- **Error/Warning:** persist until dismissed or corrected.
- **Success:** automatically dismissed on navigation.
- **Info/Normal:** may be manual or auto-dismiss depending on context.

### **Accessibility**

- Errors/Warnings: `role="alert"`
- Info/Success: `role="status"`
- Alerts must be dismissible when appropriate.
- Alerts must scroll into view when triggered.

---

## 7. Toast Specification

Toasts are ephemeral, floating notifications.

### 7.1 Toast Placement Rules

### **Desktop (≥ 768px)**

**All toasts** appear in the **top-right** corner.

### **Mobile (< 768px)**

- **Error/Warning → Top-center**
- **Success/Info/Normal → Bottom-center**

### **Safe Area Compliance**

- Must avoid notches, home indicator, OS chrome, keyboard.
- Must reposition automatically.

---

### 7.2 Toast Behavior Rules

### **Single Active Toast (Default)**

- Only **one toast** may be visible at a time.
- New toasts **replace** the current toast.
- Exception: background jobs may opt-in to multi-toast mode.

### **Duration**

- Standard: **2.5–3 seconds**
- Error/Warning: **3.5–4 seconds**
- Persisting toasts rare and require explicit `Persist=true`.

### **Interaction**

- Click/tap to dismiss.
- Swipe-to-dismiss on touch devices.
- ESC key to dismiss.
- Hover or touch-and-hold pauses timer.
- Interactions behind toast must pass through.

### **Accessibility**

- Mild → `aria-live="polite"`
- Errors → `aria-live="assertive"`
- Toasts must not trap focus.

### **Animation**

- Fade + slide.
- Respect `prefers-reduced-motion`.

---

## 8. NebaForm — Validation & Notification Policy

`<NebaForm>` coordinates validation and notification delivery.

### **Behavior**

- Runs all validation on submit.
- If validation fails → use cascading `NotificationBehavior`.
- Default: **AlertAndToast**.
- Calls `INotificationService.ValidationFailure()` to publish notifications.

### **Cascading Policy**

Developers may override:

```razor
<NebaForm NotificationBehavior="ToastOnly"> ...
```

---

## 9. Component Definitions

### NebaAlert.razor

- Inline block.
- Severity-mapped styling.
- Dismiss optional.
- Semantic roles based on severity.

### NebaToast.razor

- Floating ephemeral notification.
- Dismissible.
- Timed lifecycle.

### ToastManager

- Renders toast container with responsive placement.
- Manages replacement logic.
- Enforces single-toast rule unless overridden.

---

## 10. Error Handling Semantics

### **UI field validation (client-side)**

- Field highlights.
- On submit → **Alert + Toast**.

### **Business rule violation**

- Alert for correction.
- Optional Toast for immediate feedback.

### **Infrastructure errors**

- Error Toast + optional Alert.

---

## 11. Unit Testing Requirements

Test naming: `Method_ShouldExpectedResult_WhenCondition`

Required coverage:

- NotificationService publishes correct payloads.
- NebaForm respects cascading policies.
- ToastManager properly replaces toasts.
- Alert renders correct class mappings.
- ValidationFailure triggers expected behaviors.

---

## 12. Implementation Prompt (For Copilot)

> Implement NEBA's unified notification system using this specification. Include components, services, DI, tests, alert/toast rendering logic, CSS/Tailwind styling, and responsive/mobile behavior.

---

## End of Specification
