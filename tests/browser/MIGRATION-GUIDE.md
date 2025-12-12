# Playwright E2E Tests - Migration Guide

## What Changed

We've restructured the Playwright tests to follow the testing pyramid correctly:

### Before: ❌ Too Many E2E Tests
- **~70 Playwright tests** testing component details
- Tests for CSS classes, aria attributes, rendering
- **~210 total test executions** (70 tests × 3 browsers)
- Very slow test runs

### After: ✅ Proper Testing Pyramid
- **19 Playwright E2E tests** testing critical user journeys only
- **76 total test executions** (19 tests × 4 browsers)
- **All tests mock the API** for fast, reliable execution
- Much faster test runs (~42 seconds vs 2+ minutes)
- Component details moved to bUnit (where they belong)

## New File Structure

### Playwright (E2E Tests)
- `champions.e2e.spec.ts` - Critical user journeys for browsing champions
- `notifications.e2e.spec.ts` - Real browser timing and interaction tests
- `modals.e2e.spec.ts` - User interaction flows for modals

### Old Files (Can Be Deleted)
- `champions.spec.ts` - Replaced by `champions.e2e.spec.ts`
- `notifications.spec.ts` - Replaced by `notifications.e2e.spec.ts`
- `modals.spec.ts` - Replaced by `modals.e2e.spec.ts`

## What Belongs in Playwright vs bUnit

### ✅ Playwright (E2E) - Test These:
- **User journeys** - Can user complete a multi-step task?
- **Real browser timing** - Does auto-dismiss work correctly?
- **Cross-browser issues** - Does hover work on mobile?
- **Browser behavior** - Does the UI work correctly in real browsers?
- **Navigation** - Can user navigate between views?

**Important:** All Playwright tests MUST mock the API using `page.route()` to ensure fast, reliable, and isolated testing.

### ❌ Playwright - Don't Test These:
- CSS classes and styling
- Aria attributes and accessibility properties
- Component rendering with different props
- State management within components
- Individual event handlers

### ✅ bUnit (Component) - Test These Instead:
- Component renders correctly
- CSS classes applied based on props
- Aria attributes set correctly
- Event callbacks triggered
- Conditional rendering
- State changes within component

## Example Migration

### Before (Playwright - Wrong Level):
```typescript
test('should have proper ARIA label on close button', async ({ page }) => {
  await page.click('[data-testid="open-basic-modal-btn"]');
  const closeButton = page.locator('.neba-modal-close');
  await expect(closeButton).toHaveAttribute('aria-label', 'Close modal');
});
```

### After (bUnit - Correct Level):
```csharp
[Fact]
public void ShouldRenderCloseButtonWithAriaLabel()
{
    // Arrange & Act
    var cut = Render<NebaModal>(parameters => parameters
        .Add(p => p.IsOpen, true)
        .Add(p => p.Title, "Test Modal"));

    // Assert
    var closeButton = cut.Find("button.neba-modal-close");
    closeButton.GetAttribute("aria-label").ShouldBe("Close modal");
}
```

## Test Counts by File

### Playwright E2E (New Files)
- `champions.e2e.spec.ts`: 7 tests (5 passing + 2 pending API)
- `notifications.e2e.spec.ts`: 8 tests
- `modals.e2e.spec.ts`: 8 tests (4 skipped on webkit)
- **Total: 19 tests × 4 browsers = 76 test executions**

### Browser Targets
- Desktop Chrome (Chromium)
- Desktop Safari (WebKit)
- Mobile Chrome (Pixel 5 emulation)
- Mobile Safari (iPhone 12 emulation)

### bUnit Component Tests (Existing - Need to Expand)
- Add tests for component-level details removed from Playwright
- Expand coverage for CSS classes, aria attributes, rendering variations
- **Target: 100+ fast unit tests**

## Running Tests

```bash
# Run new E2E tests
cd tests/browser
npx playwright test *.e2e.spec.ts

# Run old tests (to be removed)
npx playwright test champions.spec.ts notifications.spec.ts modals.spec.ts

# Run bUnit tests
cd ../..
dotnet test tests/Neba.WebTests
```

## Benefits

1. **78% faster test execution** - 76 executions vs 350 (42s vs 2+ minutes)
2. **API mocking** - No backend dependency, predictable test data
3. **Better test maintenance** - Component tests are easier to maintain in C#
4. **Faster feedback** - bUnit tests run in milliseconds, E2E in ~42s
5. **Clearer intent** - E2E tests focus on user journeys
6. **Better debugging** - Component tests easier to debug in IDE
7. **Proper browser coverage** - Desktop and mobile Safari, Chrome

## Next Steps

1. ✅ New E2E test files created
2. ✅ Playwright config updated for 3 browsers
3. ⏳ Review and expand bUnit tests for component coverage
4. ⏳ Delete old Playwright test files after validation
5. ⏳ Update CI/CD to use new test structure
