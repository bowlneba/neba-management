---
description: 'Blazor component and application patterns'
applyTo: '**/*.razor, **/*.razor.cs, **/*.razor.css'
---

## Blazor Code Style and Structure

- Write idiomatic and efficient Blazor and C# code.
- Follow .NET and Blazor conventions.
- Use Razor Components appropriately for component-based UI development.
- Prefer inline functions for smaller components but separate complex logic into code-behind or service classes.
- Async/await should be used where applicable to ensure non-blocking UI operations.
- **Never use inline `<style>` and `<script>` tags in production Razor components.** Always extract styles to `.razor.css` files and scripts to `.razor.js` files for better maintainability, CSP compliance, and separation of concerns. This is a strict requirement for all production code.

### JavaScript Organization for Razor Components

**File Naming and Location:**
- JavaScript files must be co-located with their Razor component using the naming pattern: `ComponentName.razor.js`
- Place the `.razor.js` file in the same directory as the `.razor` file
- Example: For `BowlingCenters/BowlingCenters.razor`, create `BowlingCenters/BowlingCenters.razor.js`
- The build system automatically copies `.razor.js` files to `wwwroot/js/` during compilation

**JavaScript Module Pattern (Recommended):**
- Use ES6 modules with `export` keyword - do NOT use `window` global functions
- Load modules dynamically using `IJSObjectReference` in the component
- This provides component-scoped JavaScript and avoids global namespace pollution
- Components must implement `IAsyncDisposable` to properly dispose of module references

**Build Configuration:**
- The project file is configured to automatically copy `*.razor.js` files to `wwwroot/js/`
- Files are copied during build and publish operations
- No manual copying is required - just place the file next to your component

**Why This Matters:**
- **Co-location**: JavaScript code lives next to the component that uses it for better organization
- **Module Isolation**: No global namespace pollution - each component loads its own module
- **CSP Compliance**: Inline scripts in `<HeadContent>` are not available to Interactive Server components
- **Maintainability**: Easy to find and modify component-specific JavaScript
- **Performance**: Modules are loaded on-demand and can be cached
- **Security**: Enables proper Content Security Policy headers

**Implementation Example:**

*JavaScript Module (BowlingCenters.razor.js):*
```javascript
export function scrollToTop() {
    const element = document.querySelector('#centers-scroll-container');
    if (element) {
        element.scrollTop = 0;
    }
}
```

*Razor Component (BowlingCenters.razor):*
```razor
@page "/bowling-centers"
@implements IAsyncDisposable

@inject IJSRuntime JSInterop

@code {
    private IJSObjectReference? _jsModule;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await JSInterop.InvokeAsync<IJSObjectReference>(
                "import", "./js/BowlingCenters.razor.js");
        }
    }

    private async Task ScrollToTop()
    {
        if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync("scrollToTop");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            await _jsModule.DisposeAsync();
        }
    }
}
```

**File Structure:**
```
src/frontend/Neba.Web.Server/
├── BowlingCenters/
│   ├── BowlingCenters.razor          ← Component
│   ├── BowlingCenters.razor.js       ← Component JavaScript module (co-located)
│   └── BowlingCenterViewModel.cs
└── wwwroot/js/                        ← Build copies .razor.js files here
    └── BowlingCenters.razor.js       (copied during build)
```

**Important Notes:**
- Never use inline `<script>` tags in `<HeadContent>` - they won't work with Interactive Server components
- Use ES6 `export` keyword, NOT `window.functionName` for module functions
- Always implement `IAsyncDisposable` when using `IJSObjectReference`
- Load modules in `OnAfterRenderAsync(firstRender)` to ensure DOM is ready
- Check module is not null before invoking functions
- Keep JavaScript minimal - complex logic should be in C# when possible

## Modern UI/UX Patterns and Best Practices

### Design Philosophy
- Maintain a modern, professional aesthetic throughout the application
- Use subtle, purposeful animations that enhance UX without being distracting
- Follow the established NEBA theme color palette and design system
- Prioritize accessibility and keyboard navigation

### Animation and Transitions
- Use smooth transitions (0.15-0.4s duration) for all interactive elements
- Implement hover effects with subtle transforms: `translateY(-1px)` to `translateY(-4px)` for lift effects
- Add focus state animations that expand outline-offset on hover for better accessibility
- Use hardware-accelerated properties (`transform`, `opacity`) for better performance
- Keep animations subtle and professional - avoid bouncy or overly playful effects

### Interactive Elements
- **Buttons**: Add subtle lift on hover (`translateY(-1px)`), shadow depth increase, and press feedback on active state
- **Cards**: Implement hover lift effect (`translateY(-4px)`) with expanding shadow and accent border color change
- **Links**: Use animated underline effects that slide in using `transform: scaleX()` transitions
- **Form Inputs**: Inputs should lift slightly on focus with a soft glow using `box-shadow`
- **Navigation Links**: Add animated underline from center using `::after` pseudo-elements
- **Dropdown Menus**: Implement slide-down animation with `opacity` and `transform` for smooth appearance

### Visual Enhancements
- Use `box-shadow` for depth: light shadows at rest, deeper on hover/focus
- Implement smooth color transitions on all interactive elements
- Add loading states with spinners or skeleton screens for async operations
- Use the NEBA blue color (`--neba-blue-700`) for primary actions and accents
- Maintain consistent border-radius using CSS custom properties

### Component Patterns
- **Tables**: Add subtle row hover effects with background tint and slight scale (`transform: scale(1.01)`)
- **Modals**: Use backdrop blur and smooth fade-in animations
- **Alerts/Notifications**: Slide in from appropriate direction with bounce-free easing
- **Search Inputs**: Expand width on focus, integrate icons inside input boundaries
- **Mobile Menus**: Use push-down behavior instead of overlay for better UX

### Performance Considerations
- Use CSS transforms and opacity for animations (GPU-accelerated)
- Implement `transition: all` sparingly; specify exact properties when possible
- Add `will-change` only when necessary and remove after animation completes
- Use `{ passive: true }` for scroll event listeners
- Debounce/throttle expensive operations like search inputs

### Accessibility
- Ensure all interactive elements have visible focus states
- Use `aria-label` and `aria-expanded` for navigation toggles
- Maintain color contrast ratios for WCAG compliance (minimum 4.5:1 for normal text, 3:1 for large text)
- Test keyboard navigation for all interactive features
- Provide alternative text for images and icons

**ARIA Attributes - Required for Interactive Elements:**
- Navigation menus: `aria-label="Main navigation"` on `<nav>`
- Dropdown menus: `aria-haspopup="true"`, `aria-expanded="false/true"` on trigger elements
- Dropdown content: `role="menu"` on container, `role="menuitem"` on links
- Mobile menu toggle: `aria-label`, `aria-expanded`, `aria-controls` attributes
- Search inputs: Proper `<label>` elements (can be `.sr-only`) and `id` association
- Buttons: `aria-label` when text content is not descriptive (e.g., icon-only buttons)
- Error messages: `role="alert"` and `aria-live="assertive"` for critical errors
- Loading states: `aria-busy="true"` and `aria-live="polite"` for status updates
- Modals: `role="dialog"`, `aria-modal="true"`, `aria-labelledby`, `aria-describedby`

**Keyboard Support - Required Patterns:**
- All interactive elements must be keyboard accessible (Tab, Enter, Space)
- Dropdowns: Open with Enter/Space, close with Escape
- Mobile menu: Close with Escape key
- Modal dialogs: Focus trap, close with Escape, return focus on close
- Skip links: "Skip to main content" link as first focusable element
- Tab order must follow logical reading order
- No keyboard traps (users can always Tab away)

**Focus Management:**
- Always show visible focus indicators (2px solid outline with offset)
- Focus states should be more prominent than hover states
- When opening modals/dropdowns, move focus appropriately
- When closing, return focus to trigger element
- Use `:focus-visible` for keyboard-only focus indicators where appropriate

**Screen Reader Support:**
- Use semantic HTML elements (`<nav>`, `<main>`, `<footer>`, `<article>`, `<section>`)
- Provide landmarks with `aria-label` when multiple of same type exist
- Use `.sr-only` class for screen-reader-only content
- Ensure all images have descriptive `alt` text (empty `alt=""` for decorative)
- Button text must describe the action, not just "Click here"
- Use `aria-live` regions for dynamic content updates

**Reduced Motion Support:**
- Respect `prefers-reduced-motion` media query
- Disable or significantly reduce animations when enabled
- Keep essential functionality without motion dependency
- Use instant state changes instead of transitions

**Color and Contrast:**
- Never rely on color alone to convey information
- Maintain WCAG AA contrast ratios (4.5:1 text, 3:1 large text, 3:1 UI components)
- Test with color blindness simulators
- Provide visual indicators beyond color (icons, text, patterns)

**Form Accessibility:**
- All inputs must have associated `<label>` elements
- Use `autocomplete` attributes for common fields
- Provide inline validation feedback with `aria-invalid` and `aria-describedby`
- Group related inputs with `<fieldset>` and `<legend>`
- Mark required fields with `required` attribute and visual indicator

**Touch Target Sizing:**
- Minimum 44x44 pixels for touch targets (buttons, links, form controls)
- Adequate spacing between interactive elements (8px minimum)
- Larger targets for primary actions

### CSS Organization
- Use CSS custom properties (CSS variables) for theming and consistency
- Follow the established naming convention: `neba-*` prefix for all custom classes
- Keep component-specific styles in `.razor.css` files
- Use Tailwind utility classes for layout and spacing
- Maintain the established design system in `neba_theme.css`

### Anti-Patterns to Avoid
- Don't use `!important` unless absolutely necessary
- Avoid inline styles except for dynamic values from component state
- Don't create overly complex animation sequences
- Avoid auto-playing animations or videos
- Don't use deprecated CSS properties or vendor prefixes for modern browsers
- Never block the main thread with JavaScript animations

## Naming Conventions

- Follow PascalCase for component names, method names, and public members.
- Use camelCase for private fields and local variables.
- Prefix interface names with "I" (e.g., IUserService).

## Blazor and .NET Specific Guidelines

- Utilize Blazor's built-in features for component lifecycle (e.g., OnInitializedAsync, OnParametersSetAsync).
- Use data binding effectively with @bind.
- Leverage Dependency Injection for services in Blazor.
- Structure Blazor components and services following Separation of Concerns.
- Always use the latest version C#, currently C# 14 features like record types, pattern matching, and global usings.

## Error Handling and Validation

- Implement proper error handling for Blazor pages and API calls.
- Use logging for error tracking in the backend and consider capturing UI-level errors in Blazor with tools like ErrorBoundary.
- Implement validation using FluentValidation or DataAnnotations in forms.

## Blazor API and Performance Optimization

- Utilize Blazor server-side or WebAssembly optimally based on the project requirements.
- Use asynchronous methods (async/await) for API calls or UI actions that could block the main thread.
- Optimize Razor components by reducing unnecessary renders and using StateHasChanged() efficiently.
- Minimize the component render tree by avoiding re-renders unless necessary, using ShouldRender() where appropriate.
- Use EventCallbacks for handling user interactions efficiently, passing only minimal data when triggering events.

## Caching Strategies

- Implement in-memory caching for frequently used data, especially for Blazor Server apps. Use IMemoryCache for lightweight caching solutions.
- For Blazor WebAssembly, utilize localStorage or sessionStorage to cache application state between user sessions.
- Consider Distributed Cache strategies (like Redis or SQL Server Cache) for larger applications that need shared state across multiple users or clients.
- Cache API calls by storing responses to avoid redundant calls when data is unlikely to change, thus improving the user experience.

## State Management Libraries

- Use Blazor's built-in Cascading Parameters and EventCallbacks for basic state sharing across components.
- Implement advanced state management solutions using libraries like Fluxor or BlazorState when the application grows in complexity.
- For client-side state persistence in Blazor WebAssembly, consider using Blazored.LocalStorage or Blazored.SessionStorage to maintain state between page reloads.
- For server-side Blazor, use Scoped Services and the StateContainer pattern to manage state within user sessions while minimizing re-renders.

## API Design and Integration

- Use HttpClient or other appropriate services to communicate with external APIs or your own backend.
- Implement error handling for API calls using try-catch and provide proper user feedback in the UI.

## Testing and Debugging in Visual Studio

- All unit testing and integration testing should be done in Visual Studio Enterprise.
- Test Blazor components and services using xUnit, NUnit, or MSTest.
- Use Moq or NSubstitute for mocking dependencies during tests.
- Debug Blazor UI issues using browser developer tools and Visual Studio's debugging tools for backend and server-side issues.
- For performance profiling and optimization, rely on Visual Studio's diagnostics tools.
- This repository uses xUnit; prefer xUnit for consistency.

## Security and Authentication

- Implement Authentication and Authorization in the Blazor app where necessary using ASP.NET Identity or JWT tokens for API authentication.
- Use HTTPS for all web communication and ensure proper CORS policies are implemented.

## API Documentation and Swagger

- Use Swagger/OpenAPI for API documentation for your backend API services.
- Ensure XML documentation for models and API methods for enhancing Swagger documentation.
