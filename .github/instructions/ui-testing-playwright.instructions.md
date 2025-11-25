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
/tests/browser
  /responsive
  /navigation
  /scenarios
  /workflows
  /utils
```

### Guidelines:

- Organize by **scenario** or **behavior**, not by component.
- Tests must remain scenario-focused and human-readable.
- Each test should validate one cohesive workflow or layout behavior.
- Use lowercase folder names following TypeScript/JavaScript conventions.

---

# 5. General Testing Rules

### 5.1 Use Playwright's Built-in Assertions
All browser tests use Playwright's built-in `expect` assertions for readable, auto-waiting behavior:

```typescript
// Good - Playwright expect assertions
await expect(element).toBeVisible();
await expect(page.locator('[data-testid="items"]')).toHaveCount(3);
await expect(page).toHaveURL(/.*home/);
await expect(element).toHaveAttribute('aria-expanded', 'true');

// Also good - for simple checks
expect(count).toBe(3);
expect(url).toContain('/home');
```

**Key Patterns:**
- Use `await expect()` for element and page assertions (auto-waits for conditions)
- Use regular `expect()` for simple value comparisons
- Provide descriptive test names and organize with `test.describe()` blocks
- Use `toBeVisible()`, `toHaveText()`, `toHaveCount()`, etc. for element assertions

### 5.2 Use data-testid Selectors
Selectors must be stable, deterministic, and explicit.

### 5.3 Keep Tests Deterministic
Avoid timing assumptions; rely on automatic waiting where possible.

### 5.4 One Assertion Cluster per Scenario
A Playwright test must represent a single user story or responsive behavior.

### 5.5 Always Validate for No-Overflow
Any layout-sensitive page should include a "no horizontal scroll" check.

### 5.6 Validate Mobile and Desktop Behavior
Responsive tests must cover:

- desktop baseline
- mobile baseline
- resize transitions where relevant

---

# 6. Running Playwright Tests

Playwright tests are written in TypeScript and run via npm/npx commands.

### 6.1 Setup

From the repository root:

```bash
cd tests/browser
npm install
npx playwright install  # Install browser binaries
```

### 6.2 Running Tests Locally

```bash
# Run all tests
npx playwright test

# Run tests in headed mode (see browser)
npx playwright test --headed

# Run specific test file
npx playwright test responsive/layout.spec.ts

# Run tests in specific browser
npx playwright test --project=chromium

# Run tests in debug mode
npx playwright test --debug

# Run tests with UI mode (interactive)
npx playwright test --ui
```

### 6.3 Viewing Test Results

```bash
# Open HTML report
npx playwright show-report

# Open trace viewer for failed tests
npx playwright show-trace
```

### 6.4 Test File Structure

All test files should follow the pattern `*.spec.ts`:

```typescript
import { test, expect } from '@playwright/test';

test.describe('Feature Name', () => {
  test('should do something specific', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('[data-testid="element"]')).toBeVisible();
  });
});
```

---

# 7. CI and Execution Strategy

### 7.1 Per-PR Coverage
PRs run:
- Chromium tests
- Desktop viewport

### 7.2 Nightly Coverage
Nightly runs include:

- Firefox
- WebKit
- Mobile emulation
- Full workflow scenarios

### 7.3 Artifact Retention
Failures include:

- video
- trace viewer
- console logs
- network logs

Artifacts should be reviewed before merging fixes.

---

# 8. Future Enhancements

- visual regression snapshots
- broader device matrix (iPhone, Android)
- performance profiling via trace analysis
- automated accessibility audit integration

---

Playwright is a critical layer for validating real-world NEBA UI behavior. It ensures that responsive layouts, workflows, and browser-driven interactions behave correctly across devices and engines.
