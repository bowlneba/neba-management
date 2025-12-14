import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Notifications - Critical User Experience Only
 * Component-level tests (rendering, CSS classes, etc.) belong in bUnit
 * These tests use the /testing/notifications harness page which doesn't require API calls
 */

const TIMEOUTS = {
  toastAutoDismiss: 4000,
  fadeOutAnimation: 200
};

test.describe('Notifications - E2E User Experience', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/testing/notifications');
  });

  test.describe('Toast Auto-Dismiss Behavior (Real Browser Timing)', () => {
    test('Toast automatically dismisses after timeout', async ({ page }) => {
      await page.getByTestId('error-toast-btn').click();

      const toast = page.locator('.neba-toast.neba-toast-error');
      await expect(toast).toBeVisible();

      // Move mouse away to prevent pause-on-hover
      await page.mouse.move(10, 10);
      await page.waitForTimeout(100);

      // Verify auto-dismiss occurs
      const timeoutMs = TIMEOUTS.toastAutoDismiss + TIMEOUTS.fadeOutAnimation + 1500;
      await expect(toast).toBeHidden({ timeout: timeoutMs });
    });

    test('User can manually dismiss toast before auto-dismiss', async ({ page }) => {
      await page.getByTestId('error-toast-btn').click();

      const toast = page.locator('.neba-toast.neba-toast-error');
      await expect(toast).toBeVisible();

      // User clicks dismiss button
      await page.locator('button.neba-toast-dismiss').click();

      // Toast dismisses quickly
      await expect(toast).toBeHidden({ timeout: TIMEOUTS.fadeOutAnimation + 200 });
    });
  });

  test.describe('Alert Persistence (Real Browser Behavior)', () => {
    test('Alert persists until user dismisses it', async ({ page }) => {
      await page.getByTestId('error-alert-btn').click();

      const alert = page.locator('.neba-alert-container .neba-alert.neba-alert-error');
      await expect(alert).toBeVisible();

      // Wait to verify no auto-dismiss
      await page.waitForTimeout(5000);
      await expect(alert).toBeVisible();

      // User dismisses
      await page.locator('button.neba-alert-close').click();
      await expect(alert).not.toBeVisible();
    });
  });

  test.describe('Multiple Notifications (Real User Experience)', () => {
    test('Multiple toasts stack correctly', async ({ page }) => {
      await page.getByTestId('error-toast-btn').click();
      await page.getByTestId('success-toast-btn').click();

      const errorToast = page.locator('.neba-toast.neba-toast-error');
      const successToast = page.locator('.neba-toast.neba-toast-success');

      await expect(errorToast).toBeVisible();
      await expect(successToast).toBeVisible();
    });

    test('New alert replaces previous alert', async ({ page }) => {
      await page.getByTestId('error-alert-btn').click();
      const errorAlert = page.locator('.neba-alert-container .neba-alert.neba-alert-error');
      await expect(errorAlert).toBeVisible();

      await page.getByTestId('success-alert-btn').click();
      const successAlert = page.locator('.neba-alert-container .neba-alert.neba-alert-success');
      await expect(successAlert).toBeVisible();
      await expect(errorAlert).not.toBeVisible();
    });
  });

  test.describe('Loading Indicators (Real Browser Interaction)', () => {
    test('User can click overlay to dismiss loading indicator', async ({ page }) => {
      await page.getByTestId('show-loading-fullscreen-btn').click();

      const overlay = page.locator('.neba-loading-overlay-fullscreen');
      await expect(overlay).toBeVisible();

      // User clicks overlay to close
      await overlay.click();
      await expect(overlay).not.toBeVisible();
    });
  });

  test.describe('Mobile User Experience', () => {
    test('Toast displays correctly on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/testing/notifications');

      await page.getByTestId('error-toast-btn').click();

      const toast = page.locator('.neba-toast.neba-toast-error');
      await expect(toast).toBeVisible();

      // Verify toast is positioned on screen
      const boundingBox = await toast.boundingBox();
      expect(boundingBox).not.toBeNull();
      expect(boundingBox?.x).toBeGreaterThanOrEqual(0);

      // User can dismiss
      await page.locator('button.neba-toast-dismiss').click();
      await expect(toast).toBeHidden({ timeout: TIMEOUTS.fadeOutAnimation + 200 });
    });
  });
});
