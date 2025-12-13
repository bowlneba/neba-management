# NEBA UI Design System — Instructions for AI Agents

> **Authority**: This document provides concise instructions for building UI components in the NEBA Blazor application.
> **Complete Reference**: See [src/frontend/UI-DESIGN-GUIDELINES.md](../../src/frontend/UI-DESIGN-GUIDELINES.md) for comprehensive guidelines.

---

## Quick Reference for Component Development

### CSS Architecture Priority

When styling components, follow this strict hierarchy:

1. **Theme CSS Classes** (`neba-*`) — ALWAYS use first
2. **Tailwind Utility Classes** — Only for layout and spacing
3. **Scoped Component CSS** (`ComponentName.razor.css`) — For component-specific styles
4. **Inline Styles** — Avoid unless absolutely necessary

### Required Theme Classes

```razor
<!-- ✅ CORRECT: Use theme classes -->
<div class="neba-card">
    <button class="neba-btn neba-btn-primary">Save</button>
</div>

<!-- ❌ WRONG: Don't recreate with Tailwind -->
<div class="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
    <button class="bg-blue-600 text-white px-4 py-2 rounded">Save</button>
</div>
```

**Available Theme Classes:**
- Layout: `neba-card`, `neba-panel`, `neba-grid`, `neba-space-y-{4,6,8}`
- Buttons: `neba-btn` + `neba-btn-{primary,secondary,danger,sm,lg}`
- Forms: `neba-input`, `neba-select`, `neba-textarea`
- Alerts: `neba-alert-{success,warning,error,info,normal}`, `neba-alert-{filled,outlined}`
- Loading: `neba-spinner`, `neba-skeleton`
- Links: `neba-link`
- Segmented: `neba-segmented-control`, `neba-segment-button`, `neba-segment-selected`

### Color Variables

**ALWAYS use CSS variables, NEVER hard-coded colors:**

```css
/* ✅ CORRECT */
color: var(--neba-blue-700);
background: var(--neba-bg-panel);
border-color: var(--neba-border);

/* ❌ WRONG */
color: #3E4FD9;
background: #ffffff;
```

---

## Component Patterns

### 1. Standard Page Structure

```razor
@page "/page-path"
@using Neba.Web.Server.Notifications
@inject ServiceName Service

<PageTitle>Specific Title - NEBA</PageTitle>
<HeadContent>
    <meta name="description" content="Page description (150-160 chars)" />
</HeadContent>

<NebaErrorBoundary ShowDetails="@(!IsProduction)">
    <div class="neba-space-y-6">
        <!-- Page Header -->
        <div>
            <h1 class="text-3xl font-bold text-[var(--neba-text)]">Title</h1>
            <p class="mt-1 text-sm text-gray-600">Description</p>
        </div>

        <!-- Content Section with Loading -->
        <div class="relative min-h-[400px]">
            <NebaLoadingIndicator Scope="LoadingIndicatorScope.Section" IsVisible="@isLoading" Text="Loading..." />

            @if (data != null)
            {
                <div class="neba-grid neba-grid-cols-1 md:neba-grid-cols-2 lg:neba-grid-cols-3">
                    <!-- Content -->
                </div>
            }
        </div>
    </div>
</NebaErrorBoundary>

@code {
    private DataType? data;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            data = await Service.LoadAsync();
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

### 2. Loading States (REQUIRED)

**Every async operation MUST show loading feedback.**

```razor
<!-- Section Loading (preferred for independent content) -->
<div class="relative min-h-[400px]">
    <NebaLoadingIndicator IsVisible="@isLoading"
                          Text="Loading data..."
                          Scope="LoadingIndicatorScope.Section"
                          DelayMs="0"
                          MinimumDisplayMs="0" />
    @if (data != null) { /* content */ }
</div>

<!-- Page Loading (for critical operations) -->
<NebaLoadingIndicator IsVisible="@isProcessing"
                      Text="Processing..."
                      Scope="LoadingIndicatorScope.Page" />

<!-- Full Screen Loading (for app-wide operations) -->
<NebaLoadingIndicator IsVisible="@isInitializing"
                      Text="Loading application..."
                      Scope="LoadingIndicatorScope.FullScreen" />

<!-- Skeleton Loaders (when you know the layout) -->
@if (isLoading)
{
    <NebaSkeletonLoader Type="SkeletonType.Card" />
}
else
{
    <div class="neba-card">@data.Title</div>
}
```

**LoadingIndicatorScope Options:** `Section`, `Page`, `FullScreen`
- **Section**: Overlays a specific container (use with `relative` positioning and `min-height`)
- **Page**: Overlays the page content area
- **FullScreen**: Overlays the entire viewport

**SkeletonType Options:** `Card`, `Table`, `Text`, `Avatar`, `Custom`

### 3. Error Boundaries (REQUIRED for complex components)

```razor
<NebaErrorBoundary ShowDetails="@(!IsProduction)">
    <ComplexComponent />
</NebaErrorBoundary>
```

### 4. Notifications

See [ui-notifications.instructions.md](ui-notifications.instructions.md) for complete spec.

**Quick patterns:**

```csharp
// Success toast
NotificationService.Success("Saved successfully!");

// Error with persistent alert
NotificationService.Error("Failed to save", behavior: NotifyBehavior.AlertAndToast);

// Validation errors (multiple messages)
AlertService.ShowValidation(new[] { "Name required", "Invalid email" });
```

### 5. Modals

```razor
<NebaModal IsOpen="@isOpen"
           OnClose="@HandleClose"
           Title="Modal Title"
           MaxWidth="600px">
    <p>Modal content</p>

    <FooterContent>
        <button class="neba-btn neba-btn-primary" @onclick="Confirm">Confirm</button>
        <button class="neba-btn neba-btn-secondary" @onclick="HandleClose">Cancel</button>
    </FooterContent>
</NebaModal>
```

### 6. Responsive Grids

```razor
<!-- Mobile-first responsive grid -->
<div class="neba-grid neba-grid-cols-1 md:neba-grid-cols-2 lg:neba-grid-cols-3 xl:neba-grid-cols-4">
    <div class="neba-card">Card 1</div>
    <div class="neba-card">Card 2</div>
</div>
```

---

## Accessibility Requirements

### 1. Semantic HTML

```razor
<!-- ✅ CORRECT: Semantic elements -->
<nav aria-label="Main navigation"><ul><li>...</li></ul></nav>
<main><article><h1>Title</h1></article></main>

<!-- ❌ WRONG: Div soup -->
<div class="nav"><div class="link">...</div></div>
```

### 2. ARIA Labels

```razor
<!-- Icon-only buttons -->
<button aria-label="Close modal" @onclick="Close">✕</button>

<!-- Loading indicators -->
<div class="neba-spinner" aria-label="Loading" role="status"></div>

<!-- Alerts -->
<div role="alert" aria-live="assertive">Error message</div>
<div role="status" aria-live="polite">Success message</div>
```

### 3. Focus Management

All interactive elements MUST have visible focus indicators (already provided by theme CSS).

### 4. Keyboard Navigation

Support arrow keys, Escape, Enter where appropriate.

---

## SEO Requirements

**Every page MUST include:**

```razor
<PageTitle>Specific Page Title - NEBA</PageTitle>
<HeadContent>
    <meta name="description" content="Page description (150-160 chars)" />
</HeadContent>
```

---

## Mobile Responsiveness

### Breakpoints

- Mobile (default): < 768px
- Tablet: 768px - 1100px
- Desktop: > 1100px

### Mobile-First Pattern

```razor
<!-- ✅ CORRECT: Mobile-first -->
<div class="text-sm md:text-base lg:text-lg">
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3">

<!-- ❌ WRONG: Desktop-first -->
<div class="text-lg lg:text-sm">
```

### Responsive Tables

For data tables, create scoped CSS to convert to cards on mobile:

```css
/* ComponentName.razor.css */
@media (max-width: 640px) {
    .data-table thead { display: none; }
    .data-table tr {
        display: block;
        margin-bottom: 1rem;
        border: 1px solid var(--neba-border);
    }
}
```

See [YearView.razor.css](../../src/frontend/Neba.Web.Server/History/Champions/YearView.razor.css) for reference.

---

## Scoped CSS

Create `ComponentName.razor.css` for component-specific styles:

```css
/* Automatically scoped to component */
.custom-section {
    background: linear-gradient(135deg, rgba(62, 79, 217, 0.05) 0%, rgba(42, 58, 160, 0.05) 100%);
    padding: 1.5rem;
    border-left: 4px solid var(--neba-blue-700);
}
```

---

## Performance Best Practices

1. **Minimize Layout Shifts**: Use skeleton loaders
2. **Optimize Animations**: Use `transform` and `opacity` (GPU-accelerated)
3. **Respect Reduced Motion**: Already handled by theme CSS
4. **Lazy Load Images**: `<img loading="lazy" />`
5. **Virtualization**: Use `<Virtualize>` for lists > 100 items

---

## Common Mistakes to Avoid

### ❌ Don't Create Duplicate Components

```razor
<!-- WRONG: Creating custom button -->
<button class="my-custom-blue-btn">Save</button>

<!-- CORRECT: Use theme button -->
<button class="neba-btn neba-btn-primary">Save</button>
```

### ❌ Don't Mix CSS Approaches

```razor
<!-- WRONG: Inconsistent styling -->
<div class="bg-white p-4 rounded">
    <button class="neba-btn-primary">Save</button>
</div>

<!-- CORRECT: Consistent theme classes -->
<div class="neba-card">
    <button class="neba-btn neba-btn-primary">Save</button>
</div>
```

### ❌ Don't Forget Loading States

```csharp
// WRONG: No loading feedback
protected override async Task OnInitializedAsync()
{
    data = await LoadAsync();
}

// CORRECT: Show loading state
private bool isLoading = true;
protected override async Task OnInitializedAsync()
{
    try { data = await LoadAsync(); }
    finally { isLoading = false; }
}
```

### ❌ Don't Hardcode Colors

```css
/* WRONG */
.my-class { color: #3E4FD9; }

/* CORRECT */
.my-class { color: var(--neba-blue-700); }
```

### ❌ Don't Skip Error Handling

```csharp
// WRONG: Silent failure
try { await SaveAsync(); } catch { }

// CORRECT: User feedback
try {
    await SaveAsync();
    NotificationService.Success("Saved");
} catch (Exception ex) {
    NotificationService.Error($"Failed: {ex.Message}");
}
```

---

## Component Library Reference

| Component | Purpose | Doc Link |
|-----------|---------|----------|
| `NebaModal` | Dialogs/modals | [NebaModal.razor](../../src/frontend/Neba.Web.Server/Components/NebaModal.razor) |
| `NebaSegmentedControl` | Tab navigation | [NebaSegmentedControl.razor](../../src/frontend/Neba.Web.Server/Components/NebaSegmentedControl.razor) |
| `NebaLoadingIndicator` | Loading states (Section/Page/FullScreen) | [NebaLoadingIndicator.razor](../../src/frontend/Neba.Web.Server/Components/NebaLoadingIndicator.razor) |
| `NebaSkeletonLoader` | Loading placeholders | [NebaSkeletonLoader.razor](../../src/frontend/Neba.Web.Server/Components/NebaSkeletonLoader.razor) |
| `NebaErrorBoundary` | Error handling | [NebaErrorBoundary.razor](../../src/frontend/Neba.Web.Server/Components/NebaErrorBoundary.razor) |
| `NebaAlert` | Persistent alerts | [NOTIFICATIONS.md](../../src/frontend/NOTIFICATIONS.md) |
| `NebaToast` | Ephemeral toasts | [NOTIFICATIONS.md](../../src/frontend/NOTIFICATIONS.md) |

---

## Related Documentation

- **Complete Guidelines**: [src/frontend/UI-DESIGN-GUIDELINES.md](../../src/frontend/UI-DESIGN-GUIDELINES.md)
- **Notifications**: [ui-notifications.instructions.md](ui-notifications.instructions.md)
- **Loading States**: [ui-loading.instructions.md](ui-loading.instructions.md)
- **Theme CSS**: [src/frontend/Neba.Web.Server/wwwroot/neba_theme.css](../../src/frontend/Neba.Web.Server/wwwroot/neba_theme.css)

---

## Decision Framework

**When building a new component:**

1. ✅ Check if an existing component can be reused
2. ✅ Use theme CSS classes (not Tailwind for components)
3. ✅ Add loading states for async operations
4. ✅ Wrap complex components in `<NebaErrorBoundary>`
5. ✅ Ensure mobile responsiveness
6. ✅ Add ARIA labels and semantic HTML
7. ✅ Include SEO metadata for pages
8. ✅ Test keyboard navigation
9. ✅ Provide user feedback (notifications)
10. ✅ Use CSS variables for colors

---

**For detailed patterns, examples, and migration guides, always refer to [src/frontend/UI-DESIGN-GUIDELINES.md](../../src/frontend/UI-DESIGN-GUIDELINES.md).**
