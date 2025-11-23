# NEBA UI Loading Patterns — Implementation Instructions

## Purpose
Loading is a distinct user experience concern separate from notifications. It communicates system status, prevents unintended double actions, and guides user expectations during asynchronous operations.

The NEBA UI will support **three separate loading patterns**, each with specific use cases and design rules:
1. Inline Loading
2. Button-Level Loading
3. Full-Page Blocking Overlay

These patterns are deliberately separated to avoid UX overlap and ensure consistent behavior across the application.

---

# 1. Inline Loading
Inline loading is used for **page content updates** and asynchronous **data-fetching**.

### Examples
- Loading tournament details
- Loading a list of events or entries
- Fetching filters, standings, or brackets

### Characteristics
- Light visual footprint
- Does not block interaction
- Paired with contextual text (e.g., "Loading...")
- Accessibility: spinner must have `aria-label="Loading"`

### Spinner Class Requirements
A default `.neba-spinner` exists and should be used for inline loading.

---

# 2. Button-Level Loading
Button-level loading provides immediate feedback for **user-triggered actions** and prevents accidental repeated input.

### Required Behavior
- Button disables itself on click
- Replaces button content with a spinner + text (e.g., "Saving...")
- Restores to original content on completion
- Prevents double submission

### When to Use
- Save actions
- Submitting forms
- Deleting items
- Adding or updating entities
- Short-running operations that affect only one UI region

### Spinner Variant
A small spinner class (e.g., `.neba-spinner-sm`) should be used to avoid disrupting button layout.

### Accessibility
- Spinner must have accessible text (visually hidden if needed)
- Disabled state must be communicated via ARIA attributes

---

# 3. Full-Page Blocking Overlay
The blocking overlay is for operations where the user **must not** interact with the UI until the operation is finished.

### When to Use
- Payment processing
- Finalizing tournament operations
- Running scoring or calculation routines
- Bulk updates
- Multi-step transactional operations
- Any operation where changing state midway would corrupt data

### Characteristics
- Covers entire viewport
- Disables all input
- Displays large spinner and an action label (e.g., "Saving...", "Processing...")
- Prevents scrolling
- Should use a semi-transparent background with subtle blur
- Must be dismissible only when the operation ends

### Accessibility
- Must announce itself as a busy state
- Prevent focus movement outside overlay
- Should optionally trap keyboard navigation

---

# 4. Choosing the Correct Loading Pattern
The following matrix determines which loading pattern should be used based on action type:

| Scenario | Inline | Button-Level | Overlay |
|----------|--------|--------------|----------|
| Loading page data | ✔️ | | |
| Loading tournament entries | ✔️ | | |
| Submitting a form | | ✔️ | |
| Saving settings | | ✔️ | |
| Processing payment | | | ✔️ |
| Finalizing tournament | | ✔️ (initial) | ✔️ (long-running) |
| Performing bulk updates | | | ✔️ |
| Minor action (enable/disable toggle) | | ✔️ | |

### Guiding Principle
- **Inline** is for *data-fetching*.
- **Button-level** is for *user actions*.
- **Overlay** is for *critical, blocking operations*.

---

# 5. Interaction With Notification System
Loading patterns are not notifications, but they **pair with notifications**.

### Correct Patterns
- On success → close overlay/spinner → show **Success Toast**
- On warning → close overlay/spinner → show **Warning Alert**
- On error → close overlay/spinner → show **Error Toast + possibly Alert**

### Never combine loading with error notifications simultaneously.
Allow the loading state to complete before showing any notification.

---

# 6. Configurability
Loading patterns should be **consistent**, but some configurability is acceptable.

### Allowed
- Global configuration for enabling/disabling blocking overlays
- Optional per-layout override for blocking overlay behavior
- Theme-level control of spinner sizes/colors

### Not Allowed
- Per-component or per-event overrides that change the loading pattern
- Arbitrary developer choice that leads to inconsistent UX

### Example Global Preferences
```csharp
public class UiPreferences
{
    public bool UseBlockingOverlay { get; set; } = true;
    public TimeSpan ButtonSpinnerDelay { get; set; } = TimeSpan.Zero; // optional
}
```

---

# 7. UI Consistency Rules
1. **Every user-triggered asynchronous operation must show a loading state.**
2. **Double-submit must always be prevented.**
3. **Blocking overlay must be used sparingly and only for critical operations.**
4. **Inline spinners must be accompanied by text for accessibility.**
5. **Loading must never be silent** — users must see system activity.
6. **Never stack overlays.** Only one overlay may be active.

---

# 8. Future Enhancements (Optional)
- Skeleton loaders (placeholder UI for lists or cards)
- Progress bars for long operations
- Debounced button spinners (e.g., appear only if >150ms)
- "Optimistic UI" updates for fast actions where state can appear immediately

These are not required now but can be added later.

---

# End of Specification

This document intentionally contains no code implementations. It defines behavior and architecture only, to be used by Copilot or developers when adding loading features to the UI.

