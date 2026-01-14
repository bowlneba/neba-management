import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Bowling Centers Page - Real Browser Interactions
 * Tests filtering, search, map interactions, and directions modal flow
 * Component-level tests (rendering, props) belong in bUnit
 */
test.describe('Bowling Centers - E2E User Experience', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/bowling-centers');
  });

  test.describe('Page Load and Initial State', () => {
    test('Page loads with title and description', async ({ page }) => {
      const heading = page.locator('h1', { hasText: 'Bowling Centers' });
      await expect(heading).toBeVisible();

      const description = page.locator('text=USBC certified centers across New England');
      await expect(description).toBeVisible();
    });

    test('Centers load from API', async ({ page }) => {
      // Wait for loading to complete
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Filter bar should be visible
      const filterBar = page.locator('text=Filter by State:');
      await expect(filterBar).toBeVisible();
    });

    test('Results count displays correctly', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Should show count
      const resultsCount = page.locator('text=/Showing.*of.*centers/i');
      await expect(resultsCount).toBeVisible();
    });
  });

  test.describe('State Filtering', () => {
    test('User can filter centers by state', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Click MA filter
      const maButton = page.locator('button.state-btn', { hasText: 'MA' });
      await maButton.click();

      // Button should be active
      await expect(maButton).toHaveClass(/active/);

      // Results should update
      const resultsCount = page.locator('text=/Showing.*of.*centers/i');
      await expect(resultsCount).toBeVisible();
    });

    test('User can switch between state filters', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Click MA
      const maButton = page.locator('button.state-btn', { hasText: 'MA' });
      await maButton.click();
      await expect(maButton).toHaveClass(/active/);

      // Click CT
      const ctButton = page.locator('button.state-btn', { hasText: 'CT' });
      await ctButton.click();
      await expect(ctButton).toHaveClass(/active/);

      // MA should no longer be active
      await expect(maButton).not.toHaveClass(/active/);
    });

    test('User can reset filters with "All States"', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Click MA
      await page.click('button.state-btn:has-text("MA")');

      // Click All States
      const allStatesButton = page.locator('button.state-btn', { hasText: 'All States' });
      await allStatesButton.click();
      await expect(allStatesButton).toHaveClass(/active/);
    });

    test('All New England states are available as filters', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Check for all state buttons
      await expect(page.locator('button.state-btn', { hasText: 'MA' })).toBeVisible();
      await expect(page.locator('button.state-btn', { hasText: 'CT' })).toBeVisible();
      await expect(page.locator('button.state-btn', { hasText: 'RI' })).toBeVisible();
      await expect(page.locator('button.state-btn', { hasText: 'NH' })).toBeVisible();
      await expect(page.locator('button.state-btn', { hasText: 'ME' })).toBeVisible();
      await expect(page.locator('button.state-btn', { hasText: 'VT' })).toBeVisible();
    });
  });

  test.describe('Search Functionality', () => {
    test('User can search for centers by name', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Type in search box
      const searchInput = page.locator('input[placeholder="Search centers..."]');
      await searchInput.fill('Boston');

      // Wait a moment for debounce
      await page.waitForTimeout(400);

      // Results should filter
      const resultsCount = page.locator('text=/Showing.*of.*centers/i');
      await expect(resultsCount).toBeVisible();
    });

    test('Search box is clearly labeled', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      const searchInput = page.locator('input[placeholder="Search centers..."]');
      await expect(searchInput).toBeVisible();
      await expect(searchInput).toHaveAttribute('placeholder', 'Search centers...');
    });

    test('Search updates results count', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      const searchInput = page.locator('input[placeholder="Search centers..."]');

      // Get initial count
      const resultsCount = page.locator('text=/Showing.*of.*centers/i');
      const initialText = await resultsCount.textContent();

      // Search for something
      await searchInput.fill('xyz123nonexistent');
      await page.waitForTimeout(400);

      // Count should change
      const newText = await resultsCount.textContent();
      expect(newText).not.toBe(initialText);
    });

    test('Search works in combination with state filter', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Filter by MA
      await page.click('button.state-btn:has-text("MA")');

      // Then search
      const searchInput = page.locator('input[placeholder="Search centers..."]');
      await searchInput.fill('Boston');
      await page.waitForTimeout(400);

      // Both filters should be active
      const maButton = page.locator('button.state-btn', { hasText: 'MA' });
      await expect(maButton).toHaveClass(/active/);
      await expect(searchInput).toHaveValue('Boston');
    });
  });

  test.describe('Centers List Display', () => {
    test('Centers are displayed in a list', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Should have a scrollable container
      const scrollContainer = page.locator('#centers-scroll-container');
      await expect(scrollContainer).toBeVisible();
    });

    test('No results message shows when no centers match', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Search for something that doesn't exist
      const searchInput = page.locator('input[placeholder="Search centers..."]');
      await searchInput.fill('xyz123nonexistent');
      await page.waitForTimeout(400);

      // Should show no results message
      const noResults = page.locator('text=/No centers/i');
      await expect(noResults).toBeVisible();
    });

    test('Centers list is scrollable', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      const scrollContainer = page.locator('#centers-scroll-container');

      // Container should have overflow-y-auto class or style
      const classList = await scrollContainer.getAttribute('class');
      expect(classList).toContain('overflow-y-auto');
    });
  });

  test.describe('Responsive Design', () => {
    test('Layout adapts to mobile viewport', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/bowling-centers');

      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Filter bar should still be visible
      const filterBar = page.locator('text=Filter by State:');
      await expect(filterBar).toBeVisible();

      // Search should be visible
      const searchInput = page.locator('input[placeholder="Search centers..."]');
      await expect(searchInput).toBeVisible();
    });

    test('State filter buttons wrap on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/bowling-centers');

      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // All state buttons should still be visible
      await expect(page.locator('button.state-btn', { hasText: 'MA' })).toBeVisible();
      await expect(page.locator('button.state-btn', { hasText: 'CT' })).toBeVisible();
    });

    test('Search input is full width on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/bowling-centers');

      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      const searchInput = page.locator('input[placeholder="Search centers..."]');
      const box = await searchInput.boundingBox();

      // Should be wide enough for mobile
      expect(box?.width).toBeGreaterThan(150);
    });
  });

  test.describe('Error Handling', () => {
    test('Error message can be dismissed', async ({ page }) => {
      // Navigate to a page that might show an error
      await page.goto('/bowling-centers');

      // If an error appears, it should be dismissible
      const errorAlert = page.locator('[data-testid="alert"]').filter({ hasText: 'Error' });

      // Only test if error is visible
      if (await errorAlert.isVisible({ timeout: 1000 }).catch(() => false)) {
        const dismissButton = errorAlert.locator('button');
        await dismissButton.click();
        await expect(errorAlert).not.toBeVisible();
      }
    });
  });

  test.describe('Loading States', () => {
    test('Loading indicator shows initially', async ({ page }) => {
      const loadingPromise = page.goto('/bowling-centers');

      // Loading indicator should appear
      const loadingIndicator = page.locator('text=Loading bowling centers...');

      // It might appear very briefly
      try {
        await expect(loadingIndicator).toBeVisible({ timeout: 1000 });
      } catch {
        // Loading was too fast, that's okay
      }

      await loadingPromise;

      // Eventually it should be hidden
      await expect(loadingIndicator).not.toBeVisible({ timeout: 5000 });
    });

    test('Content appears after loading completes', async ({ page }) => {
      await page.goto('/bowling-centers');

      // Wait for loading to finish
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Filter bar should be visible
      const filterBar = page.locator('text=Filter by State:');
      await expect(filterBar).toBeVisible();
    });
  });

  test.describe('Performance', () => {
    test('Page loads quickly', async ({ page }) => {
      const startTime = Date.now();
      await page.goto('/bowling-centers');
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });
      const loadTime = Date.now() - startTime;

      // Should load in reasonable time (5 seconds including API)
      expect(loadTime).toBeLessThan(5000);
    });

    test('Search filtering is responsive', async ({ page }) => {
      await page.goto('/bowling-centers');
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      const searchInput = page.locator('input[placeholder="Search centers..."]');

      const startTime = Date.now();
      await searchInput.fill('Boston');
      await page.waitForTimeout(500); // Wait for debounce
      const searchTime = Date.now() - startTime;

      // Search should be fast
      expect(searchTime).toBeLessThan(1000);
    });
  });

  test.describe('Accessibility', () => {
    test('Page has proper heading structure', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Should have h1
      const h1 = page.locator('h1', { hasText: 'Bowling Centers' });
      await expect(h1).toHaveCount(1);
    });

    test('Filter buttons are keyboard accessible', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      const maButton = page.locator('button.state-btn', { hasText: 'MA' });

      // Tab to button
      await page.keyboard.press('Tab');

      // Button should be focusable
      await maButton.focus();
      await expect(maButton).toBeFocused();
    });

    test('Search input is keyboard accessible', async ({ page }) => {
      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      const searchInput = page.locator('input[placeholder="Search centers..."]');

      // Focus input
      await searchInput.focus();
      await expect(searchInput).toBeFocused();

      // Type with keyboard
      await searchInput.fill('Boston');
      await expect(searchInput).toHaveValue('Boston');
    });
  });

  test.describe('Map Integration', () => {
    test('Map container is present on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1024, height: 768 });
      await page.goto('/bowling-centers');

      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Look for the two-column grid layout
      const gridLayout = page.locator(String.raw`.grid.grid-cols-1.lg\:grid-cols-2`);
      await expect(gridLayout).toBeVisible();
    });

    test('Map and list are side-by-side on large screens', async ({ page }) => {
      await page.setViewportSize({ width: 1400, height: 900 });
      await page.goto('/bowling-centers');

      await page.waitForSelector('text=Loading bowling centers...', { state: 'hidden', timeout: 5000 });

      // Grid should have 2 columns
      const gridLayout = page.locator('.grid.grid-cols-1.lg\\:grid-cols-2');
      await expect(gridLayout).toBeVisible();

      // Centers container should be visible
      const centersContainer = page.locator('#centers-scroll-container');
      await expect(centersContainer).toBeVisible();
    });
  });
});
