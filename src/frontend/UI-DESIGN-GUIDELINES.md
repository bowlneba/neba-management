# NEBA UI Design Guidelines

> **Last Updated**: December 2025
> **Version**: 2.0
> **Status**: Active

This document defines the design principles, component patterns, and best practices for building the NEBA Blazor UI.

---

## Table of Contents

1. [Design Principles](#design-principles)
2. [CSS Architecture](#css-architecture)
3. [Component Patterns](#component-patterns)
4. [Responsive Design](#responsive-design)
5. [Accessibility](#accessibility)
6. [Performance](#performance)
7. [Component Library](#component-library)
8. [Page Patterns](#page-patterns)
9. [Common Pitfalls](#common-pitfalls)

---

## Design Principles

### 1. Consistency Over Novelty

- Use established components from the NEBA design system
- Follow existing patterns for similar use cases
- Don't create custom solutions when standard components exist

### 2. Progressive Enhancement

- Start with semantic HTML
- Layer on CSS for presentation
- Add JavaScript for enhancement only
- Ensure core functionality works without JavaScript

### 3. Mobile-First Responsive

- Design for mobile viewports first (320px+)
- Enhance for tablet (768px+) and desktop (1024px+)
- Test at all breakpoints

### 4. Accessible by Default

- Use semantic HTML elements
- Provide ARIA labels where needed
- Support keyboard navigation
- Test with screen readers

### 5. Performance Conscious

- Minimize layout shifts
- Use skeleton loaders for async content
- Optimize animations (transform, opacity)
- Lazy load images and heavy components

---

## CSS Architecture

### The CSS Cascade Strategy

**Priority Order** (highest to lowest):

```
1. Theme CSS Classes (neba-*)
2. Tailwind Utility Classes (layout, spacing)
3. Scoped Component CSS (ComponentName.razor.css)
4. Inline Styles (avoid unless necessary)
```

### Theme CSS Classes

**Always prefer theme classes over inline Tailwind:**

```razor
<!-- ✅ Good: Theme class -->
<div class="neba-card">
    <button class="neba-btn neba-btn-primary">Save</button>
</div>

<!-- ❌ Bad: Inline Tailwind -->
<div class="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
    <button class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-500">Save</button>
</div>
```

### Available Theme Classes

**Layout & Containers:**
- `neba-card` - Card container with hover effects
- `neba-panel` - Panel container without hover
- `neba-grid` - Grid container (use with `neba-grid-cols-*`)
- `neba-space-y-*` - Vertical spacing (4, 6, 8)

**Buttons:**
- `neba-btn` (base) + `neba-btn-primary|secondary|danger`
- `neba-btn-sm|lg` for size variants

**Form Elements:**
- `neba-input`, `neba-select`, `neba-textarea`

**Alerts:**
- `neba-alert` + `neba-alert-success|warning|error|info|normal`
- `neba-alert-filled|outlined`

**Segmented Control:**
- `neba-segmented-control`
- `neba-segment-button`, `neba-segment-selected`

**Loading:**
- `neba-spinner` - Standard spinner
- `neba-skeleton` - Skeleton loader animation

**Links:**
- `neba-link` - Animated underline on hover

### Responsive Grid Utilities

```razor
<!-- Responsive grid -->
<div class="neba-grid neba-grid-cols-1 md:neba-grid-cols-2 lg:neba-grid-cols-3 xl:neba-grid-cols-4">
    <div class="neba-card">Card 1</div>
    <div class="neba-card">Card 2</div>
    <div class="neba-card">Card 3</div>
</div>
```

### Scoped CSS

Create scoped CSS files for component-specific styles:

```
ComponentName.razor
ComponentName.razor.css  <-- Automatically scoped
ComponentName.razor.js   <-- Optional JS interop
```

**Example:**
```css
/* TitleCountView.razor.css */
.tier-elite-section {
    background: linear-gradient(135deg, rgba(62, 79, 217, 0.05) 0%, rgba(42, 58, 160, 0.05) 100%);
    padding: 1.5rem;
    border-radius: 0.75rem;
    border-left: 4px solid var(--neba-blue-700);
}
```

### CSS Variables

Use theme CSS variables for colors:

```css
/* ✅ Good: Theme variables */
color: var(--neba-blue-700);
background: var(--neba-bg-panel);
border-color: var(--neba-border);

/* ❌ Bad: Hard-coded colors */
color: #3E4FD9;
background: #ffffff;
border-color: #E5E5E5;
```

**Available Variables:**
- Colors: `--neba-blue-{100,300,500,600,700}`, `--neba-gray-{050,100,200,300,400,700,800}`
- Semantic: `--neba-success`, `--neba-warning`, `--neba-accent-red`, `--neba-info`
- Layout: `--neba-bg`, `--neba-bg-panel`, `--neba-text`, `--neba-border`
- Radius: `--neba-radius`, `--neba-radius-lg`
- Transitions: `--neba-transition`

---

## Component Patterns

### 1. Error Boundaries

Wrap components that might fail with error boundaries:

```razor
<NebaErrorBoundary ShowDetails="@(!IsProduction)">
    <ComplexComponent />
</NebaErrorBoundary>
```

**When to use:**
- Complex components with external dependencies
- Components that fetch data
- Components with user input/interaction

### 2. Loading States

**Always show loading feedback for async operations.**

```razor
<!-- Section loading (preferred for independent content) -->
<div class="relative min-h-[400px]">
    <NebaLoadingIndicator IsVisible="@isLoading" Text="Loading data..." Scope="LoadingIndicatorScope.Section" />
    @if (data != null) { /* content */ }
</div>

<!-- Page loading (for critical operations) -->
<NebaLoadingIndicator IsVisible="@isProcessing"
                      Text="Processing..."
                      Scope="LoadingIndicatorScope.Page" />
```

### 3. Skeleton Loaders

Use skeleton loaders instead of spinners when you know the layout:

```razor
@if (isLoading)
{
    <NebaSkeletonLoader Type="SkeletonType.Card" />
}
else
{
    <div class="neba-card">@data.Title</div>
}
```

**Types available:**
- `Card` - Card layout
- `Table` - Table rows (specify Rows parameter)
- `Text` - Text lines
- `Avatar` - Avatar with optional text
- `Custom` - Custom dimensions

### 4. Modals

Use the `NebaModal` component for dialogs:

```razor
<NebaModal IsOpen="@isOpen"
           OnClose="@HandleClose"
           Title="Confirm Action"
           ShowCloseButton="true"
           MaxWidth="600px">
    <p>Are you sure you want to proceed?</p>

    <FooterContent>
        <button class="neba-btn neba-btn-danger" @onclick="Confirm">Confirm</button>
        <button class="neba-btn neba-btn-secondary" @onclick="HandleClose">Cancel</button>
    </FooterContent>
</NebaModal>
```

**Features:**
- Fade-in backdrop animation
- Slide-in content animation
- Keyboard ESC to close
- Backdrop click to close (configurable)
- Body scroll prevention

### 5. Notifications

See [NOTIFICATIONS.md](NOTIFICATIONS.md) for complete guide.

**Quick reference:**

```csharp
// Success toast
NotificationService.Success("Saved successfully!");

// Error with persistent alert
NotificationService.Error("Failed to save", behavior: NotifyBehavior.AlertAndToast);

// Validation errors
AlertService.ShowValidation(new[] { "Name required", "Invalid email" });
```

### 6. Segmented Controls

For tab-like navigation:

```razor
<NebaSegmentedControl Options="@viewOptions"
                      SelectedValue="@selectedView"
                      OnValueChanged="@HandleViewChange" />

@code {
    private List<SegmentedControlOption> viewOptions = new()
    {
        new() { Label = "By Titles", Value = "titles" },
        new() { Label = "By Year", Value = "year" }
    };
}
```

---

## Responsive Design

### Breakpoints

```css
/* Mobile (default) */
@media (max-width: 767px) { }

/* Tablet */
@media (min-width: 768px) and (max-width: 1100px) { }

/* Desktop */
@media (min-width: 1101px) { }
```

**Theme breakpoints match Tailwind:**
- `sm:` - 640px+
- `md:` - 768px+
- `lg:` - 1024px+
- `xl:` - 1280px+

### Mobile-First Patterns

```razor
<!-- ✅ Good: Mobile-first classes -->
<div class="text-sm md:text-base lg:text-lg">
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
<div class="p-4 md:p-6 lg:p-8">

<!-- ❌ Bad: Desktop-first -->
<div class="text-lg lg:text-sm">  <!-- backwards -->
```

### Responsive Tables

Convert tables to cards on mobile using scoped CSS:

```css
/* YearView.razor.css pattern */
@media (max-width: 640px) {
    .data-table thead {
        position: absolute;
        width: 1px;
        height: 1px;
        overflow: hidden;
    }

    .data-table tr {
        display: block;
        margin-bottom: 1rem;
        border: 1px solid var(--neba-border);
        border-radius: 0.5rem;
    }

    .data-table td::before {
        content: attr(data-label);
        font-weight: 600;
    }
}
```

### Touch Targets

Ensure touch targets are at least 44x44px:

```css
/* ✅ Good: Large enough for touch */
.neba-btn {
    min-height: 44px;
    padding: 0.5rem 1rem;
}

/* ❌ Bad: Too small */
button {
    padding: 2px 4px;
}
```

---

## Accessibility

### Semantic HTML

```razor
<!-- ✅ Good: Semantic elements -->
<nav aria-label="Main navigation">
    <ul>
        <li><a href="/tournaments">Tournaments</a></li>
    </ul>
</nav>

<main>
    <article>
        <h1>Page Title</h1>
        <section>Content</section>
    </article>
</main>

<!-- ❌ Bad: Div soup -->
<div class="nav">
    <div class="link">Tournaments</div>
</div>
<div class="content">
    <div class="title">Page Title</div>
</div>
```

### ARIA Labels

```razor
<!-- Buttons with icon-only -->
<button aria-label="Close modal" @onclick="Close">✕</button>

<!-- Loading indicators -->
<div class="neba-spinner" aria-label="Loading" role="status"></div>

<!-- Form fields -->
<label for="tournament-name" class="sr-only">Tournament Name</label>
<input id="tournament-name" type="text" />

<!-- Alerts -->
<div role="alert" aria-live="assertive">Error message</div>
<div role="status" aria-live="polite">Success message</div>
```

### Keyboard Navigation

```csharp
// Support arrow keys in controls
private async Task HandleKeyDown(KeyboardEventArgs e)
{
    if (e.Key == "ArrowLeft" || e.Key == "ArrowRight")
    {
        // Navigate between items
    }
    else if (e.Key == "Escape")
    {
        // Close modal/dropdown
    }
    else if (e.Key == "Enter")
    {
        // Activate item
    }
}
```

### Focus Management

```css
/* Ensure visible focus indicators */
*:focus-visible {
    outline: 2px solid var(--neba-blue-500);
    outline-offset: 2px;
}

/* Screen reader only text */
.sr-only {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border-width: 0;
}
```

### Skip Links

Already included in MainLayout.razor:

```razor
<a href="#main-content" class="sr-only focus:not-sr-only">
    Skip to main content
</a>
```

---

## Performance

### 1. Minimize Layout Shifts

Use skeleton loaders:

```razor
@if (isLoading)
{
    <NebaSkeletonLoader Type="SkeletonType.Card" />
}
else
{
    <div class="neba-card" style="min-height: 200px">
        @content
    </div>
}
```

### 2. Optimize Animations

Use GPU-accelerated properties:

```css
/* ✅ Good: GPU-accelerated */
.modal {
    transform: translateY(-20px);
    opacity: 0;
    transition: transform 0.3s ease, opacity 0.3s ease;
}

/* ❌ Bad: Causes layout thrashing */
.modal {
    top: -20px;
    transition: top 0.3s ease;
}
```

### 3. Respect Reduced Motion

```css
@media (prefers-reduced-motion: reduce) {
    *,
    *::before,
    *::after {
        animation-duration: 0.01ms !important;
        transition-duration: 0.01ms !important;
    }
}
```

### 4. Lazy Load Images

```razor
<img src="@imageSrc" loading="lazy" alt="Description" />
```

### 5. Virtualization for Large Lists

```razor
@using Microsoft.AspNetCore.Components.Web.Virtualization

<Virtualize Items="@largeList" Context="item">
    <div class="list-item">@item.Name</div>
</Virtualize>
```

---

## Component Library

### Core Components

| Component | Purpose | Documentation |
|-----------|---------|---------------|
| `NebaModal` | Dialog/modal overlays | [NebaModal.razor](Neba.Web.Server/Components/NebaModal.razor) |
| `NebaSegmentedControl` | Tab-like navigation | [NebaSegmentedControl.razor](Neba.Web.Server/Components/NebaSegmentedControl.razor) |
| `NebaLoadingIndicator` | Page and section loading (use `Scope="LoadingIndicatorScope.Section"` for section-specific) | [LOADING.md](LOADING.md) |
| `NebaSkeletonLoader` | Loading placeholders | [NebaSkeletonLoader.razor](Neba.Web.Server/Components/NebaSkeletonLoader.razor) |
| `NebaErrorBoundary` | Error handling | [NebaErrorBoundary.razor](Neba.Web.Server/Components/NebaErrorBoundary.razor) |
| `NebaAlert` | Persistent notifications | [NOTIFICATIONS.md](NOTIFICATIONS.md) |
| `NebaToast` | Ephemeral notifications | [NOTIFICATIONS.md](NOTIFICATIONS.md) |
| `NebaIcon` | Severity icons | [NebaIcon.razor](Neba.Web.Server/Notifications/NebaIcon.razor) |

### When to Create New Components

**Create a new component when:**
- The pattern is used in 3+ places
- The logic is complex and reusable
- The component has clear boundaries and responsibilities

**Don't create a component when:**
- It's only used once
- It's a simple wrapper with no logic
- It makes the code harder to understand

---

## Page Patterns

### 1. Standard Page Structure

```razor
@page "/page-path"
@using Neba.Web.Server.Notifications
@inject Service Service

<PageTitle>Page Name - NEBA</PageTitle>
<HeadContent>
    <meta name="description" content="Page description for SEO" />
</HeadContent>

<NebaErrorBoundary>
    <div class="neba-space-y-6">
        <!-- Page Header -->
        <div>
            <h1 class="text-3xl font-bold text-[var(--neba-text)]">Page Title</h1>
            <p class="mt-1 text-sm text-gray-600">Page description</p>
        </div>

        <!-- Content with loading state -->
        <div class="relative min-h-[400px]">
            <NebaLoadingIndicator Scope="LoadingIndicatorScope.Section" IsVisible="@isLoading" Text="Loading..." />

            @if (data != null)
            {
                <!-- Render data -->
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
            data = await Service.LoadDataAsync();
        }
        catch (Exception ex)
        {
            // Error boundary will catch this
            throw;
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

### 2. SEO Metadata Pattern

Always include:

```razor
<PageTitle>Specific Page Title - NEBA</PageTitle>
<HeadContent>
    <meta name="description" content="Descriptive content about this page (150-160 chars)" />
</HeadContent>
```

### 3. Empty State Pattern

```razor
@if (items.Count == 0 && !isLoading)
{
    <div class="text-center py-12">
        <svg class="mx-auto h-12 w-12 text-gray-400" ...></svg>
        <h3 class="mt-2 text-sm font-medium text-gray-900">No items found</h3>
        <p class="mt-1 text-sm text-gray-500">Get started by creating a new item.</p>
        <div class="mt-6">
            <button class="neba-btn neba-btn-primary" @onclick="CreateNew">
                Create New
            </button>
        </div>
    </div>
}
```

### 4. Error Page Pattern

See [NotFound.razor](Neba.Web.Server/Pages/NotFound.razor) for reference:

- Large, clear 404 indicator
- Friendly error message
- Action buttons (Return Home, etc.)
- Helpful links to popular pages
- Proper SEO metadata

---

## Common Pitfalls

### 1. ❌ Creating Duplicate Components

**Problem:**
```razor
<!-- Bad: Creating new button variant -->
<button class="my-custom-btn blue">Save</button>
```

**Solution:**
```razor
<!-- Good: Use existing theme classes -->
<button class="neba-btn neba-btn-primary">Save</button>
```

### 2. ❌ Inconsistent CSS Approach

**Problem:**
```razor
<!-- Bad: Mix of approaches -->
<div class="bg-white p-4 rounded-lg shadow-sm">
    <button class="neba-btn-primary">Save</button>  <!-- Missing neba-btn base -->
</div>
```

**Solution:**
```razor
<!-- Good: Consistent theme classes -->
<div class="neba-card">
    <button class="neba-btn neba-btn-primary">Save</button>
</div>
```

### 3. ❌ Forgetting Loading States

**Problem:**
```csharp
// Bad: No loading feedback
protected override async Task OnInitializedAsync()
{
    data = await LoadDataAsync();  // User sees nothing during load
}
```

**Solution:**
```csharp
// Good: Show loading state
private bool isLoading = true;

protected override async Task OnInitializedAsync()
{
    try
    {
        data = await LoadDataAsync();
    }
    finally
    {
        isLoading = false;
    }
}
```

### 4. ❌ Poor Error Handling

**Problem:**
```csharp
// Bad: Silent failure
try { await SaveAsync(); }
catch { /* Do nothing */ }
```

**Solution:**
```csharp
// Good: User feedback
try
{
    await SaveAsync();
    NotificationService.Success("Saved successfully");
}
catch (Exception ex)
{
    NotificationService.Error($"Failed to save: {ex.Message}");
}
```

### 5. ❌ Accessibility Gaps

**Problem:**
```razor
<!-- Bad: No labels or ARIA -->
<button @onclick="Delete">🗑️</button>
<div>Loading...</div>  <!-- No ARIA for screen readers -->
```

**Solution:**
```razor
<!-- Good: Proper ARIA labels -->
<button @onclick="Delete" aria-label="Delete item">🗑️</button>
<div class="neba-spinner" role="status" aria-label="Loading"></div>
```

### 6. ❌ Hardcoded Colors

**Problem:**
```css
/* Bad: Hard-coded colors */
.my-component {
    color: #3E4FD9;
    background: #ffffff;
}
```

**Solution:**
```css
/* Good: Theme variables */
.my-component {
    color: var(--neba-blue-700);
    background: var(--neba-bg-panel);
}
```

### 7. ❌ Missing Mobile Responsiveness

**Problem:**
```razor
<!-- Bad: Fixed desktop layout -->
<div class="grid grid-cols-4 gap-6">
```

**Solution:**
```razor
<!-- Good: Mobile-first responsive -->
<div class="neba-grid neba-grid-cols-1 md:neba-grid-cols-2 lg:neba-grid-cols-4">
```

---

## Migration Guide

### Updating Existing Components

If you have components using old patterns:

1. **Replace inline Tailwind with theme classes:**
   ```razor
   <!-- Before -->
   <div class="bg-white border border-gray-200 rounded-lg p-4">

   <!-- After -->
   <div class="neba-card">
   ```

2. **Add loading states:**
   ```razor
   <!-- Before -->
   @if (data != null) { /* render */ }

   <!-- After -->
   <div class="relative min-h-[400px]">
       <NebaLoadingIndicator Scope="LoadingIndicatorScope.Section" IsVisible="@isLoading" Text="Loading..." />
       @if (data != null) { /* render */ }
   </div>
   ```

3. **Add error boundaries:**
   ```razor
   <!-- Wrap complex components -->
   <NebaErrorBoundary>
       <ComplexComponent />
   </NebaErrorBoundary>
   ```

4. **Add SEO metadata:**
   ```razor
   <PageTitle>Specific Title - NEBA</PageTitle>
   <HeadContent>
       <meta name="description" content="..." />
   </HeadContent>
   ```

---

## Resources

### Official Documentation

- **Notifications**: [NOTIFICATIONS.md](NOTIFICATIONS.md)
- **Loading States**: [LOADING.md](LOADING.md)
- **Component Specs**:
  - [UI Loading Instructions](.github/instructions/ui-loading.instructions.md)
  - [UI Notifications Instructions](.github/instructions/ui-notifications.instructions.md)

### Blazor Resources

- [Blazor Component Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/)
- [Blazor Accessibility](https://learn.microsoft.com/en-us/aspnet/core/blazor/accessibility)

### Design System

- Theme CSS: [neba_theme.css](Neba.Web.Server/wwwroot/neba_theme.css)
- Component Library: [src/frontend/Neba.Web.Server/Components/](Neba.Web.Server/Components/)

---

## Version History

**v2.0** (December 2025)
- Added skeleton loader component
- Added error boundary component
- Enhanced theme CSS with animations
- Added scoped CSS patterns
- Updated responsive design guidelines
- Added performance best practices
- Expanded component library

**v1.0** (Initial)
- Initial design system
- Notification system
- Loading patterns
- Basic component library

---

## Contributing

When adding new components or patterns:

1. Follow the CSS architecture (theme classes first)
2. Ensure mobile responsiveness
3. Add accessibility attributes
4. Include loading states
5. Provide clear documentation
6. Add to this guide if it's a new pattern

---

**For questions or clarifications, consult the team or review existing component implementations in [src/frontend/Neba.Web.Server/](Neba.Web.Server/).**
