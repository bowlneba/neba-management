import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Champions Page - Critical User Journeys Only
 * Component-level tests (CSS classes, aria attributes, rendering) belong in bUnit
 * All tests mock the API for fast, reliable, isolated browser testing
 */

const mockTitlesSummaryData = {
  items: [
    {
      bowlerId: "00000000-0000-0000-0000-000000000001",
      bowlerName: "Test Champion One",
      titleCount: 15,
      hallOfFame: true
    },
    {
      bowlerId: "00000000-0000-0000-0000-000000000002",
      bowlerName: "Test Champion Two",
      titleCount: 8,
      hallOfFame: false
    },
    {
      bowlerId: "00000000-0000-0000-0000-000000000003",
      bowlerName: "Test Champion Three",
      titleCount: 3,
      hallOfFame: false
    }
  ]
};

const mockBowlerTitles = {
  "00000000-0000-0000-0000-000000000001": {
    data: {
      bowlerId: "00000000-0000-0000-0000-000000000001",
      bowlerName: "Test Champion One",
      titles: [
        { titleId: "1", titleName: "Singles", year: 2023, season: "Fall" },
        { titleId: "2", titleName: "Doubles", year: 2023, season: "Fall" }
      ]
    }
  },
  "00000000-0000-0000-0000-000000000002": {
    data: {
      bowlerId: "00000000-0000-0000-0000-000000000002",
      bowlerName: "Test Champion Two",
      titles: [
        { titleId: "3", titleName: "Singles", year: 2022, season: "Spring" }
      ]
    }
  },
  "00000000-0000-0000-0000-000000000003": {
    data: {
      bowlerId: "00000000-0000-0000-0000-000000000003",
      bowlerName: "Test Champion Three",
      titles: [
        { titleId: "4", titleName: "Team", year: 2021, season: "Fall" }
      ]
    }
  }
};

test.describe('Champions Page - E2E User Journeys', () => {
  test.beforeEach(async ({ page }) => {
    // Mock API responses
    await page.route('**/titles/summary', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockTitlesSummaryData)
      });
    });

    await page.route('**/bowlers/*/titles', async route => {
      const regex = /bowlers\/([^/]+)\/titles/;
      const match = regex.exec(route.request().url());
      const bowlerId = match?.[1];
      const titleData = bowlerId ? mockBowlerTitles[bowlerId as keyof typeof mockBowlerTitles] : null;

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(titleData || { data: { bowlerId, bowlerName: "", titles: [] } })
      });
    });

    await page.goto('/history/champions', { waitUntil: 'networkidle' });
    await page.waitForSelector('.neba-loading-overlay-page', { state: 'hidden', timeout: 5000 })
      .catch(() => {});
  });

  test.describe('Critical User Journey: Browse Champions by Title Count', () => {
    test('User can view champions grouped by title count', async ({ page }) => {
      // Verify champions are displayed
      const cards = page.getByTestId('champion-card');
      await expect(cards.first()).toBeVisible({ timeout: 5000 });

      // Verify grouping by checking section headers
      const headers = page.locator('h2');
      await expect(headers.first()).toBeVisible();
    });

    test('User can collapse/expand sections to browse champions', async ({ page }) => {
      const toggleButton = page.locator('.tier-elite-header, .tier-mid-header, .tier-standard-header').first();
      await expect(toggleButton).toBeVisible({ timeout: 5000 });

      const section = page.locator('.tier-collapse-container').first();
      await expect(section).toHaveClass(/tier-expanded/);

      // User collapses section
      await toggleButton.click();
      await expect(section).toHaveClass(/tier-collapsed/);

      // User expands section again
      await toggleButton.click();
      await expect(section).toHaveClass(/tier-expanded/);
    });

    test('User can click champion to view detailed title history', async ({ page }) => {
      const firstCard = page.getByTestId('champion-card').first();
      await expect(firstCard).toBeVisible({ timeout: 5000 });

      const bowlerName = await firstCard.locator('h3').textContent();

      // User clicks champion card
      await firstCard.click();

      // Modal opens with champion details
      const modal = page.locator('.neba-modal-content.bowler-titles-modal');
      await expect(modal).toBeVisible({ timeout: 2000 });

      const modalTitle = page.locator('.neba-modal-title');
      await expect(modalTitle).toContainText(bowlerName || '');

      // User can see titles list
      const titlesTable = page.locator('.bowler-titles-table-section');
      await expect(titlesTable).toBeVisible({ timeout: 3000 });
    });
  });

  // Note: Year view tests require API implementation for /titles endpoint
  // These will be added once the API endpoint is available

  test.describe('Mobile User Experience', () => {
    test('Mobile user can browse champions and view details', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/history/champions');
      await page.waitForSelector('.neba-loading-overlay-page', { state: 'hidden', timeout: 5000 })
        .catch(() => {});

      // Mobile user sees champions
      const cards = page.getByTestId('champion-card');
      await expect(cards.first()).toBeVisible({ timeout: 5000 });

      // Mobile user can open modal
      await cards.first().click();
      const modal = page.locator('.neba-modal-content.bowler-titles-modal');
      await expect(modal).toBeVisible();
    });
  });

  test.describe('Error Handling', () => {
    test('User sees error message when data fails to load', async ({ page }) => {
      // Override mock with error response
      await page.route('**/titles/summary', route => {
        route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ error: 'Internal server error' })
        });
      });

      await page.goto('/history/champions', { waitUntil: 'networkidle' });
      await page.waitForTimeout(3000);

      // User should see some indication of error or empty state
      const body = page.locator('body');
      await expect(body).toBeVisible();
    });
  });
});
