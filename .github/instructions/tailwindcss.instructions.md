---
description: 'Tailwind CSS guidelines for NEBA UI'
applyTo: '**/*.razor, **/*.razor.css'
---

## Foundational Design Principles

1. Brand Consistency

Use NEBA logo color palette as the primary identity; avoid introducing additional blues or reds unless needed for contrast/accessibility.

2. Tailwind-First, Token-Driven

Layout, spacing, and responsiveness remain Tailwind-driven.  Visual styling (color, radii, typography, dark mode) is driven by CSSvariables in `neba_theme.css`.

3. Component-Led Design

Build reusable Blazor components that consume tokens + Tailwind utilities.

4. Minimal JS Philosophy

Only use JavaScript where Blazor cannot natively perform the task (e.g., dark mode root-class toggles, clipboard).

5. Uniform Experience

Server and WASM must behave identically and render the same UI.

6. Desktop First

Optimize for desktop workflows with responsive support for mobile.

## Core Branding Tokens (CSS Variables)

- All major theme colors, radii, typography, and transitions are defined in `neba_theme.css`.
- These variables are designed for
    - Tailwind utility pairing (e.g., `bg-[var(--neba-bg)]`)
    - Consistency across all Blazor components
    - Dark mode with zero Tailwind config extension

## Tailwind Configuration Strategy

- You are loading Tailwind from the CDN, but your theming layer is independent, therefore:
    - No Tailwind theme extension is required.
    - No custom colors in tailwind.config.js are necessary.

- Tailwind handles:
    - Spacing
    - Layout
    - Responsiveness
    - Flex/Grid
    - Shadows
    - Transitions
    - Typography sizing

- `neba_theme.css` handles:
    - Colors
    - Radii
    - Component helpers
    - Dark mode
    - Global background/text

## Global Overrides

`neba_theme.css` overrides app-wide styles

```css
html, body {
  background-color: var(--neba-bg);
  color: var(--neba-text);
  font-family: var(--neba-font-base);
}
```

These override `app.css` safely because they load last.

## Semantic Utility Classes

These are small, reusable helpers that complement Tailwind without replacing it.

### Panels

```css
.neba-panel {
  background-color: var(--neba-bg-panel);
  border: 1px solid var(--neba-border);
  border-radius: var(--neba-radius-lg);
  padding: 1rem;
}
```

### Dividers

```css
.neba-divider {
  border-bottom: 1px solid var(--neba-border);
  margin: 1rem 0;
}
```
### Headings

```css
.neba-heading {
  font-weight: 600;
  margin-bottom: 0.5rem;
}
```

### Card Containers

```css
.neba-card {
  background: var(--neba-bg-panel);
  border: 1px solid var(--neba-border);
  padding: 1rem;
  border-radius: var(--neba-radius-lg);
}
```

## Buttons (full semantic set)

These intentionally avoid Tailwind classes so that future theme changes require no markup changes.

### Base

```css
.neba-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 500;
  padding: 0.5rem 1rem;
  border-radius: var(--neba-radius);
  transition: var(--neba-transition);
  cursor: pointer;
  border: 1px solid transparent;
}
```

### Primary

```css
.neba-btn-primary {
  background-color: var(--neba-blue-700);
  color: white;
}
.neba-btn-primary:hover {
  background-color: var(--neba-blue-600);
}
```

### Secondary

```css
.neba-btn-secondary {
  background-color: transparent;
  border-color: var(--neba-blue-600);
  color: var(--neba-blue-700);
}
.neba-btn-secondary:hover {
  background-color: var(--neba-blue-300);
  color: white;
}
```

### Danger

```css
.neba-btn-danger {
  background-color: var(--neba-accent-red);
  color: white;
}
```

## Form Elements

Tokens ensure consistency across inputs regardless of Tailwind usage.

```css
.neba-input,
.neba-select,
.neba-textarea {
  width: 100%;
  padding: 0.5rem 0.75rem;
  border-radius: var(--neba-radius);
  border: 1px solid var(--neba-border);
  background-color: var(--neba-bg-panel);
  color: var(--neba-text);
  transition: var(--neba-transition);
}
```

### On Focus

```css
.neba-input:focus {
  border-color: var(--neba-blue-500);
  box-shadow: 0 0 0 3px rgba(2, 102, 200, 0.3);
}
```

## Tables

Enterprise data-heavy UI requires consistent tables.

```css
.neba-table {
  width: 100%;
  border-collapse: collapse;
}

.neba-table thead th {
  padding: 0.75rem;
  border-bottom: 2px solid var(--neba-border);
}
```

Hover rows for better scanability

```css
.neba-table tbody tr:hover {
  background-color: rgba(0, 0, 0, 0.03);
}
.dark .neba-table tbody tr:hover {
  background-color: rgba(255, 255, 255, 0.05);
}
```

## Loading Components

### Spinner

```css
.neba-spinner {
  height: 1.5rem;
  width: 1.5rem;
  border-radius: 9999px;
  border: 3px solid var(--neba-border);
  border-top-color: var(--neba-blue-600);
  animation: neba-spin 0.7s linear infinite;
}
```

Skeleton loader pattern is recommended for list-heavy areas.

## Layout Approach (Consistent)

Use Tailwind Grid for page layouts:

- Dashboards
- Multi-column admin views
- Split-pane scoring interfaces

Use Tailwind Flex for components

- Cards
- Buttons
- Tabs
- Inputs
- Table actions

## Accessibility Requirements

Mandatory for all PRs:

- Semantic HTML elements
- Labels for all form fields
- High contrast tokens baked into palette
- Focus-visible mechanics
- Keyboard navigation
- aria-live for real-time score updates
- Table semantics (scope, caption, sortable columns)

## Dark Mode Implementation

Strategy: class-based

This avoids OS-override issues and gives full Blazor control.

Blazor toggles the `.dark` class on the `<html>` element (minimal JS for class manipulation only).

All components automatically switch based on variable overrides.

## Component Folder Structure

The UI must be organized by feature, not by technical or architectural concerns.  This avoid scattering related UI across folders and aligns with modern modular front-end design.

### Root Structure

```
/Components
    /Shared
    /Layout
    /Infrastructure   (JS interop wrappers, theme switchers, browser helpers)
```

### Feature Folders

Each business capability owns its own folder

```
/Features
    /Tournaments
        /Components
        /Pages
        TournamentList.razor
        TournamentDetails.razor
        TournamentEntryForm.razor

    /Membership
        /Components
        /Pages
        MemberList.razor
        MemberProfile.razor
        MemberEditForm.razor

    /Payments
        /Components
        /Pages
        PaymentHistory.razor
        PaymentEntryForm.razor

    /Scoring
        /Components
        /Pages
        ScoreEntryForm.razor
        ScoreSheetView.razor
        ScoreLiveUpdates.razor

    /Administration
        /Components
        /Pages
        UserManagement.razor
        RoleManagement.razor
        Settings.razor
```

#### Inside each Feature Folder

Each feature folder contains

```
/Pages         → routable pages
/Components    → reusable UI within this feature only
/Services      → UI-specific services (if needed, but avoid business logic)
/Models        → UI-only models (e.g., ViewModels, form models)
```

**Important**

- Domain logic stays in the Domain layer
- Application logic stays in the Application layer
- UI services must NOT contain business rules - only UI behaviors and client API shaping.

### Shared Components

Some UI elements are reusable globally.  These live under

```
/Components/Shared
```

Examples

- PrimaryButton.razor
- SectionSpinner.razor
- Modal.razor
- ConfirmationDialog.razor
- Pagination.razor
- SearchBar.razor
- FormField.razor (common wrapper for neba-input)

### Layout Components

Global layouts are grouped separately

```
/Components/Layout
    MainLayout.razor
    AdminLayout.razor
    NavMenu.razor
    Sidebar.razor
    TopBar.razor
```

These should remain feature-agnostic

### Infrastructure UI Components

This folder holds small UI infrastructure helpers that are not feature-specific

```
/Components/Infrastructure
    ThemeToggle.razor
    DarkModeInterop.js
    BrowserStorageInterop.js
```

These exist purely to support UI mechanics.

## Performance Guidelines

- Keep JS Minimal
- Tailwind purge removes unused utilities
- Use system font stack
- Preload logo + CSS
- Server-side pagination for large datasets
- Using WASM or Server should NOT affect styling or load feel

## Blazor Component Example

```razor
<button class="neba-btn neba-btn-primary @CssClass"
        @onclick="OnClick"
        @attributes="AdditionalAttributes">
    @ChildContent
</button>
```

## Final Integration Notes

1. Load Tailwind first, then `app.css`, then `neba_theme.css`
2. All color/visual tweaks should happen in `neba_theme.css` not in Razor files.
3. Tailwind utilities should only control structure, spacing, and responsiveness.
4. This hybrid model gives NEBA a scalable design system without heavy Tailwind configuration.
