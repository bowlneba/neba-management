import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Modals - Critical User Interaction Only
 * Component-level tests (rendering, props, etc.) belong in bUnit
 * These tests use the /testing/modals harness page which doesn't require API calls
 */
test.describe('Modals - E2E User Interaction', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/testing/modals');
  });

  test.describe('Modal Open/Close Flow', () => {
    test('User can open and close modal', async ({ page }) => {
      // User opens modal
      await page.click('[data-testid="open-basic-modal-btn"]');

      const modalContent = page.locator('[data-testid="basic-modal-content"]');
      await expect(modalContent).toBeVisible();

      // User closes with X button
      await page.click('.neba-modal-close');
      await expect(modalContent).not.toBeVisible();
    });

    test('User can close modal by clicking backdrop', async ({ page, isMobile }) => {
      test.skip(isMobile, 'Backdrop clicking unreliable on mobile');

      await page.click('[data-testid="open-basic-modal-btn"]');

      const modalContent = page.locator('[data-testid="basic-modal-content"]');
      await expect(modalContent).toBeVisible();

      // User clicks outside modal
      await page.locator('.neba-modal-backdrop').click({ position: { x: 10, y: 10 } });
      await expect(modalContent).not.toBeVisible();
    });

    test('Modal stays open when clicking inside content', async ({ page }) => {
      await page.click('[data-testid="open-basic-modal-btn"]');

      const modalContent = page.locator('[data-testid="basic-modal-content"]');
      await expect(modalContent).toBeVisible();

      // User clicks modal content
      await modalContent.click();
      await expect(modalContent).toBeVisible();
    });
  });

  test.describe('Modal with Actions', () => {
    test('User can cancel and close modal', async ({ page }) => {
      await page.click('[data-testid="open-footer-modal-btn"]');

      const modalContent = page.locator('[data-testid="footer-modal-content"]');
      await expect(modalContent).toBeVisible();

      // User clicks cancel
      await page.click('[data-testid="cancel-btn"]');
      await expect(modalContent).not.toBeVisible();
    });

    test('User can confirm action and see result', async ({ page }) => {
      await page.click('[data-testid="open-footer-modal-btn"]');

      const modalContent = page.locator('[data-testid="footer-modal-content"]');
      await expect(modalContent).toBeVisible();

      // User confirms action
      await page.click('[data-testid="confirm-btn"]');

      // Modal closes
      await expect(modalContent).not.toBeVisible();

      // Confirmation appears
      const confirmationMessage = page.locator('[data-testid="confirmation-message"]');
      await expect(confirmationMessage).toBeVisible();
      await expect(confirmationMessage).toHaveText('Action confirmed!');

      // Confirmation auto-dismisses
      await expect(confirmationMessage).not.toBeVisible({ timeout: 5000 });
    });
  });

  test.describe('Backdrop Close Prevention', () => {
    test('Modal with backdrop close disabled stays open when clicking outside', async ({ page }) => {
      await page.click('[data-testid="open-no-backdrop-close-modal-btn"]');

      const modalContent = page.locator('[data-testid="no-backdrop-close-modal-content"]');
      await expect(modalContent).toBeVisible();

      // User tries clicking backdrop
      await page.click('.neba-modal-backdrop', { position: { x: 10, y: 10 } });

      // Modal stays open
      await expect(modalContent).toBeVisible();

      // User must use X button or custom button
      await page.click('.neba-modal-close');
      await expect(modalContent).not.toBeVisible();
    });
  });

  test.describe('Keyboard Navigation', () => {
    test('User can close modal with keyboard', async ({ page, browserName }) => {
      test.skip(browserName === 'webkit', 'Keyboard navigation differs in webkit');

      await page.click('[data-testid="open-basic-modal-btn"]');

      const modalContent = page.locator('[data-testid="basic-modal-content"]');
      await expect(modalContent).toBeVisible();

      // User navigates to close button and presses Enter
      const closeButton = page.locator('.neba-modal-close');
      await closeButton.focus();
      await closeButton.press('Enter');

      await expect(modalContent).not.toBeVisible();
    });
  });

  test.describe('Mobile Modal Experience', () => {
    test('Modal works correctly on mobile viewport', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });

      await page.goto('/testing/modals');

      // User opens modal
      await page.click('[data-testid="open-basic-modal-btn"]');

      const modalContent = page.locator('[data-testid="basic-modal-content"]');
      await expect(modalContent).toBeVisible();

      // User can close
      await page.click('.neba-modal-close');
      await expect(modalContent).not.toBeVisible();
    });
  });
});
