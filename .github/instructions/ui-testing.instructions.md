# UI Testing Instructions

This document defines how UI testing is structured within the NEBA Management System, what each testing tool is responsible for, and how we ensure reliability, speed, and correctness across the application’s UI layer.

## 1. Purpose of This Document

NEBA’s UI includes responsive layouts, workflow-heavy pages, modal and toast systems, and reusable Blazor components. This document establishes a unified approach for testing these behaviors in a way that is:

- deterministic
- fast
- maintainable
- aligned with Clean Architecture and DDD principles

The goal is not to maximize the number of UI tests but to put the right tests at the right layer using the appropriate tool.

## 2. Testing Philosophy

UI testing follows a layered approach:

- Playwright runs full-browser, full-stack workflows to validate behavior that depends on the rendering engine, CSS rules, or multi-step user interaction.
- bUnit validates component semantics, branching logic, and permutations without touching browser-rendered layout.
- xUnit ensures domain rules and adapters to the UI layer behave correctly.

We avoid:
- testing the same logic in two layers
- testing layout in non-browser environments
- over-reliance on slow end-to-end tests
- brittle tests that assume specific HTML structures unless explicitly required by component contracts

## 3. Tools Overview

### 3.1 Playwright

Purpose: Browser-driven end-to-end testing with real rendering engines (Chromium, Firefox, WebKit).

Technology: TypeScript + npm, located in `/tests/browser`

Strengths:
- Validates responsive behavior, breakpoints, CSS-driven layout changes
- Tests workflows across navigation, modals, forms, and multi-step interactions
- Captures real scroll, hover, click, keyboard, and mobile-specific mechanics
- Tracing and video capture simplify debugging flakiness
- Auto-waiting for elements reduces flaky tests

Limitations:
- Slower than component tests
- Unsuitable for component-level logic or permutations
- Should not test logic that belongs in domain or Blazor component layers

### 3.2 bUnit

Purpose: Fast, deterministic Blazor component unit tests.

Strengths:
- Validates component branching logic and state transitions
- Tests input validation, event callbacks, parameter wiring
- Handles permutations quickly
- Fully isolated: no browser, no CSS, no layout

Limitations:
- Cannot test responsive UI
- Cannot measure layout, position, scroll, overflow
- Not suitable for real-world workflows or integration behavior

### 3.3 xUnit + Shouldly (Context Only)

Used for:
- domain logic
- UI adapter logic
- invariant enforcement

## 4. Playwright Test Coverage (What We Test With Playwright & Why)

Playwright owns anything requiring a real browser engine, including:

### Responsive Layout and Breakpoints

- Navbar collapse behavior
- Toast position changes (desktop vs mobile)
- Grid/flex reflow
- Handling of viewport shrinking/expanding

### Overflow and Scroll Behavior

- Ensuring no horizontal scrolling
- Ensuring body content respects layout constraints
- Sticky header behavior
- Modal scroll locking

### Toast System Behavior

- Placement based on message type
- Stacking rules
- Animation and timing interactions
- Interaction with viewport changes

### Menu Bar Behavior

- Automatic collapse into hamburger
- Hamburger alignment
- Stability across breakpoints
- No layout jitter or clipped content

### Full Functional Workflows

- Login
- Tournament creation
- Member registration
- Score entry
- Payment flows

### Cross-Browser Rendering Differences

- WebKit, Firefox, Chromium
- Mobile emulation

### Interaction and UX

- Keyboard navigation
- Hover, focus, blur behavior
- Tooltip visibility
- Drawer opening/closing

## 5. bUnit Test Coverage (What We Test With bUnit & Why)

bUnit owns component-level behavior, including:

### Component Branching Logic

- showing/hiding elements
- conditional rendering
- parameter-driven variations

### Validation Logic

- field-by-field validation
- rules for disabled/enabled states
- error/help message semantics

### Component Contracts

- ensuring components render required markup
- verifying outputs for given inputs
- enforcing reusable component standards

### Event Callback Behavior

- button clicks triggering handlers
- cascading parameters working as intended
- child → parent event communication

### Service Interaction (Within Components)

- verifying mock-based interactions
- injecting service dependencies
- ensuring components call services correctly

## 6. What Not to Test

Avoid:

- Using Playwright for component permutations
- Using bUnit to test responsive layout
- Duplicating tests across Playwright and bUnit
- Testing CSS class implementation details unless contractually required
- Writing Playwright tests for button-level logic already tested in bUnit
- Mocking browser behavior in Playwright tests — test the real behavior

## 7. Test Folder Structure & Organization

/tests
  /browser                    # Playwright E2E tests (TypeScript)
    /scenarios
    /responsive
    /navigation
    /workflows
    /utils

  /Neba.ComponentTests        # bUnit component tests (C#)
    /Components
      /Shared
      /Navigation
      /Forms

## 8. Test Review Guidelines

- Prefer bUnit for logic and permutations
- Use Playwright only when browser behavior is essential
- Avoid timing-dependent Playwright tests unless necessary
- Ensure responsive coverage for any layout-sensitive change
- Validate accessibility behaviors (tab order, focus management)
- Require deterministic selectors (data-testid > CSS selectors)
- **REQUIRED: All bUnit tests must include DisplayName attributes** on `[Fact]` and `[Theory]` attributes that clearly describe what is being tested

## 9. CI Integration Strategy (High Level)

- Playwright tests run in a dedicated job with Node.js and browser dependencies
- Tests are executed via `npx playwright test` in the `/tests/browser` directory
- bUnit tests run in the main .NET test pipeline
- Playwright produces trace/video artifacts on failure
- Tests parallelize per scenario
- Mobile and cross-browser runs are nightly, not per PR

## 10. Future Enhancements

- Visual regression snapshots
- Device-specific scenario testing (iPhone, iPad, Android)
- Performance profiling via Playwright tracing
- Expansion to full accessibility audits (axe-core integration)
