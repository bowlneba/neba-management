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

### 7.1 Per-PR Coverage (Fast Feedback)

PRs run Playwright tests with:
- **Browser**: Chromium only
- **Viewport**: Desktop (1280x720)
- **Purpose**: Fast feedback for common scenarios
- **Execution Time**: ~5-10 minutes

This configuration balances speed and coverage for rapid iteration.

### 7.2 Expanding to Multi-Browser Testing

To enable full cross-browser testing, update `.github/workflows/pr_application.yml`:

```yaml
strategy:
  fail-fast: false
  matrix:
    browser: [chromium, firefox, webkit]
```

**Recommendation**: Use multi-browser testing for:
- Nightly builds
- Release branches
- Pre-production validation

Reserve single-browser (chromium) testing for PRs to keep CI fast.

### 7.3 Mobile Testing

Mobile viewport tests run in all CI environments using explicit viewport sizing in tests:

```typescript
await page.setViewportSize({ width: 375, height: 667 });
```

For comprehensive mobile device emulation, configure Playwright projects in `playwright.config.ts`.

### 7.4 Artifact Retention

On test failure, CI uploads:
- **HTML Report**: Full test results with screenshots (30 days retention)
- **Trace Files**: Step-by-step execution trace with network logs (7 days retention)
- **Videos**: Recording of test execution (if configured)

Download artifacts from GitHub Actions to debug failures locally:

```bash
# Extract traces and view them
npx playwright show-trace path/to/trace.zip
```

### 7.5 Running Tests Locally to Match CI

To replicate PR CI environment:

```bash
cd tests/browser
npm ci
npx playwright install chromium
npx playwright test --project=chromium
```

To replicate nightly/full testing:

```bash
npx playwright install
npx playwright test --project=chromium --project=firefox --project=webkit
```

For detailed local testing instructions, see [tests/browser/README.md](../../tests/browser/README.md).

---

# 8. Future Enhancements

- Visual regression snapshots using Playwright's built-in screenshot comparison
- Broader device matrix (iPhone, Android devices) via Playwright device emulation
- Performance profiling via trace analysis and Lighthouse integration
- Automated accessibility audit integration (axe-core)
- Parallel test execution across multiple CI runners for faster feedback

---

# 9. Documentation and Resources

- **Local Testing Guide**: [tests/browser/README.md](../../tests/browser/README.md)
- **CI Configuration**: [.github/workflows/pr_application.yml](../../.github/workflows/pr_application.yml)
- **General UI Testing Strategy**: [.github/instructions/ui-testing.instructions.md](ui-testing.instructions.md)

---

Playwright is a critical layer for validating real-world NEBA UI behavior. It ensures that responsive layouts, workflows, and browser-driven interactions behave correctly across devices and engines.
