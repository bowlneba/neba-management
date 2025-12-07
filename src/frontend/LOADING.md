# NEBA Loading Indicators

This document provides practical guidance for using loading indicators in your Blazor components.

## Table of Contents

- [Overview](#overview)
- [Loading Patterns](#loading-patterns)
  - [Inline Loading](#inline-loading)
  - [Section Loading Overlay](#section-loading-overlay)
  - [Page Loading Overlay](#page-loading-overlay)
- [Component Reference](#component-reference)
- [Examples](#examples)
- [Best Practices](#best-practices)

---

## Overview

The NEBA application uses different loading patterns to communicate system status during asynchronous operations. Each pattern has specific use cases:

- **Inline Loading**: Simple spinner for data fetching
- **Section Loading Overlay**: Overlays a specific content region while keeping the rest of the page interactive
- **Page Loading Overlay**: Full-page blocking overlay for critical operations

---

## Loading Patterns

### Inline Loading

Use inline loading for simple data-fetching operations where the content is being loaded.

**When to use:**
- Loading tournament details
- Loading lists of data
- Fetching filter options

**Implementation:**

```razor
@if (isLoading)
{
    <div class="flex items-center gap-2">
        <div class="neba-spinner"></div>
        <span>Loading...</span>
    </div>
}
else
{
    <!-- Your content here -->
}
```

---

### Section Loading Overlay

Use section loading overlays when you want to load a specific content region while keeping the rest of the page (like headers, navigation, and other sections) interactive and visible.

**When to use:**
- Loading awards data while keeping the page description visible
- Loading statistics in a dashboard widget
- Loading chart data in a specific section
- Any content section that loads independently

**Implementation:**

```razor
<!-- Wrap the section in a relative container -->
<div class="relative min-h-[400px]">
    <!-- Section loading indicator -->
    <NebaSectionLoadingIndicator IsVisible="@isLoading"
                                 Text="Loading data..." />

    <!-- Your content -->
    @if (data != null)
    {
        <!-- Render data -->
    }
</div>
```

**Component Features:**
- Overlays only the specific section (not the entire page)
- Uses `bg-gray-50/95` background with backdrop blur for readability
- Centers spinner and text within the section
- Requires parent container with `position: relative`

**Example from BowlerOfTheYear page:**

```razor
<div class="relative min-h-[400px] mt-6">
    <NebaSectionLoadingIndicator IsVisible="@isLoading"
                                 Text="Loading awards..." />

    @if (awardsByYear.Count > 0)
    {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <!-- Award cards -->
        </div>
    }
</div>
```

---

### Page Loading Overlay

Use the page loading overlay for operations that require blocking all user interaction.

**When to use:**
- Payment processing
- Finalizing tournaments
- Bulk updates
- Critical operations where user interaction could corrupt data

**Implementation:**

```razor
<NebaLoadingIndicator IsVisible="@isLoading"
                      Text="Processing..."
                      Scope="LoadingIndicatorScope.Page" />
```

Or for full-screen blocking:

```razor
<NebaLoadingIndicator IsVisible="@isLoading"
                      Text="Processing payment..."
                      Scope="LoadingIndicatorScope.FullScreen" />
```

**LoadingIndicatorScope options:**
- `Page`: Overlays the page content area, leaving navigation accessible
- `FullScreen`: Overlays the entire viewport including navigation

---

## Component Reference

### NebaSectionLoadingIndicator

Located in [Components/NebaSectionLoadingIndicator.razor](Neba.Web.Server/Components/NebaSectionLoadingIndicator.razor)

**Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `IsVisible` | bool | - | Controls whether the loading indicator is visible |
| `Text` | string? | null | Optional text to display below the loading animation |

**Styling:**
- Background: `bg-gray-50/95` (light gray, 95% opacity)
- Backdrop blur: `backdrop-blur-sm`
- Position: Absolute within relative parent
- Z-index: 10

**Usage Notes:**
- Must be placed inside a container with `position: relative`
- Container should have a minimum height to prevent layout shift
- Uses the same `neba-loading-wave` animation as other loading indicators

---

### NebaLoadingIndicator

Located in [Components/NebaLoadingIndicator.razor](Neba.Web.Server/Components/NebaLoadingIndicator.razor)

**Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `IsVisible` | bool | - | Controls whether the loading indicator is visible |
| `Text` | string? | null | Optional text to display below the loading animation |
| `Scope` | LoadingIndicatorScope | Page | Determines overlay scope (Page or FullScreen) |
| `DelayMs` | int | 100 | Delay before showing indicator (prevents flash) |
| `MinimumDisplayMs` | int | 500 | Minimum time to display once shown |
| `OnOverlayClick` | EventCallback | - | Callback when overlay is clicked |

---

## Examples

### Example 1: Section Loading (Bowler of the Year)

```razor
@page "/history/bowler-of-the-year"

<div>
    <h1>Bowler of the Year</h1>

    <!-- Static content visible during load -->
    <div class="description">
        <p>The Bowler of the Year honors represent...</p>
    </div>

    <!-- Awards section with loading overlay -->
    <div class="relative min-h-[400px] mt-6">
        <NebaSectionLoadingIndicator IsVisible="@isLoading"
                                     Text="Loading awards..." />

        @if (awardsByYear.Count > 0)
        {
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                @foreach (var yearData in awardsByYear)
                {
                    <div class="award-card">
                        <!-- Award content -->
                    </div>
                }
            </div>
        }
    </div>
</div>

@code {
    private List<AwardData> awardsByYear = new();
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        awardsByYear = await LoadAwardsAsync();
        isLoading = false;
    }
}
```

### Example 2: Page Loading Overlay

```razor
<NebaLoadingIndicator IsVisible="@isProcessing"
                      Text="Finalizing tournament..."
                      Scope="LoadingIndicatorScope.Page" />

@code {
    private bool isProcessing = false;

    private async Task FinalizeTournament()
    {
        isProcessing = true;
        try
        {
            await TournamentService.FinalizeAsync(tournamentId);
            NotificationService.Success("Tournament finalized successfully");
        }
        catch (Exception ex)
        {
            NotificationService.Error($"Failed to finalize: {ex.Message}");
        }
        finally
        {
            isProcessing = false;
        }
    }
}
```

### Example 3: Inline Loading

```razor
@if (isLoading)
{
    <div class="flex items-center justify-center py-12">
        <div class="neba-spinner"></div>
        <span class="ml-2">Loading tournament data...</span>
    </div>
}
else if (tournament != null)
{
    <div class="tournament-details">
        <!-- Tournament content -->
    </div>
}
```

---

## Best Practices

### 1. Choose the Right Pattern

```csharp
// ✅ Good: Section overlay for independent content
<NebaSectionLoadingIndicator IsVisible="@loadingAwards" Text="Loading awards..." />

// ✅ Good: Page overlay for critical operations
<NebaLoadingIndicator IsVisible="@processing" Text="Processing payment..." Scope="LoadingIndicatorScope.Page" />

// ❌ Bad: Full page overlay for simple data fetch
<NebaLoadingIndicator IsVisible="@loadingList" Scope="LoadingIndicatorScope.FullScreen" />
```

### 2. Provide Clear Loading Text

```csharp
// ✅ Good: Specific and descriptive
Text="Loading awards..."
Text="Processing payment..."
Text="Saving changes..."

// ❌ Bad: Vague or missing
Text="Loading..."
Text=""
```

### 3. Use Relative Containers for Section Loading

```razor
// ✅ Good: Relative positioning with min-height
<div class="relative min-h-[400px]">
    <NebaSectionLoadingIndicator IsVisible="@isLoading" Text="Loading..." />
    <!-- Content -->
</div>

// ❌ Bad: No relative positioning
<div>
    <NebaSectionLoadingIndicator IsVisible="@isLoading" Text="Loading..." />
    <!-- Content won't be overlaid correctly -->
</div>
```

### 4. Handle Loading States Properly

```csharp
// ✅ Good: Set loading state, perform operation, clear loading state
protected override async Task OnInitializedAsync()
{
    isLoading = true;
    try
    {
        data = await LoadDataAsync();
    }
    finally
    {
        isLoading = false; // Always clear, even on error
    }
}

// ❌ Bad: Forget to clear loading state
protected override async Task OnInitializedAsync()
{
    isLoading = true;
    data = await LoadDataAsync();
    // Missing: isLoading = false;
}
```

### 5. Combine with Notifications

```csharp
// ✅ Good: Loading completes before showing notification
isLoading = false;
NotificationService.Success("Data loaded successfully");

// ✅ Good: Show error notification after clearing loading
isLoading = false;
NotificationService.Error("Failed to load data");
```

---

## Related Files

- **Implementation Spec**: [.github/instructions/ui-loading.instructions.md](../../.github/instructions/ui-loading.instructions.md)
- **Components**:
  - [Components/NebaSectionLoadingIndicator.razor](Neba.Web.Server/Components/NebaSectionLoadingIndicator.razor)
  - [Components/NebaLoadingIndicator.razor](Neba.Web.Server/Components/NebaLoadingIndicator.razor)
- **Styling**: [wwwroot/neba_theme.css](Neba.Web.Server/wwwroot/neba_theme.css)
- **Notifications**: [NOTIFICATIONS.md](NOTIFICATIONS.md)

---

## Decision Matrix

| Scenario | Pattern | Component |
|----------|---------|-----------|
| Loading page data | Inline or Section | `neba-spinner` or `NebaSectionLoadingIndicator` |
| Loading section while header visible | Section | `NebaSectionLoadingIndicator` |
| Loading awards while description visible | Section | `NebaSectionLoadingIndicator` |
| Loading dashboard widget | Section | `NebaSectionLoadingIndicator` |
| Processing payment | Page Overlay | `NebaLoadingIndicator` (Page/FullScreen) |
| Finalizing tournament | Page Overlay | `NebaLoadingIndicator` (Page/FullScreen) |
| Bulk operations | Page Overlay | `NebaLoadingIndicator` (Page/FullScreen) |
