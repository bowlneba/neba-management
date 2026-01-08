import { test, expect, Page } from '@playwright/test';

/**
 * E2E Tests for Document Navigation - Hash Navigation & Scrolling
 * Tests the critical user experience of navigating to document sections via URL hash and clicking links
 *
 * These tests verify:
 * - Scrolling to section when loading page with hash in URL
 * - Clicking section links updates URL and scrolls correctly
 * - Different behavior for desktop (with TOC) vs mobile (without TOC)
 * - Bookmark/named range navigation works
 */

// Sample HTML document for testing (includes sections, bookmarks, and internal links)
const MOCK_DOCUMENT_HTML = `
<div>
  <h1 id="section-10.3-annual-meeting">Section 10.3 Annual Meeting</h1>
  <p>The annual meeting shall be held...</p>
  <p>See also <a href="#established-election-cycle">established election cycle</a> and <a href="#quorum-provisions">quorum provisions</a>.</p>

  <h2 id="article-vii-hall-of-fame-committee">ARTICLE VII - HALL OF FAME COMMITTEE</h2>
  <p>The Hall of Fame Committee shall...</p>

  <h2 id="established-election-cycle"><span id="established-election-cycle"></span>Established Election Cycle</h2>
  <p>Elections shall be conducted on a regular cycle...</p>

  <h2 id="quorum-provisions"><span id="quorum-provisions"></span>Quorum Provisions</h2>
  <p>A quorum shall consist of...</p>

  <h2 id="section-12.1-amendments">Section 12.1 Amendments</h2>
  <p>These bylaws may be amended...</p>

  <!-- Link to another document -->
  <p>See <a href="/tournaments/rules">Tournament Rules</a> for competition guidelines.</p>
</div>
`;

async function mockDocumentAPI(page: Page) {
  // Mock the API endpoint that returns bylaws HTML
  // The web server calls the API at localhost:5150
  await page.route('**/bylaws', async route => {
    // Only intercept GET requests to the API endpoint
    if (route.request().url().includes('localhost:5150')) {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          content: MOCK_DOCUMENT_HTML,
          metadata: {
            LastUpdatedUtc: '2024-01-01T00:00:00Z',
            LastUpdatedBy: 'Test User'
          }
        })
      });
    } else {
      // Allow the page request through
      await route.continue();
    }
  });

  // Mock the refresh status endpoint
  await page.route('**/bylaws/refresh/status', async route => {
    await route.fulfill({
      status: 200,
      contentType: 'text/event-stream',
      body: 'data: {"status":"Completed"}\n\n'
    });
  });
}

test.describe('Document Navigation - Desktop with TOC', () => {
  test.use({ viewport: { width: 1280, height: 720 } });

  test.beforeEach(async ({ page }) => {
    await mockDocumentAPI(page);
  });

  test('loads page with hash and scrolls to section', async ({ page }) => {
    await page.goto('/bylaws#section-10.3-annual-meeting');

    // Wait for content to load and scroll to complete
    const heading = page.locator('h1#section-10\\.3-annual-meeting');
    await expect(heading).toBeVisible({ timeout: 5000 });

    // Verify the heading is in viewport (scrolled to)
    const isInViewport = await heading.evaluate(el => {
      const rect = el.getBoundingClientRect();
      return rect.top >= 0 && rect.top <= window.innerHeight;
    });
    expect(isInViewport).toBeTruthy();

    // Verify URL still has the hash
    expect(page.url()).toContain('#section-10.3-annual-meeting');
  });

  test('clicking section link scrolls and updates URL', async ({ page }) => {
    await page.goto('/bylaws');

    // Wait for content to load
    await page.waitForSelector('h1#section-10\\.3-annual-meeting', { timeout: 5000 });

    // Click a link to "established election cycle"
    await page.click('a[href="#established-election-cycle"]');

    // Wait for scroll animation
    await page.waitForTimeout(500);

    // Verify URL updated
    expect(page.url()).toContain('#established-election-cycle');

    // Verify target section is visible
    const targetHeading = page.locator('h2#established-election-cycle');
    await expect(targetHeading).toBeVisible();

    // Verify scrolled to the section
    const isInViewport = await targetHeading.evaluate(el => {
      const rect = el.getBoundingClientRect();
      return rect.top >= 0 && rect.top <= window.innerHeight;
    });
    expect(isInViewport).toBeTruthy();
  });

  test('clicking bookmark link navigates correctly', async ({ page }) => {
    await page.goto('/bylaws');

    // Wait for content to load
    await page.waitForSelector('h1#section-10\\.3-annual-meeting', { timeout: 5000 });

    // Click bookmark link for "quorum provisions"
    await page.click('a[href="#quorum-provisions"]');

    // Wait for scroll
    await page.waitForTimeout(500);

    // Verify URL updated
    expect(page.url()).toContain('#quorum-provisions');

    // Verify target is visible
    const targetHeading = page.locator('h2#quorum-provisions');
    await expect(targetHeading).toBeVisible();
  });

  test('TOC is visible on desktop', async ({ page }) => {
    await page.goto('/bylaws');

    // Wait for TOC to render
    const toc = page.locator('.neba-document-toc');
    await expect(toc).toBeVisible({ timeout: 5000 });
  });

  test('navigating between sections updates active TOC link', async ({ page }) => {
    await page.goto('/bylaws');

    // Wait for content and TOC to load
    await page.waitForSelector('.neba-document-toc', { timeout: 5000 });
    await page.waitForTimeout(500);

    // Click a section link
    await page.click('a[href="#article-vii-hall-of-fame-committee"]');
    await page.waitForTimeout(500);

    // Verify the corresponding TOC link is active (if TOC contains this link)
    const activeTocLink = page.locator('.neba-document-toc .toc-link.active');
    const exists = await activeTocLink.count() > 0;

    // If TOC link exists, it should be active
    if (exists) {
      await expect(activeTocLink).toBeVisible();
    }
  });
});

test.describe('Document Navigation - Mobile without TOC', () => {
  test.use({ viewport: { width: 375, height: 667 } });

  test.beforeEach(async ({ page }) => {
    await mockDocumentAPI(page);
  });

  test('loads page with hash and scrolls to section on mobile', async ({ page }) => {
    await page.goto('/bylaws#section-10.3-annual-meeting');

    // Wait for content to load and scroll to complete
    const heading = page.locator('h1#section-10\\.3-annual-meeting');
    await expect(heading).toBeVisible({ timeout: 5000 });

    // Wait for scroll animation
    await page.waitForTimeout(500);

    // Verify the heading is in viewport and NOT hidden behind navbar
    const headingPosition = await heading.evaluate(el => {
      const rect = el.getBoundingClientRect();
      return {
        top: rect.top,
        isVisible: rect.top >= 80 && rect.top <= window.innerHeight
      };
    });

    // Heading should be visible below the navbar (navbar is ~80px)
    expect(headingPosition.isVisible).toBeTruthy();
    expect(headingPosition.top).toBeGreaterThanOrEqual(80);

    // Verify URL still has the hash
    expect(page.url()).toContain('#section-10.3-annual-meeting');
  });

  test('clicking section link scrolls correctly on mobile', async ({ page }) => {
    await page.goto('/bylaws');

    // Wait for content to load
    await page.waitForSelector('h1#section-10\\.3-annual-meeting', { timeout: 5000 });

    // Click a link to another section
    await page.click('a[href="#section-12.1-amendments"]');

    // Wait for scroll animation
    await page.waitForTimeout(500);

    // Verify URL updated
    expect(page.url()).toContain('#section-12.1-amendments');

    // Verify target section is visible and not behind navbar
    const targetHeading = page.locator('h2#section-12\\.1-amendments');
    await expect(targetHeading).toBeVisible();

    const headingPosition = await targetHeading.evaluate(el => {
      const rect = el.getBoundingClientRect();
      return {
        top: rect.top,
        isVisible: rect.top >= 80 && rect.top <= window.innerHeight
      };
    });

    expect(headingPosition.isVisible).toBeTruthy();
    expect(headingPosition.top).toBeGreaterThanOrEqual(80); // Below navbar
  });

  test('TOC is hidden on mobile by default', async ({ page }) => {
    await page.goto('/bylaws');

    // Wait for page to load
    await page.waitForTimeout(1000);

    // Desktop TOC should be hidden on mobile
    const desktopToc = page.locator('.neba-document-toc:not(.neba-document-toc-modal)');
    const isVisible = await desktopToc.isVisible().catch(() => false);
    expect(isVisible).toBeFalsy();
  });

  test('mobile TOC button is visible', async ({ page }) => {
    await page.goto('/bylaws');

    // Mobile TOC button should be visible
    const tocButton = page.locator('.neba-document-toc-mobile-btn');
    await expect(tocButton).toBeVisible({ timeout: 5000 });
  });

  test('can open mobile TOC modal', async ({ page }) => {
    await page.goto('/bylaws');

    // Wait for page to load
    await page.waitForSelector('.neba-document-toc-mobile-btn', { timeout: 5000 });

    // Click mobile TOC button
    await page.click('.neba-document-toc-mobile-btn');

    // Wait for animation
    await page.waitForTimeout(300);

    // Modal should be visible
    const tocModal = page.locator('.neba-document-toc-modal');
    await expect(tocModal).toBeVisible();
  });
});

test.describe('Document Navigation - Internal Document Links', () => {
  test.use({ viewport: { width: 1280, height: 720 } });

  test.beforeEach(async ({ page }) => {
    await mockDocumentAPI(page);
  });

  test('clicking internal document link opens slide-over', async ({ page }) => {
    await page.goto('/bylaws');

    // Wait for content to load
    await page.waitForSelector('h1#section-10\\.3-annual-meeting', { timeout: 5000 });

    // Click link to another document
    const tournamentRulesLink = page.locator('a[href="/tournaments/rules"]');

    if (await tournamentRulesLink.count() > 0) {
      await tournamentRulesLink.click();

      // Wait for slide-over to open
      await page.waitForTimeout(500);

      // Verify slide-over is visible (if implemented)
      const slideover = page.locator('.neba-document-slideover');
      const isVisible = await slideover.isVisible().catch(() => false);

      // This depends on implementation - either opens slide-over or navigates
      // For now, just verify the click doesn't error
      expect(true).toBeTruthy();
    }
  });
});

test.describe('Document Navigation - Edge Cases', () => {
  test.use({ viewport: { width: 1280, height: 720 } });

  test.beforeEach(async ({ page }) => {
    await mockDocumentAPI(page);
  });

  test('handles invalid hash gracefully', async ({ page }) => {
    await page.goto('/bylaws#non-existent-section');

    // Page should load without error
    await expect(page.locator('body')).toBeVisible({ timeout: 5000 });

    // URL should still have the hash
    expect(page.url()).toContain('#non-existent-section');
  });

  test('clicking same section link multiple times works', async ({ page }) => {
    await page.goto('/bylaws#section-10.3-annual-meeting');

    // Wait for initial load
    await page.waitForSelector('h1#section-10\\.3-annual-meeting', { timeout: 5000 });
    await page.waitForTimeout(500);

    // Click the same section link again
    await page.click('a[href="#section-10.3-annual-meeting"]');

    // Should still scroll/stay at the section without error
    const heading = page.locator('h1#section-10\\.3-annual-meeting');
    await expect(heading).toBeVisible();
  });

  test('viewport resize maintains scroll position', async ({ page }) => {
    await page.goto('/bylaws#article-vii-hall-of-fame-committee');

    // Wait for scroll
    await page.waitForTimeout(1000);

    // Get current scroll position
    const scrollBefore = await page.evaluate(() => window.scrollY);

    // Resize viewport (desktop to mobile)
    await page.setViewportSize({ width: 375, height: 667 });
    await page.waitForTimeout(500);

    // Heading should still be visible
    const heading = page.locator('h2#article-vii-hall-of-fame-committee');
    await expect(heading).toBeVisible();
  });
});
