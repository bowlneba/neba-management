# Playwright Browser Tests

This directory contains Playwright-based end-to-end tests for the NEBA Management System. These tests validate browser-driven behavior, responsive layouts, and full user workflows.

## Prerequisites

- Node.js LTS or later
- .NET 10.0 SDK
- Playwright browsers (installed automatically)

## Setup

From the repository root:

```bash
cd tests/browser
npm install
npx playwright install  # Install browser binaries
```

## Running Tests Locally

### Run all tests (default: Chromium)

```bash
npx playwright test
```

### Run tests in headed mode (see the browser)

```bash
npx playwright test --headed
```

### Run specific test file

```bash
npx playwright test notifications.spec.ts
```

### Run tests in a specific browser

```bash
# Chromium (default)
npx playwright test --project=chromium

# Firefox
npx playwright test --project=firefox

# WebKit (Safari engine)
npx playwright test --project=webkit
```

### Run tests for all browsers

```bash
npx playwright test --project=chromium --project=firefox --project=webkit
```

### Run tests in debug mode

```bash
npx playwright test --debug
```

### Run tests with UI mode (interactive)

```bash
npx playwright test --ui
```

### Run tests on mobile viewports

Tests with explicit mobile viewport calls (like `setViewportSize({ width: 375, height: 667 })`) will automatically test mobile behavior. You can also use Playwright's device emulation:

```bash
npx playwright test --project="Mobile Chrome"
npx playwright test --project="Mobile Safari"
```

## Viewing Test Results

### Open HTML report

After running tests:

```bash
npx playwright show-report
```

### Open trace viewer for failed tests

```bash
npx playwright show-trace
```

### View traces from a specific test run

```bash
npx playwright show-trace test-results/<test-name>/trace.zip
```

## Test Organization

```
tests/browser/
├── notifications.spec.ts    # Notification component tests
├── playwright.config.ts     # Playwright configuration
├── package.json            # Node dependencies
└── README.md              # This file
```

## CI/CD Integration

### Pull Request Testing (Fast)

PRs run tests in:
- **Browser**: Chromium only
- **Viewport**: Desktop (1280x720)
- **Purpose**: Fast feedback for common scenarios

To match PR testing locally:

```bash
npx playwright test --project=chromium
```

### Nightly Testing (Comprehensive)

Nightly builds run tests in:
- **Browsers**: Chromium, Firefox, WebKit
- **Viewports**: Desktop + Mobile emulation
- **Purpose**: Catch cross-browser and responsive issues

To match nightly testing locally:

```bash
npx playwright test --project=chromium --project=firefox --project=webkit
```

### Artifacts on Failure

When tests fail in CI:
- **HTML Report**: Full test results with screenshots
- **Trace Files**: Step-by-step execution trace with network logs
- **Screenshots**: Visual evidence of failures
- **Videos**: Recording of test execution (if configured)

Download artifacts from the GitHub Actions run to debug failures.

## Multi-Browser Testing

### Expanding CI to Full Browser Matrix

To enable multi-browser testing in CI, edit `.github/workflows/pr_application.yml`:

```yaml
strategy:
  fail-fast: false
  matrix:
    browser: [chromium, firefox, webkit]  # Add all browsers
```

**Note**: This increases CI time and cost. Consider:
- Using chromium for PRs
- Using all browsers for nightly or release branches

## Writing Tests

### Test Structure

```typescript
import { test, expect } from '@playwright/test';

test.describe('Feature Name', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/your-page');
  });

  test('should do something specific', async ({ page }) => {
    // Arrange
    const element = page.locator('[data-testid="element"]');

    // Act
    await element.click();

    // Assert
    await expect(element).toBeVisible();
  });
});
```

### Mobile Testing

```typescript
test('should work on mobile', async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 667 });
  await page.goto('/your-page');

  // Test mobile-specific behavior
});
```

### Best Practices

1. **Use Playwright's auto-waiting**: Use `expect().toBeHidden()` instead of `waitForTimeout()`
2. **Use data-testid selectors**: More stable than CSS classes
3. **Extract test data**: Use constants for reusable test data
4. **One scenario per test**: Keep tests focused and readable
5. **Test accessibility**: Verify ARIA attributes and roles
6. **Avoid timing assumptions**: Let Playwright handle waiting

## Test Data Configuration

Test timeouts and messages are centralized in each spec file:

```typescript
const TEST_DATA = {
  timeouts: {
    toastAutoDismiss: 4000,  // 4 seconds
    fadeOutAnimation: 300    // 0.3 seconds
  },
  messages: {
    error: 'This is an error message'
  }
};
```

Update these constants when component behavior changes.

## Troubleshooting

### Tests are flaky

- Ensure you're using Playwright's built-in waiting (`expect().toBeVisible()`)
- Avoid `waitForTimeout()` unless absolutely necessary
- Check for race conditions in async operations

### Browser not found

```bash
npx playwright install
```

### Tests fail only in CI

- Check viewport differences (CI uses 1280x720 by default)
- Review CI artifacts for traces and screenshots
- Run tests in headless mode locally: `npx playwright test`

### Slow test execution

- Run tests in parallel (enabled by default)
- Use `test.describe.configure({ mode: 'parallel' })` for independent tests
- Profile tests with `--trace on` to identify bottlenecks

## Resources

- [Playwright Documentation](https://playwright.dev/)
- [UI Testing Instructions](.github/instructions/ui-testing-playwright.instructions.md)
- [General UI Testing Strategy](.github/instructions/ui-testing.instructions.md)
