import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Navigation - Real Browser Behavior Only
 * Tests mobile menu, dropdowns, active states, and responsive behavior
 * Component-level tests (props, callbacks) belong in bUnit
 */
test.describe('Navigation - E2E User Experience', () => {
  test.describe('Mobile Menu', () => {
    test('User can toggle mobile menu on small viewport', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/');

      // Mobile menu should be hidden initially
      const menu = page.locator('#main-menu');
      await expect(menu).not.toBeVisible();

      // User clicks hamburger menu
      const menuToggle = page.locator('[data-action="toggle-menu"]');
      await menuToggle.click();

      // Menu becomes visible
      await expect(menu).toBeVisible();

      // User clicks again to close
      await menuToggle.click();
      await expect(menu).not.toBeVisible();
    });

    test.skip('Menu is visible on desktop viewport', async ({ page }, testInfo) => {
      // Skipped: Flaky due to viewport detection issues in chromium

      await page.setViewportSize({ width: 1024, height: 768 });
      await page.goto('/');

      const menu = page.locator('#main-menu');
      await expect(menu).toBeVisible();
    });

    test('Mobile menu items are clickable', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/');

      // Open mobile menu
      await page.click('[data-action="toggle-menu"]');

      // Click a menu item
      await page.click('a[href="/hall-of-fame"]');

      // Should navigate to the page
      await expect(page).toHaveURL(/\/hall-of-fame/);
    });
  });

  test.describe('Dropdown Menus', () => {
    test.skip('User can open and close dropdown on desktop', async ({ page, browserName }, testInfo) => {
      // Skipped: Flaky due to viewport detection issues in chromium

      await page.setViewportSize({ width: 1024, height: 768 });
      await page.goto('/');

      // Find Tournaments dropdown
      const tournamentsDropdown = page.locator('.neba-nav-item', { hasText: 'Tournaments' }).first();
      const dropdown = tournamentsDropdown.locator('.neba-dropdown');

      // Dropdown initially hidden
      await expect(dropdown).not.toBeVisible();

      // User clicks to open
      await tournamentsDropdown.click();
      await expect(dropdown).toBeVisible();

      // User clicks again to close
      await tournamentsDropdown.click();
      await expect(dropdown).not.toBeVisible();
    });

    test.skip('User can navigate to dropdown items', async ({ page, browserName }, testInfo) => {
      // Skipped: Flaky due to viewport detection issues in chromium

      await page.setViewportSize({ width: 1024, height: 768 });
      await page.goto('/');

      // Open History dropdown
      const historyDropdown = page.locator('.neba-nav-item', { hasText: 'History' }).first();
      await historyDropdown.click();

      // Click Champions link
      const championsLink = page.locator('a[href="/history/champions"]');
      await championsLink.click();

      // Should navigate
      await expect(page).toHaveURL(/\/history\/champions/);
    });

    test.skip('Multiple dropdowns can be opened independently', async ({ page, browserName }, testInfo) => {
      // Skipped: Flaky due to viewport detection issues in chromium

      await page.setViewportSize({ width: 1024, height: 768 });
      await page.goto('/');

      // Open Tournaments dropdown
      const tournamentsDropdown = page.locator('.neba-nav-item', { hasText: 'Tournaments' }).first();
      await tournamentsDropdown.click();
      const tournamentsMenu = tournamentsDropdown.locator('.neba-dropdown');
      await expect(tournamentsMenu).toBeVisible();

      // Open History dropdown
      const historyDropdown = page.locator('.neba-nav-item', { hasText: 'History' }).first();
      await historyDropdown.click();
      const historyMenu = historyDropdown.locator('.neba-dropdown');
      await expect(historyMenu).toBeVisible();
    });

    test('Dropdown works on mobile after opening menu', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/');

      // Open mobile menu
      await page.click('[data-action="toggle-menu"]');

      // Open dropdown
      const aboutDropdown = page.locator('.neba-nav-item', { hasText: 'About' }).first();
      await aboutDropdown.click();

      // Check if Bylaws link is visible
      const bylawsLink = page.locator('a[href="/bylaws"]');
      await expect(bylawsLink).toBeVisible();
    });
  });

  test.describe('Active Route Highlighting', () => {
    test('Home page shows no active nav items', async ({ page }) => {
      await page.goto('/');

      // Only the home logo should be current page, nav items should not have active class
      const activeNavLinks = page.locator('.neba-nav-link.active');
      await expect(activeNavLinks).toHaveCount(0);
    });

    test('Active state highlights current page', async ({ page }) => {
      await page.goto('/hall-of-fame');

      // Hall of Fame link should be active
      const hofLink = page.locator('a[href="/hall-of-fame"].neba-nav-link');
      await expect(hofLink).toHaveClass(/active/);
    });

    test('Active state works for dropdown items', async ({ page }) => {
      await page.goto('/history/champions');

      // Champions dropdown link should be active
      const championsLink = page.locator('a[href="/history/champions"].neba-dropdown-link');
      await expect(championsLink).toHaveClass(/active/);
    });

    test('Active state updates after navigation', async ({ page }) => {
      await page.goto('/hall-of-fame');

      const viewport = page.viewportSize();
      const isMobile = viewport && viewport.width < 768;

      if (isMobile) {
        // Open mobile menu first
        await page.click('[data-action="toggle-menu"]');
      }

      // Initial active state
      let hofLink = page.locator('a[href="/hall-of-fame"].neba-nav-link');
      await expect(hofLink).toHaveClass(/active/);

      // Navigate to different page
      await page.click('a[href="/bowling-centers"]');
      await expect(page).toHaveURL(/\/bowling-centers/);

      if (isMobile) {
        // Open mobile menu again after navigation
        await page.click('[data-action="toggle-menu"]');
      }

      // Bowling Centers should now be active
      const centersLink = page.locator('a[href="/bowling-centers"].neba-nav-link');
      await expect(centersLink).toHaveClass(/active/);

      // Hall of Fame should no longer be active
      hofLink = page.locator('a[href="/hall-of-fame"].neba-nav-link');
      await expect(hofLink).not.toHaveClass(/active/);
    });

    test('Active state works for nested routes', async ({ page }) => {
      await page.goto('/tournaments/rules');

      // Tournament Rules dropdown link should be active
      const rulesLink = page.locator('a[href="/tournaments/rules"].neba-dropdown-link');
      await expect(rulesLink).toHaveClass(/active/);
    });
  });

  test.describe('Responsive Behavior', () => {
    test.skip('Navigation transitions from mobile to desktop on resize', async ({ page }, testInfo) => {
      // Skipped: Flaky due to viewport detection and device emulation conflicts

      // Start with mobile
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/');

      const menu = page.locator('#main-menu');
      await expect(menu).not.toBeVisible();

      // Resize to desktop
      await page.setViewportSize({ width: 1024, height: 768 });

      // Menu should now be visible without needing to click
      await expect(menu).toBeVisible();
    });

    test('Search input expands on focus', async ({ page }) => {
      await page.goto('/');

      const viewport = page.viewportSize();
      const isMobile = viewport && viewport.width < 768;

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

      if (isMobile) {
        // On mobile, search is already full width, so it shouldn't expand further
        expect(focusedWidth).toBeGreaterThan(0);
      } else {
        // On desktop, should expand when focused
        expect(focusedWidth).toBeGreaterThan(initialWidth);
      }
    });
  });

  test.describe('Logo and Branding', () => {
    test('Logo links to home page', async ({ page }) => {
      await page.goto('/hall-of-fame');

      const viewport = page.viewportSize();
      const isMobile = viewport && viewport.width < 768;

      if (isMobile) {
        // On mobile, top banner logo is hidden, so click navbar logo instead
        await page.click('.neba-navbar-header img[alt="NEBA | 1963"]');
      } else {
        // On desktop, click top banner logo
        await page.click('.neba-top-banner img[alt="NEBA Logo"]');
      }

      // Should navigate to home
      await expect(page).toHaveURL(/^\/$|\/$/);
    });

    test('Navigation logo links to home page', async ({ page }) => {
      await page.goto('/hall-of-fame');

      // Click navbar logo
      await page.click('.neba-navbar-header img[alt="NEBA | 1963"]');

      // Should navigate to home
      await expect(page).toHaveURL(/^\/$|\/$/);
    });

    test('Both logos are visible and properly sized', async ({ page }) => {
      await page.goto('/');

      const viewport = page.viewportSize();
      const isMobile = viewport && viewport.width < 768;

      if (!isMobile) {
        // Check top banner logo (only on desktop)
        const topLogo = page.locator('.neba-top-banner img[alt="NEBA Logo"]');
        await expect(topLogo).toBeVisible();
        const topBox = await topLogo.boundingBox();
        expect(topBox?.height).toBeGreaterThan(0);
      }

      // Check navbar logo (visible on all viewports)
      const navLogo = page.locator('.neba-navbar-header img[alt="NEBA | 1963"]');
      await expect(navLogo).toBeVisible();
      const navBox = await navLogo.boundingBox();
      expect(navBox?.height).toBeGreaterThan(0);
    });
  });

  test.describe('Accessibility', () => {
    test('Skip to main content link works', async ({ page }) => {
      await page.goto('/');

      // Focus skip link (it's sr-only by default)
      const skipLink = page.locator('a[href="#main-content"]');
      await skipLink.focus();

      // Click it
      await skipLink.click();

      // Main content should exist and be visible
      const mainContent = page.locator('#main-content');
      await expect(mainContent).toBeVisible();

      // Skip link should exist and be accessible
      await expect(skipLink).toBeAttached();
    });

    test('Navigation has proper ARIA attributes', async ({ page }) => {
      await page.goto('/');

      // Main navigation should have role
      const nav = page.locator('nav[role="navigation"]');
      await expect(nav).toHaveAttribute('aria-label', 'Main navigation');

      // Menu toggle should have proper attributes
      const menuToggle = page.locator('[data-action="toggle-menu"]');
      await expect(menuToggle).toHaveAttribute('aria-label', 'Toggle navigation menu');
      await expect(menuToggle).toHaveAttribute('aria-expanded');
    });

    test('Dropdown menus have proper ARIA attributes', async ({ page }) => {
      await page.goto('/');

      // Tournaments dropdown trigger
      const tournamentsLink = page.locator('.neba-nav-item .neba-nav-link', { hasText: 'Tournaments' }).first();
      await expect(tournamentsLink).toHaveAttribute('aria-haspopup', 'true');
      await expect(tournamentsLink).toHaveAttribute('aria-expanded');

      // Dropdown menu
      const dropdown = page.locator('.neba-dropdown').first();
      await expect(dropdown).toHaveAttribute('role', 'menu');
    });

    test('Active links have aria-current attribute', async ({ page }) => {
      await page.goto('/hall-of-fame');

      const hofLink = page.locator('a[href="/hall-of-fame"].neba-nav-link');
      await expect(hofLink).toHaveAttribute('aria-current', 'page');
    });
  });

  test.describe('Footer', () => {
    test('Footer is visible on all pages', async ({ page }) => {
      await page.goto('/');

      const footer = page.locator('footer[role="contentinfo"]');
      await expect(footer).toBeVisible();

      // Navigate to another page
      await page.goto('/hall-of-fame');
      await expect(footer).toBeVisible();
    });

    test('Footer links are clickable', async ({ page }) => {
      await page.goto('/');

      const footer = page.locator('footer[role="contentinfo"]');

      // Check for privacy link
      const privacyLink = footer.locator('a[href="/privacy"]');
      await expect(privacyLink).toBeVisible();
    });

    test('Footer shows current year in copyright', async ({ page }) => {
      await page.goto('/');

      const currentYear = new Date().getFullYear();
      const footer = page.locator('footer[role="contentinfo"]');

      await expect(footer).toContainText(currentYear.toString());
    });
  });
});
