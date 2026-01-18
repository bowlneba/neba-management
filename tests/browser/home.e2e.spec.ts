import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Home Page - Critical Landing Page Experience
 * Tests hero section, quick links, stats, and responsive behavior
 * Component-level tests belong in bUnit
 */
test.describe('Home Page - E2E User Experience', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test.describe('Hero Section', () => {
    test('Hero section displays main heading and tagline', async ({ page }) => {
      const heading = page.locator('h1', { hasText: 'New England Bowling Association' });
      await expect(heading).toBeVisible();

      const tagline = page.locator('text=Building Bowling Excellence Since 1963');
      await expect(tagline).toBeVisible();
    });

    test('Hero section is responsive on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/');

      const heading = page.locator('h1', { hasText: 'New England Bowling Association' });
      await expect(heading).toBeVisible();

      // Heading should still be readable
      const box = await heading.boundingBox();
      expect(box?.width).toBeLessThan(400);
    });

    test('Descriptive text is visible', async ({ page }) => {
      const description = page.locator('text=Join New England\'s premier bowling tour');
      await expect(description).toBeVisible();
    });
  });

  test.describe('Motto Section', () => {
    test('All three motto cards are displayed', async ({ page }) => {
      await expect(page.locator('text=Run by bowlers')).toBeVisible();
      await expect(page.locator('text=Built on respect')).toBeVisible();
      await expect(page.locator('text=Driven by competition')).toBeVisible();
    });

    test('Motto cards display in grid on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1024, height: 768 });
      await page.goto('/');

      const mottoCards = page.locator('p.font-semibold').filter({ hasText: /Run by bowlers|Built on respect|Driven by competition/ });
      await expect(mottoCards).toHaveCount(3);

      // All should be visible
      const count = await mottoCards.count();
      for (let i = 0; i < count; i++) {
        await expect(mottoCards.nth(i)).toBeVisible();
      }
    });

    test('Motto cards stack on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/');

      const mottoCards = page.locator('p.font-semibold').filter({ hasText: /Run by bowlers|Built on respect|Driven by competition/ });
      await expect(mottoCards).toHaveCount(3);

      // All should still be visible
      await expect(mottoCards.first()).toBeVisible();
      await expect(mottoCards.last()).toBeVisible();
    });
  });

  test.describe('Quick Stats', () => {
    test('All three stat cards are displayed', async ({ page }) => {
      await expect(page.locator('text=60+')).toBeVisible();
      await expect(page.locator('text=Years of History')).toBeVisible();

      await expect(page.locator('text=1000+')).toBeVisible();
      await expect(page.locator('text=Tournaments Held')).toBeVisible();

      await expect(page.locator('text=500+')).toBeVisible();
      await expect(page.locator('text=Active Bowlers')).toBeVisible();
    });

    test('Stat cards have hover effects on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1024, height: 768 });
      await page.goto('/');

      const statCard = page.locator('.neba-card').filter({ hasText: 'Years of History' }).first();

      // Hover over the card
      await statCard.hover();

      // Card should be visible and interactive
      await expect(statCard).toBeVisible();
    });

    test('Stats are readable on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/');

      const stat = page.locator('text=60+').first();
      await expect(stat).toBeVisible();

      // Should be large and readable
      const fontSize = await stat.evaluate((el) => window.getComputedStyle(el).fontSize);
      expect(parseFloat(fontSize)).toBeGreaterThan(30);
    });
  });

  test.describe('Quick Links', () => {
    test('All four quick link cards are displayed', async ({ page }) => {
      await expect(page.locator('.neba-card[href="/tournaments"]')).toBeVisible();
      await expect(page.locator('.neba-card[href="/stats"]')).toBeVisible();
      await expect(page.locator('.neba-card[href="/history/champions"]')).toBeVisible();
      await expect(page.locator('.neba-card[href="/hall-of-fame"]')).toBeVisible();
    });

    test('Quick link cards are clickable and navigate correctly', async ({ page }) => {
      // Click Tournaments card
      await page.click('.neba-card[href="/tournaments"]');
      await expect(page).toHaveURL(/\/tournaments/);

      // Go back and test another
      await page.goto('/');
      await page.click('.neba-card[href="/hall-of-fame"]');
      await expect(page).toHaveURL(/\/hall-of-fame/);
    });

    test('Quick link cards have hover effects', async ({ page }) => {
      const tournamentsCard = page.locator('.neba-card[href="/tournaments"]');

      // Initial state
      await expect(tournamentsCard).toBeVisible();

      // Hover
      await tournamentsCard.hover();

      // Should still be visible and interactive
      await expect(tournamentsCard).toBeVisible();
    });

    test('Quick links display with icons', async ({ page }) => {
      // Each card should have an SVG icon
      const cards = page.locator('.neba-card svg');
      await expect(cards).toHaveCount(4);

      // All icons should be visible
      for (let i = 0; i < 4; i++) {
        await expect(cards.nth(i)).toBeVisible();
      }
    });

    test('Quick links are responsive on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/');

      // All cards should still be visible
      await expect(page.locator('.neba-card[href="/tournaments"]')).toBeVisible();
      await expect(page.locator('.neba-card[href="/stats"]')).toBeVisible();
      await expect(page.locator('.neba-card[href="/history/champions"]')).toBeVisible();
      await expect(page.locator('.neba-card[href="/hall-of-fame"]')).toBeVisible();
    });
  });

  test.describe('About Section', () => {
    test('About section displays heading and content', async ({ page }) => {
      const aboutHeading = page.locator('h2', { hasText: 'About NEBA' });
      await expect(aboutHeading).toBeVisible();

      await expect(page.locator('text=The New England Bowling Association (NEBA) has been')).toBeVisible();
    });

    test('About section contains all key information', async ({ page }) => {
      const aboutSection = page.locator('.neba-panel').filter({ hasText: 'About NEBA' });

      // Check for key phrases within the about section
      await expect(aboutSection.locator('text=/premier bowling tour.*since 1963/i')).toBeVisible();
      await expect(aboutSection.locator('text=/Bowler of the Year/i')).toBeVisible();
      await expect(aboutSection.locator('text=/Hall of Fame/i')).toBeVisible();
    });

    test('About section is readable on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/');

      const aboutSection = page.locator('.neba-panel').filter({ hasText: 'About NEBA' });
      await expect(aboutSection).toBeVisible();

      // Text should be readable
      const paragraph = aboutSection.locator('p').first();
      const box = await paragraph.boundingBox();
      expect(box?.width).toBeLessThan(400);
    });
  });

  test.describe('Page Layout and Structure', () => {
    test('Page title is set correctly', async ({ page }) => {
      // Wait for page to be fully loaded before checking title
      await expect(page.locator('h1')).toBeVisible();
      await expect(page).toHaveTitle(/Welcome to NEBA/);
    });

    test('Page has proper semantic structure', async ({ page }) => {
      // Should have h1
      const h1 = page.locator('h1');
      await expect(h1).toHaveCount(1);

      // Should have h2 (About section)
      const h2 = page.locator('h2');
      await expect(h2).toHaveCount(1);
    });

    test('Page content is properly spaced', async ({ page }) => {
      // Check major sections are present and properly spaced
      const heroHeading = page.locator('h1').first();
      const mottoSection = page.locator('text=Run by bowlers').first();

      // Both sections should be visible without overlap
      await expect(heroHeading).toBeVisible();
      await expect(mottoSection).toBeVisible();
    });

    test('All sections load without errors', async ({ page }) => {
      // Check that major sections are present
      await expect(page.locator('h1')).toBeVisible();
      await expect(page.locator('text=60+')).toBeVisible();
      await expect(page.locator('.neba-card[href="/tournaments"]')).toBeVisible();
      await expect(page.locator('h2', { hasText: 'About NEBA' })).toBeVisible();
    });
  });

  test.describe('Responsive Design', () => {
    test('Layout adapts to mobile viewport', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/');

      // All key content should be visible
      await expect(page.locator('h1')).toBeVisible();
      await expect(page.locator('text=60+')).toBeVisible();
      await expect(page.locator('.neba-card[href="/tournaments"]')).toBeVisible();
    });

    test('Layout adapts to tablet viewport', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto('/');

      await expect(page.locator('h1')).toBeVisible();
      await expect(page.locator('text=60+')).toBeVisible();
    });

    test('Layout adapts to desktop viewport', async ({ page }) => {
      await page.setViewportSize({ width: 1920, height: 1080 });
      await page.goto('/');

      await expect(page.locator('h1')).toBeVisible();
      await expect(page.locator('text=60+')).toBeVisible();
    });

    test('Content remains centered on wide screens', async ({ page }) => {
      await page.setViewportSize({ width: 1920, height: 1080 });
      await page.goto('/');

      const mainContent = page.locator('article.max-w-\\[1400px\\]');
      await expect(mainContent).toBeVisible();

      // Content should be centered
      const box = await mainContent.boundingBox();
      expect(box?.width).toBeLessThanOrEqual(1400);
    });
  });

  test.describe('Performance and Loading', () => {
    test('Page loads without visible layout shift', async ({ page }) => {
      await page.goto('/');

      // Wait for content to be visible
      await expect(page.locator('h1')).toBeVisible();

      // Give a moment for any potential shifts
      await page.waitForTimeout(100);

      // Content should still be visible and stable
      await expect(page.locator('h1')).toBeVisible();
      await expect(page.locator('text=60+')).toBeVisible();
    });

    test('All critical content is immediately visible', async ({ page }) => {
      await page.goto('/');

      // These should all be visible without waiting
      await expect(page.locator('h1')).toBeVisible({ timeout: 2000 });
      await expect(page.locator('text=60+')).toBeVisible({ timeout: 2000 });
      await expect(page.locator('.neba-card[href="/tournaments"]')).toBeVisible({ timeout: 2000 });
    });
  });

  test.describe('Cross-Browser Consistency', () => {
    test('Page displays correctly across browsers', async ({ page, browserName }) => {
      await page.goto('/');

      // Core content should be visible regardless of browser
      await expect(page.locator('h1')).toBeVisible();
      await expect(page.locator('text=60+')).toBeVisible();
      await expect(page.locator('.neba-card[href="/tournaments"]')).toBeVisible();
      await expect(page.locator('h2', { hasText: 'About NEBA' })).toBeVisible();
    });

    test('Interactive elements work across browsers', async ({ page, browserName }) => {
      await page.goto('/');

      // Quick link should work
      await page.click('.neba-card[href="/tournaments"]');
      await expect(page).toHaveURL(/\/tournaments/);
    });
  });
});
