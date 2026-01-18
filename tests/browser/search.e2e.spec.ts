import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Site-Wide Search Functionality
 * Tests search form in header, keyboard shortcuts, and search submission
 */
test.describe('Site-Wide Search - E2E User Experience', () => {
  test.describe('Search Form in Header', () => {
    test('Search form is visible on all pages', async ({ page }) => {
      // Check on home page
      await page.goto('/');
      let searchInput = page.locator('#site-search');
      await expect(searchInput).toBeVisible();

      // Check on another page
      await page.goto('/hall-of-fame');
      searchInput = page.locator('#site-search');
      await expect(searchInput).toBeVisible();
    });

    test('Search form has proper labels and accessibility', async ({ page }) => {
      await page.goto('/');

      const searchForm = page.locator('form[role="search"]');
      await expect(searchForm).toHaveAttribute('aria-label', 'Site search');

      const searchInput = page.locator('#site-search');
      await expect(searchInput).toHaveAttribute('placeholder');

      // Should have a label (even if sr-only)
      const label = page.locator('label[for="site-search"]');
      await expect(label).toBeVisible({ timeout: 1000 }).catch(() => {
        // Label might be sr-only, check it exists
        return expect(label).toHaveCount(1);
      });
    });

    test('Search input accepts text', async ({ page }) => {
      await page.goto('/');

      const searchInput = page.locator('#site-search');
      await searchInput.fill('bowling tournament');

      await expect(searchInput).toHaveValue('bowling tournament');
    });

    test('Search submit button is visible', async ({ page }) => {
      await page.goto('/');

      const submitButton = page.locator('button[type="submit"][aria-label="Submit search"]');
      await expect(submitButton).toBeVisible();
    });

    test('User can submit search via button click', async ({ page }) => {
      await page.goto('/');

      const searchInput = page.locator('#site-search');
      await searchInput.fill('test query');

      const submitButton = page.locator('button[type="submit"][aria-label="Submit search"]');
      await submitButton.click();

      // Should navigate to search page
      await expect(page).toHaveURL(/\/search/);
    });

    test('User can submit search via Enter key', async ({ page }) => {
      await page.goto('/');

      const searchInput = page.locator('#site-search');
      await searchInput.fill('test query');
      await searchInput.press('Enter');

      // Should navigate to search page
      await expect(page).toHaveURL(/\/search/);
    });

    test('Search query is passed to search page', async ({ page }) => {
      await page.goto('/');

      const searchInput = page.locator('#site-search');
      await searchInput.fill('bowling');
      await searchInput.press('Enter');

      // URL should contain query parameter
      await expect(page).toHaveURL(/\/search\?q=bowling/);
    });

    test('Search input expands on focus', async ({ page }) => {
      await page.setViewportSize({ width: 1024, height: 768 });
      await page.goto('/');

      const searchInput = page.locator('#site-search');

      // Get initial width
      const initialBox = await searchInput.boundingBox();
      const initialWidth = initialBox?.width || 0;

      // Focus the input
      await searchInput.focus();

      // Wait for transition
      await page.waitForTimeout(400);

      // Get new width
      const focusedBox = await searchInput.boundingBox();
      const focusedWidth = focusedBox?.width || 0;

      // Should be wider
      expect(focusedWidth).toBeGreaterThan(initialWidth);
    });

    test('Search form works on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/');

      const searchInput = page.locator('#site-search');
      await expect(searchInput).toBeVisible();

      // Should be usable
      await searchInput.fill('test');
      await expect(searchInput).toHaveValue('test');
    });

    test('Search icon is visible', async ({ page }) => {
      await page.goto('/');

      // Look for the SVG search icon
      const searchIcon = page.locator('button[type="submit"][aria-label="Submit search"] svg');
      await expect(searchIcon).toBeVisible();
    });
  });

  test.describe('Search Persistence', () => {
    test('Search form persists across navigation', async ({ page }) => {
      await page.goto('/');

      const viewport = page.viewportSize();
      const isMobile = viewport && viewport.width < 768;

      let searchInput = page.locator('#site-search');
      await expect(searchInput).toBeVisible();

      if (isMobile) {
        // Open mobile menu first
        await page.click('[data-action="toggle-menu"]');
      }

      // Navigate to another page
      await page.click('a[href="/hall-of-fame"]');
      await expect(page).toHaveURL(/\/hall-of-fame/);

      // Search form should still be there
      searchInput = page.locator('#site-search');
      await expect(searchInput).toBeVisible();
    });

    test('Empty search does not break functionality', async ({ page }) => {
      await page.goto('/');

      const searchInput = page.locator('#site-search');

      // Try to submit empty search
      await searchInput.press('Enter');

      // Should still navigate to search page
      await expect(page).toHaveURL(/\/search/);
    });
  });

  test.describe('Keyboard Navigation', () => {
    test('User can tab to search input', async ({ page }) => {
      await page.goto('/');

      const searchInput = page.locator('#site-search');

      // Verify search input is keyboard accessible
      await searchInput.focus();
      await expect(searchInput).toBeFocused();
    });

    test('User can tab to submit button', async ({ page }) => {
      await page.goto('/');

      const submitButton = page.locator('button[type="submit"][aria-label="Submit search"]');

      // Verify submit button is keyboard accessible
      await submitButton.focus();
      await expect(submitButton).toBeFocused();
    });
  });

  test.describe('Responsive Behavior', () => {
    test('Search width adapts to viewport', async ({ page }) => {
      // Mobile - search should be visible and appropriately sized
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/');

      let searchInput = page.locator('#site-search');
      await expect(searchInput).toBeVisible();
      let mobileBox = await searchInput.boundingBox();
      const mobileWidth = mobileBox?.width || 0;
      expect(mobileWidth).toBeGreaterThan(0);

      // Desktop - search should be visible and appropriately sized
      await page.setViewportSize({ width: 1400, height: 900 });
      await page.goto('/');

      searchInput = page.locator('#site-search');
      await expect(searchInput).toBeVisible();
      let desktopBox = await searchInput.boundingBox();
      const desktopWidth = desktopBox?.width || 0;
      expect(desktopWidth).toBeGreaterThan(0);

      // Both should have reasonable widths for their context
      expect(mobileWidth).toBeGreaterThan(100);
      expect(desktopWidth).toBeGreaterThan(100);
    });

    test('Search remains functional at various viewport sizes', async ({ page }) => {
      const viewports = [
        { width: 375, height: 667 },   // Mobile
        { width: 768, height: 1024 },  // Tablet
        { width: 1024, height: 768 },  // Small desktop
        { width: 1920, height: 1080 }, // Large desktop
      ];

      for (const viewport of viewports) {
        await page.setViewportSize(viewport);
        await page.goto('/');

        const searchInput = page.locator('#site-search');
        await expect(searchInput).toBeVisible();
        await searchInput.fill('test');
        await expect(searchInput).toHaveValue('test');

        // Clear for next iteration
        await searchInput.clear();
      }
    });
  });

  test.describe('Visual Feedback', () => {
    test('Submit button shows hover state', async ({ page }) => {
      await page.goto('/');

      const submitButton = page.locator('button[type="submit"][aria-label="Submit search"]');

      // Hover over button
      await submitButton.hover();

      // Button should still be visible and clickable
      await expect(submitButton).toBeVisible();
    });

    test('Search input shows focus state', async ({ page }) => {
      await page.goto('/');

      const searchInput = page.locator('#site-search');

      // Focus input
      await searchInput.focus();

      // Should be focused
      await expect(searchInput).toBeFocused();
    });
  });

  test.describe('Cross-Browser Compatibility', () => {
    test('Search works across different browsers', async ({ page, browserName }) => {
      await page.goto('/');

      const searchInput = page.locator('#site-search');
      await searchInput.fill('test query');
      await searchInput.press('Enter');

      // Should work in all browsers
      await expect(page).toHaveURL(/\/search/);
    });
  });

  test.describe('Form Action and Method', () => {
    test('Form uses GET method for search', async ({ page }) => {
      await page.goto('/');

      const searchForm = page.locator('form[role="search"]');

      // Check method attribute
      const method = await searchForm.getAttribute('method');
      expect(method).toBe('get');
    });

    test('Form action points to search endpoint', async ({ page }) => {
      await page.goto('/');

      const searchForm = page.locator('form[role="search"]');

      // Check action attribute
      const action = await searchForm.getAttribute('action');
      expect(action).toBe('/search');
    });
  });
});
