# Playwright Testing Instructions

This document defines how Playwright is used within the NEBA Management System to validate browser-driven, responsive, and workflow-based UI behavior. Playwright tests focus on behaviors that require a *real rendering engine*, real CSS execution, and actual browser eventing.

---

# 1. Purpose

Playwright provides full-browser end-to-end testing for validating:

- responsive layout
- multi-step workflows
- cross-browser rendering differences
- real input interactions
- overflow, scroll, and viewport mechanics
- toast and modal behavior
- navigation and routing

Playwright is the only layer that validates **actual rendered UI behavior**. It complements bUnit and xUnit by focusing strictly on what requires a browser.

---

# 2. What Playwright Is Responsible For

## 2.1 Responsive Layout & Breakpoints

Playwright owns all testing of:

- viewport-dependent layout changes
- menu bar collapse into hamburger
- prevention of horizontal scroll overflow
- toast message placement (desktop vs mobile)
- real CSS-based stacking, alignment, and justification
- grid/flex reflows at breakpoints

These behaviors depend on CSS, real layout engines, and viewport resizing.

---

## 2.2 Interaction & UX Behavior

Playwright validates behaviors that require actual browser event execution:

- hover, focus, blur
- pointer events (mouse, touch)
- keyboard navigation and tab order
- opening/closing modals, drawers, menus
- toast animations and timing behavior

These interactions cannot be reliably simulated in bUnit or pure unit tests.

---

## 2.3 Workflow Scenarios (End-to-End)

Playwright tests major functional flows:

- user login/logout
- tournament creation
- member registration
- score entry and updates
- payout workflows
- browsing across routes

Workflow tests verify integration between the UI, domain services, and backend APIs.

---

## 2.4 Overflow & Scrolling Rules

Playwright checks:

- no horizontal scroll at any viewport size
- content respecting layout boundaries
- correct scroll behavior under modals or drawers
- sticky navigation/header behavior

These require measurement of real rendered sizes.

---

## 2.5 Cross-Browser Verification

Playwright supports testing in:

- Chromium
- Firefox
- WebKit (Safari engine)

This ensures UI correctness across engines.

---

# 3. What Playwright Should *Not* Test

Avoid:

- Blazor component logic
- validation logic already covered by bUnit
- markup-only conditions that do not affect layout
- permutation-heavy component states
- UI adapter/domain logic already tested by xUnit

Playwright is slower and should not be used for logic tests.

---

# 4. Test Organization

Recommended structure:

```
/tests/NEBA.UI.BrowserTests
  /Responsive
  /Navigation
  /Scenarios
  /Workflows
  /TestUtils
```

### Guidelines:

- Organize by **scenario** or **behavior**, not by component.
- Tests must remain scenario-focused and human-readable.
- Each test should validate one cohesive workflow or layout behavior.

---

# 5. General Testing Rules

### 5.1 Use data-testid Selectors  
Selectors must be stable, deterministic, and explicit.

### 5.2 Keep Tests Deterministic  
Avoid timing assumptions; rely on automatic waiting where possible.

### 5.3 One Assertion Cluster per Scenario  
A Playwright test must represent a single user story or responsive behavior.

### 5.4 Always Validate for No-Overflow  
Any layout-sensitive page should include a “no horizontal scroll” check.

### 5.5 Validate Mobile and Desktop Behavior  
Responsive tests must cover:

- desktop baseline
- mobile baseline
- resize transitions where relevant

---

# 6. CI and Execution Strategy

### 6.1 Per-PR Coverage
PRs run:
- Chromium tests
- Desktop viewport

### 6.2 Nightly Coverage
Nightly runs include:

- Firefox
- WebKit
- Mobile emulation
- Full workflow scenarios

### 6.3 Artifact Retention
Failures include:

- video
- trace viewer
- console logs
- network logs

Artifacts should be reviewed before merging fixes.

---

# 7. Future Enhancements

- visual regression snapshots
- broader device matrix (iPhone, Android)
- performance profiling via trace analysis
- automated accessibility audit integration

---

Playwright is a critical layer for validating real-world NEBA UI behavior. It ensures that responsive layouts, workflows, and browser-driven interactions behave correctly across devices and engines.
