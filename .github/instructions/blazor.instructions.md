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
- Maintain color contrast ratios for WCAG compliance
- Test keyboard navigation for all interactive features
- Provide alternative text for images and icons

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
