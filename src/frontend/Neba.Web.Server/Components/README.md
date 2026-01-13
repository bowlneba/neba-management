# NEBA Components Library

This document provides comprehensive documentation for all reusable Blazor components in the NEBA Management System.

## Table of Contents

1. [NebaDropdown](#nebadropdown)
2. [NebaLoadingIndicator](#nebaloadingindicator)
3. [NebaModal](#nebamodal)
4. [NebaSegmentedControl](#nebasegmentedcontrol)
5. [NebaSkeletonLoader](#nebaskeletonloader)
6. [NebaErrorBoundary](#nebaerrorboundary)
7. [NebaDocument](#nebadocument)

---

## NebaDropdown

A searchable dropdown component with generic value and item types. Supports keyboard navigation, filtering, and click-outside detection. Inspired by MudBlazor's MudSelect architecture.

### Features

- **Searchable/Filterable**: Case-insensitive search on display text
- **Keyboard Navigation**: Arrow keys, Enter, Escape, Tab
- **Generic Types**: Support for any value/item type combination
- **Auto-select**: First filtered item is highlighted when dropdown opens
- **Click-outside Detection**: JavaScript interop for proper UX
- **Mobile-Friendly**: Touch targets optimized for mobile devices
- **Accessible**: ARIA labels, roles, and keyboard navigation

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Value` | `TValue?` | No | `null` | The currently selected value |
| `ValueChanged` | `EventCallback<TValue>` | No | - | Event callback invoked when the value changes |
| `Items` | `IEnumerable<TItem>` | Yes | `Empty` | The collection of items to display |
| `DisplayProperty` | `Func<TItem, string>?` | No | `null` | Function to extract display text from an item |
| `ValueProperty` | `Func<TItem, TValue>?` | No | `null` | Function to extract value from an item |
| `Label` | `string?` | No | `null` | Label text displayed above the dropdown |
| `Placeholder` | `string` | No | `"Type to search..."` | Placeholder text in search input |
| `Disabled` | `bool` | No | `false` | Whether the dropdown is disabled |
| `Required` | `bool` | No | `false` | Whether the dropdown is required |
| `Id` | `string?` | No | Auto-generated | HTML id attribute for label association |

### Usage Examples

#### Simple String List

```razor
<NebaDropdown @bind-Value="@selectedYear"
              Items="@yearStrings"
              Label="Select year:"
              Placeholder="Type to search..." />

@code {
    private string selectedYear;
    private List<string> yearStrings = Enumerable
        .Range(1963, DateTime.Now.Year - 1963 + 1)
        .Select(y => y.ToString())
        .ToList();
}
```

#### Integer Values

```razor
<NebaDropdown TValue="int"
              TItem="int"
              @bind-Value="@Year"
              Items="@_availableYears"
              Label="View tournaments from:"
              Placeholder="Select a year..." />

@code {
    private int Year;
    private List<int> _availableYears = Enumerable.Range(1963, DateTime.Now.Year - 1963 + 1)
        .OrderByDescending(y => y)
        .ToList();
}
```

#### Complex Objects

```razor
<NebaDropdown TValue="Guid"
              TItem="BowlingCenter"
              @bind-Value="@selectedBowlingCenterId"
              Items="@bowlingCenters"
              DisplayProperty="@(c => c.Name)"
              ValueProperty="@(c => c.Id)"
              Label="Select bowling center:"
              Placeholder="Type to search..." />

@code {
    private Guid selectedBowlingCenterId;
    private List<BowlingCenter> bowlingCenters = new();

    public class BowlingCenter
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
```

### Keyboard Support

- **Arrow Down/Up**: Navigate through filtered items
- **Enter**: Select highlighted item and close dropdown
- **Escape**: Close dropdown without changing selection
- **Tab**: Close dropdown and move to next focusable element
- **Type**: Filter items by display text

### Accessibility

- Uses proper ARIA roles (`combobox`, `listbox`, `option`)
- Supports `aria-expanded`, `aria-activedescendant`, `aria-labelledby`
- Keyboard navigation updates ARIA attributes
- Touch-friendly targets (minimum 44px height)

---

## NebaLoadingIndicator

An animated loading indicator with smart delay and minimum display duration to prevent jarring flashes.

### Features

- **Smart Delay**: Configurable delay before showing (prevents flash for fast operations)
- **Minimum Display**: Once shown, displays for minimum duration (prevents jarring flash)
- **Multiple Scopes**: Page, FullScreen, or Section overlay
- **Wave Animation**: Modern wave-style loading animation
- **Overlay Protection**: Prevents interaction with underlying content

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `IsVisible` | `bool` | No | `false` | Controls whether the loading indicator is visible |
| `Text` | `string?` | No | `null` | Optional text to display below the loading animation |
| `Scope` | `LoadingIndicatorScope` | No | `Page` | Determines the overlay scope |
| `DelayMs` | `int` | No | `100` | Delay in milliseconds before showing the indicator |
| `MinimumDisplayMs` | `int` | No | `500` | Minimum time to display once shown |
| `OnOverlayClick` | `EventCallback` | No | - | Callback invoked when overlay is clicked |

### LoadingIndicatorScope

- **`Page`**: Overlays only the page content area (default)
- **`FullScreen`**: Overlays the entire viewport including navigation
- **`Section`**: Overlays a specific section (parent needs `position: relative`)

### Usage Examples

#### Basic Usage

```razor
<NebaLoadingIndicator IsVisible="@_isLoading"
                      Text="Loading tournaments..." />

@code {
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        _isLoading = false;
    }
}
```

#### Fullscreen Loading

```razor
<NebaLoadingIndicator IsVisible="@_isProcessing"
                      Text="Processing payment..."
                      Scope="LoadingIndicatorScope.FullScreen"
                      DelayMs="200"
                      MinimumDisplayMs="800" />
```

#### Section Loading

```razor
<div style="position: relative; min-height: 400px;">
    <NebaLoadingIndicator IsVisible="@_isLoadingSection"
                          Text="Loading section..."
                          Scope="LoadingIndicatorScope.Section" />
    <!-- Section content -->
</div>
```

---

## NebaModal

A modal dialog component with backdrop blur and centered content. Supports customizable headers, footers, and click-outside behavior.

### Features

- **Backdrop Blur**: Visual focus on modal content
- **Click-outside Detection**: Configurable close behavior
- **Custom Footer**: Support for action buttons
- **Body Scroll Lock**: Prevents background scrolling via JavaScript
- **Keyboard Support**: Escape key to close (handled by browser default)

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `IsOpen` | `bool` | Yes | - | Controls whether the modal is visible |
| `OnClose` | `EventCallback` | Yes | - | Callback invoked when the modal should be closed |
| `Title` | `string?` | No | `null` | Optional title displayed at the top |
| `ChildContent` | `RenderFragment?` | No | `null` | The main content to display in the modal body |
| `FooterContent` | `RenderFragment?` | No | `null` | Optional footer content (e.g., action buttons) |
| `ShowCloseButton` | `bool` | No | `true` | Whether to show the X close button in the header |
| `CloseOnBackdropClick` | `bool` | No | `true` | Whether clicking the backdrop should close the modal |
| `MaxWidth` | `string?` | No | `null` | Optional maximum width (e.g., "600px", "80%", "lg") |
| `CssClass` | `string?` | No | `null` | Optional CSS class for custom styling |

### Usage Examples

#### Basic Modal

```razor
<NebaModal IsOpen="@_showModal"
           OnClose="@HandleCloseModal"
           Title="Confirm Action">
    <p>Are you sure you want to delete this tournament?</p>
</NebaModal>

@code {
    private bool _showModal = false;

    private void HandleCloseModal()
    {
        _showModal = false;
    }
}
```

#### Modal with Footer Actions

```razor
<NebaModal IsOpen="@_showDeleteModal"
           OnClose="@CancelDelete"
           Title="Delete Tournament"
           MaxWidth="500px">
    <p>This action cannot be undone. Are you sure?</p>

    <FooterContent>
        <button class="neba-btn neba-btn-secondary" @onclick="CancelDelete">
            Cancel
        </button>
        <button class="neba-btn neba-btn-danger" @onclick="ConfirmDelete">
            Delete
        </button>
    </FooterContent>
</NebaModal>

@code {
    private bool _showDeleteModal = false;

    private void CancelDelete()
    {
        _showDeleteModal = false;
    }

    private async Task ConfirmDelete()
    {
        // Perform delete
        _showDeleteModal = false;
    }
}
```

#### Non-Dismissible Modal

```razor
<NebaModal IsOpen="@_isProcessing"
           OnClose="@(() => { })"
           ShowCloseButton="false"
           CloseOnBackdropClick="false"
           Title="Processing">
    <NebaLoadingIndicator IsVisible="true" Text="Please wait..." />
</NebaModal>
```

---

## NebaSegmentedControl

An iOS-style segmented control for switching between options. Provides a clean, modern way to select between 2-4 related options.

### Features

- **iOS-style Design**: Familiar mobile-first UI pattern
- **Keyboard Navigation**: Arrow keys to navigate options
- **Accessible**: ARIA roles and labels
- **Smooth Transitions**: Animated selection indicator

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Options` | `List<SegmentedControlOption>` | Yes | - | The list of options to display |
| `SelectedValue` | `string` | Yes | - | The currently selected option value |
| `OnValueChanged` | `EventCallback<string>` | Yes | - | Callback invoked when selection changes |
| `CssClass` | `string?` | No | `null` | Optional CSS class for custom styling |

### SegmentedControlOption

```csharp
public sealed record SegmentedControlOption
{
    public required string Label { get; init; }  // Display text
    public required string Value { get; init; }  // Unique identifier
}
```

### Usage Examples

#### Basic Usage

```razor
<NebaSegmentedControl Options="@_viewOptions"
                      SelectedValue="@_selectedView"
                      OnValueChanged="@HandleViewChanged" />

@code {
    private string _selectedView = "future";
    private List<SegmentedControlOption> _viewOptions = new()
    {
        new() { Label = "Future", Value = "future" },
        new() { Label = "Past", Value = "past" },
        new() { Label = "All", Value = "all" }
    };

    private void HandleViewChanged(string value)
    {
        _selectedView = value;
    }
}
```

#### Tournament Filter Example

```razor
<NebaSegmentedControl Options="@_tournamentTypes"
                      SelectedValue="@_selectedType"
                      OnValueChanged="@FilterTournaments" />

@code {
    private string _selectedType = "all";
    private List<SegmentedControlOption> _tournamentTypes = new()
    {
        new() { Label = "All", Value = "all" },
        new() { Label = "Singles", Value = "singles" },
        new() { Label = "Doubles", Value = "doubles" },
        new() { Label = "Team", Value = "team" }
    };

    private void FilterTournaments(string type)
    {
        _selectedType = type;
        // Apply filter logic
    }
}
```

### Best Practices

- Use 2-4 options maximum (more becomes unwieldy)
- Keep labels short (1-2 words)
- Use for mutually exclusive options
- Consider horizontal space on mobile

---

## NebaSkeletonLoader

Loading placeholders that improve perceived performance by showing content structure before data arrives.

### Features

- **Multiple Types**: Card, Table, Text, Avatar, Custom
- **Configurable**: Rows, size, dimensions
- **Smooth Animation**: Pulsing effect
- **Responsive**: Adapts to container width

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Type` | `SkeletonType` | No | `Text` | The type of skeleton to display |
| `Rows` | `int` | No | `3` | Number of rows to display (for Table and Text types) |
| `Size` | `int` | No | `48` | Size of the avatar in pixels (for Avatar type) |
| `ShowText` | `bool` | No | `true` | Show text lines next to avatar (for Avatar type) |
| `Width` | `string` | No | `"100%"` | Custom width (for Custom type) |
| `Height` | `string` | No | `"1rem"` | Custom height (for Custom type) |

### SkeletonType

- **`Card`**: Card layout with title and text lines
- **`Table`**: Table rows layout
- **`Text`**: Simple text lines
- **`Avatar`**: Avatar with optional text lines
- **`Custom`**: Custom dimensions

### Usage Examples

#### Card Skeleton

```razor
@if (_isLoading)
{
    <NebaSkeletonLoader Type="SkeletonType.Card" />
}
else
{
    <div class="neba-card">
        <!-- Actual card content -->
    </div>
}
```

#### Table Skeleton

```razor
@if (_isLoadingTable)
{
    <NebaSkeletonLoader Type="SkeletonType.Table" Rows="5" />
}
else
{
    <table>
        <!-- Actual table content -->
    </table>
}
```

#### Avatar Skeleton

```razor
@if (_isLoadingProfile)
{
    <NebaSkeletonLoader Type="SkeletonType.Avatar" Size="64" ShowText="true" />
}
else
{
    <div class="profile">
        <img src="@user.Avatar" />
        <div>
            <h3>@user.Name</h3>
            <p>@user.Email</p>
        </div>
    </div>
}
```

#### Custom Skeleton

```razor
<NebaSkeletonLoader Type="SkeletonType.Custom" Width="200px" Height="100px" />
```

---

## NebaErrorBoundary

Error boundary component for graceful error handling. Wraps content and catches exceptions, displaying a user-friendly error message with recovery options.

### Features

- **Graceful Degradation**: Shows friendly error instead of blank page
- **Recovery Action**: Try Again button to reset error state
- **Debug Info**: Optional stack trace display (disable in production)
- **Custom Messages**: Override default error messaging

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `ShowDetails` | `bool` | No | `false` | Whether to show detailed error info (stack trace) |
| `CustomMessage` | `string?` | No | `null` | Custom error message to display |

### Usage Examples

#### Basic Usage

```razor
<NebaErrorBoundary>
    <!-- Your page content -->
    <TournamentList />
</NebaErrorBoundary>
```

#### Development Mode with Details

```razor
@inject IWebHostEnvironment Environment

<NebaErrorBoundary ShowDetails="@Environment.IsDevelopment()">
    <!-- Your page content -->
</NebaErrorBoundary>
```

#### Custom Error Message

```razor
<NebaErrorBoundary CustomMessage="Unable to load tournament data. Please check your connection and try again.">
    <TournamentDetails TournamentId="@TournamentId" />
</NebaErrorBoundary>
```

### Best Practices

- Always set `ShowDetails="false"` in production
- Use custom messages for known error scenarios
- Wrap pages or large sections, not individual components
- Consider logging errors to external service

---

## NebaDocument

A comprehensive document viewer with table of contents, internal link slide-overs, and metadata display. Designed for displaying rich HTML content with navigation.

### Features

- **Table of Contents**: Auto-generated from headings with smooth scrolling
- **Mobile TOC Modal**: Responsive TOC for mobile devices
- **Internal Link Slide-overs**: Opens internal links in slide-over panel
- **Last Updated Info**: Displays document metadata
- **Hash Navigation**: Supports URL hash navigation to sections
- **Accessible**: Proper ARIA labels and keyboard navigation

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Content` | `MarkupString?` | Yes | - | The HTML content to display |
| `IsLoading` | `bool` | No | `false` | Controls loading indicator visibility |
| `LoadingText` | `string` | No | `"Loading document..."` | Text for loading indicator |
| `ErrorMessage` | `string?` | No | `null` | Error message to display if loading fails |
| `ErrorTitle` | `string` | No | `"Error Loading Document"` | Title for error alert |
| `OnErrorDismiss` | `EventCallback` | No | - | Callback when error is dismissed |
| `ShowTableOfContents` | `bool` | No | `true` | Whether to show the TOC |
| `TableOfContentsTitle` | `string` | No | `"Contents"` | Title for the TOC section |
| `HeadingLevels` | `string` | No | `"h1, h2"` | Heading levels to include in TOC |
| `DocumentId` | `string?` | No | Auto-generated | Unique identifier for this document instance |
| `Metadata` | `IReadOnlyDictionary<string, string>?` | No | `null` | Document metadata (LastUpdatedUtc, LastUpdatedBy) |

### Usage Examples

#### Basic Usage

```razor
<NebaDocument Content="@_htmlContent"
              IsLoading="@_isLoading"
              ErrorMessage="@_errorMessage" />

@code {
    private MarkupString _htmlContent;
    private bool _isLoading = true;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var html = await LoadDocumentAsync();
            _htmlContent = new MarkupString(html);
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isLoading = false;
        }
    }
}
```

#### With Metadata

```razor
<NebaDocument Content="@_content"
              Metadata="@_metadata"
              ShowTableOfContents="true"
              HeadingLevels="h1, h2, h3" />

@code {
    private Dictionary<string, string> _metadata = new()
    {
        ["LastUpdatedUtc"] = DateTime.UtcNow.ToString("o"),
        ["LastUpdatedBy"] = "John Doe"
    };
}
```

#### Custom Configuration

```razor
<NebaDocument Content="@_content"
              ShowTableOfContents="false"
              LoadingText="Loading guide..."
              ErrorTitle="Failed to Load Guide"
              DocumentId="user-guide" />
```

### Best Practices

- Use for long-form content (documentation, guides, articles)
- Include h1-h3 headings for good TOC structure
- Provide meaningful `DocumentId` for analytics/tracking
- Use metadata to show authorship and update info

---

## Component Design Principles

All NEBA components follow these design principles:

### 1. Accessibility First
- Proper ARIA labels and roles
- Keyboard navigation support
- Focus management
- Screen reader compatibility

### 2. Mobile-Responsive
- Touch-friendly targets (min 44px)
- Responsive breakpoints
- Mobile-specific UI patterns (modals, dropdowns)

### 3. Performance
- Smart delays (loading indicators)
- Minimal re-renders
- Efficient event handling
- Proper disposal of resources

### 4. Consistent Styling
- NEBA brand color (`#0047AB`)
- Consistent spacing and typography
- Scoped CSS with CSS Isolation
- Tailwind utility classes where appropriate

### 5. Developer Experience
- Clear parameter names and descriptions
- XML documentation comments
- Intuitive APIs
- Comprehensive examples

---

## Common Patterns

### Loading States

```razor
<NebaLoadingIndicator IsVisible="@_isLoading" Text="Loading..." />

@if (!_isLoading && _data != null)
{
    <!-- Display data -->
}
else if (!_isLoading && _data == null)
{
    <!-- Display empty state -->
}
```

### Error Handling

```razor
<NebaErrorBoundary ShowDetails="@IsDevelopment">
    @if (_errorMessage != null)
    {
        <NebaAlert Severity="NotifySeverity.Error" Message="@_errorMessage" />
    }
    else
    {
        <!-- Display content -->
    }
</NebaErrorBoundary>
```

### Skeleton Loading

```razor
@if (_isLoading)
{
    <NebaSkeletonLoader Type="SkeletonType.Card" />
}
else if (_data != null)
{
    <div class="neba-card">
        @_data
    </div>
}
```

### Modal Confirmation

```razor
<button @onclick="() => _showModal = true">Delete</button>

<NebaModal IsOpen="@_showModal" OnClose="@CancelDelete" Title="Confirm">
    <p>Are you sure?</p>
    <FooterContent>
        <button @onclick="CancelDelete">Cancel</button>
        <button @onclick="ConfirmDelete">Confirm</button>
    </FooterContent>
</NebaModal>
```

---

## Browser Support

All components are tested and supported on:

- **Chrome/Edge**: Latest 2 versions
- **Firefox**: Latest 2 versions
- **Safari**: Latest 2 versions
- **Mobile Safari (iOS)**: iOS 14+
- **Chrome Mobile (Android)**: Latest 2 versions

---

## Getting Help

For questions or issues with components:

1. Check this documentation
2. Review the component source code
3. Look for usage examples in the codebase
4. Contact the development team

---

**Last Updated**: 2026-01-11
