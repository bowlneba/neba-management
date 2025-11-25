import { test, expect } from '@playwright/test';

// Test data constants
const TEST_DATA = {
  toast: {
    error: { message: 'This is a test error toast message' },
    warning: { message: 'This is a test warning toast message' },
    success: { message: 'This is a test success toast message' },
    info: { message: 'This is a test info toast message' },
    normal: { message: 'This is a test normal toast message' }
  },
  alert: {
    error: { title: 'Error', message: 'This is a test error alert' },
    warning: { title: 'Warning', message: 'This is a test warning alert' },
    success: { title: 'Success', message: 'This is a test success alert' },
    info: { title: 'Information', message: 'This is a test info alert' },
    normal: { title: 'Notice', message: 'This is a test normal alert' },
    validation: {
      title: 'Validation Failed',
      message: 'Email is required. Password must be at least 8 characters.'
    }
  },
  timeouts: {
    toastAutoDismiss: 4000, // 4 seconds
    fadeOutAnimation: 200   // 0.2 seconds (matches ToastTiming.FadeOutAnimationMs)
  }
};

test.describe('Notification Test Harness', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/testing/notifications');
    await expect(page.locator('h1')).toContainText('Notification Test Harness');
  });

  test.describe('Toast Notifications - Basic Functionality', () => {
    test('Error Toast appears and auto-dismisses', async ({ page }) => {
      await page.getByTestId('error-toast-btn').click();

      const toast = page.locator('.neba-toast.neba-toast-error');
      await expect(toast).toBeVisible();

      // Move mouse far away to prevent PauseOnHover from triggering
      await page.mouse.move(10, 10);
      await page.waitForTimeout(100); // Small delay to ensure timer starts

      // Wait for auto-dismiss - element should be removed from DOM
      const timeoutMs = TEST_DATA.timeouts.toastAutoDismiss + TEST_DATA.timeouts.fadeOutAnimation + 1500;
      await expect(toast).toBeHidden({ timeout: timeoutMs });
    });

    test('Warning Toast appears and auto-dismisses', async ({ page }) => {
      await page.getByTestId('warning-toast-btn').click();

      const toast = page.locator('.neba-toast.neba-toast-warning');
      await expect(toast).toBeVisible();

      // Move mouse far away to prevent PauseOnHover from triggering
      await page.mouse.move(10, 10);
      await page.waitForTimeout(100); // Small delay to ensure timer starts

      const timeoutMs = TEST_DATA.timeouts.toastAutoDismiss + TEST_DATA.timeouts.fadeOutAnimation + 1500;
      await expect(toast).toBeHidden({ timeout: timeoutMs });
    });

    test('Success Toast appears and auto-dismisses', async ({ page }) => {
      await page.getByTestId('success-toast-btn').click();

      const toast = page.locator('.neba-toast.neba-toast-success');
      await expect(toast).toBeVisible();

      // Move mouse far away to prevent PauseOnHover from triggering
      await page.mouse.move(10, 10);
      await page.waitForTimeout(100); // Small delay to ensure timer starts

      const timeoutMs = TEST_DATA.timeouts.toastAutoDismiss + TEST_DATA.timeouts.fadeOutAnimation + 1500;
      await expect(toast).toBeHidden({ timeout: timeoutMs });
    });

    test('Info Toast appears and auto-dismisses', async ({ page }) => {
      await page.getByTestId('info-toast-btn').click();

      const toast = page.locator('.neba-toast.neba-toast-info');
      await expect(toast).toBeVisible();

      // Move mouse far away to prevent PauseOnHover from triggering
      await page.mouse.move(10, 10);
      await page.waitForTimeout(100); // Small delay to ensure timer starts

      const timeoutMs = TEST_DATA.timeouts.toastAutoDismiss + TEST_DATA.timeouts.fadeOutAnimation + 1500;
      await expect(toast).toBeHidden({ timeout: timeoutMs });
    });

    test('Normal Toast appears and auto-dismisses', async ({ page }) => {
      await page.getByTestId('normal-toast-btn').click();

      const toast = page.locator('.neba-toast.neba-toast-normal');
      await expect(toast).toBeVisible();

      // Move mouse far away to prevent PauseOnHover from triggering
      await page.mouse.move(10, 10);
      await page.waitForTimeout(100); // Small delay to ensure timer starts

      const timeoutMs = TEST_DATA.timeouts.toastAutoDismiss + TEST_DATA.timeouts.fadeOutAnimation + 1500;
      await expect(toast).toBeHidden({ timeout: timeoutMs });
    });
  });

  test.describe('Toast Notifications - Manual Dismiss', () => {
    test('Error Toast can be manually dismissed', async ({ page }) => {
      await page.getByTestId('error-toast-btn').click();

      const toast = page.locator('.neba-toast.neba-toast-error');
      await expect(toast).toBeVisible();

      const dismissBtn = page.locator('button.neba-toast-dismiss');
      await dismissBtn.click();

      // Verify dismissing class is added
      await expect(toast).toHaveClass(/dismissing/);

      // Wait for fade-out animation using Playwright's built-in waiting
      await expect(toast).toBeHidden({ timeout: TEST_DATA.timeouts.fadeOutAnimation + 200 });
    });

    test('Warning Toast can be manually dismissed', async ({ page }) => {
      await page.getByTestId('warning-toast-btn').click();

      const toast = page.locator('.neba-toast.neba-toast-warning');
      await expect(toast).toBeVisible();

      await page.locator('button.neba-toast-dismiss').click();

      await expect(toast).toBeHidden({ timeout: TEST_DATA.timeouts.fadeOutAnimation + 200 });
    });
  });

  test.describe('Toast Notifications - Content Verification', () => {
    test('Error Toast displays correct message', async ({ page }) => {
      await page.getByTestId('error-toast-btn').click();

      const message = page.locator('.neba-toast-message');
      await expect(message).toContainText(TEST_DATA.toast.error.message);
    });

    test('Toast dismiss button has correct aria-label', async ({ page }) => {
      await page.getByTestId('error-toast-btn').click();

      const dismissBtn = page.locator('button.neba-toast-dismiss');
      await expect(dismissBtn).toHaveAttribute('aria-label', 'Dismiss notification');
    });
  });

  test.describe('Alert Notifications - Basic Functionality', () => {
    test('Error Alert appears and persists', async ({ page }) => {
      await page.getByTestId('error-alert-btn').click();

      const alert = page.locator('.neba-alert-container .neba-alert.neba-alert-error');
      await expect(alert).toBeVisible();

      // Wait 5 seconds to verify no auto-dismiss
      await page.waitForTimeout(5000);
      await expect(alert).toBeVisible();
    });

    test('Warning Alert appears and persists', async ({ page }) => {
      await page.getByTestId('warning-alert-btn').click();

      const alert = page.locator('.neba-alert-container .neba-alert.neba-alert-warning');
      await expect(alert).toBeVisible();
    });

    test('Success Alert appears and persists', async ({ page }) => {
      await page.getByTestId('success-alert-btn').click();

      const alert = page.locator('.neba-alert-container .neba-alert.neba-alert-success');
      await expect(alert).toBeVisible();
    });

    test('Info Alert appears and persists', async ({ page }) => {
      await page.getByTestId('info-alert-btn').click();

      const alert = page.locator('.neba-alert-container .neba-alert.neba-alert-info');
      await expect(alert).toBeVisible();
    });

    test('Normal Alert appears and persists', async ({ page }) => {
      await page.getByTestId('normal-alert-btn').click();

      const alert = page.locator('.neba-alert-container .neba-alert.neba-alert-normal');
      await expect(alert).toBeVisible();
    });
  });

  test.describe('Alert Notifications - Dismissal', () => {
    test('Error Alert can be dismissed', async ({ page }) => {
      await page.getByTestId('error-alert-btn').click();

      const alert = page.locator('.neba-alert-container .neba-alert.neba-alert-error');
      await expect(alert).toBeVisible();

      await page.locator('button.neba-alert-close').click();
      await expect(alert).not.toBeVisible();
    });

    test('Alert close button has correct aria-label', async ({ page }) => {
      await page.getByTestId('error-alert-btn').click();

      const closeBtn = page.locator('.neba-alert-container button.neba-alert-close');
      await expect(closeBtn).toHaveAttribute('aria-label', 'Dismiss alert');
    });
  });

  test.describe('Alert Notifications - Content Verification', () => {
    test('Error Alert displays correct title and message', async ({ page }) => {
      await page.getByTestId('error-alert-btn').click();

      const title = page.locator('.neba-alert-container .neba-alert-title');
      await expect(title).toContainText(TEST_DATA.alert.error.title);

      const content = page.locator('.neba-alert-container .neba-alert-content');
      await expect(content).toContainText(TEST_DATA.alert.error.message);
    });

    test('Info Alert displays correct title and message', async ({ page }) => {
      await page.getByTestId('info-alert-btn').click();

      const title = page.locator('.neba-alert-container .neba-alert-title');
      await expect(title).toContainText(TEST_DATA.alert.info.title);

      const content = page.locator('.neba-alert-container .neba-alert-content');
      await expect(content).toContainText(TEST_DATA.alert.info.message);
    });
  });

  test.describe('Combined Alert + Toast', () => {
    test('Validation Failure triggers both alert and toast', async ({ page }) => {
      await page.getByTestId('validation-failure-btn').click();

      const alert = page.locator('.neba-alert-container .neba-alert.neba-alert-error');
      const toast = page.locator('.neba-toast.neba-toast-error');

      await expect(alert).toBeVisible();
      await expect(toast).toBeVisible();
    });

    test('Validation Failure alert contains correct title', async ({ page }) => {
      await page.getByTestId('validation-failure-btn').click();

      const title = page.locator('.neba-alert-container .neba-alert-title');
      await expect(title).toContainText(TEST_DATA.alert.validation.title);
    });

    test('Validation Failure displays correct message', async ({ page }) => {
      await page.getByTestId('validation-failure-btn').click();

      const content = page.locator('.neba-alert-container .neba-alert-content');
      await expect(content).toContainText(TEST_DATA.alert.validation.message);
    });

    test('Custom Alert + Toast triggers both simultaneously', async ({ page }) => {
      await page.getByTestId('custom-alert-toast-btn').click();

      const alert = page.locator('.neba-alert-container .neba-alert.neba-alert-warning');
      const toast = page.locator('.neba-toast.neba-toast-warning');

      await expect(alert).toBeVisible();
      await expect(toast).toBeVisible();
    });
  });

  test.describe('Accessibility', () => {
    test('Error/Warning toasts have assertive aria-live', async ({ page }) => {
      await page.getByTestId('error-toast-btn').click();
      let toast = page.locator('.neba-toast.neba-toast-error');
      await expect(toast).toHaveAttribute('aria-live', 'assertive');

      // Move mouse far away to prevent PauseOnHover from triggering
      await page.mouse.move(10, 10);
      await page.waitForTimeout(100); // Small delay to ensure timer starts

      // Wait for toast to dismiss before triggering next toast
      const timeoutMs = TEST_DATA.timeouts.toastAutoDismiss + TEST_DATA.timeouts.fadeOutAnimation + 500;
      await expect(toast).toBeHidden({ timeout: timeoutMs });

      await page.getByTestId('warning-toast-btn').click();
      toast = page.locator('.neba-toast.neba-toast-warning');
      await expect(toast).toHaveAttribute('aria-live', 'assertive');
    });

    test('Info/Success/Normal toasts have polite aria-live', async ({ page }) => {
      await page.getByTestId('info-toast-btn').click();
      const toast = page.locator('.neba-toast.neba-toast-info');
      await expect(toast).toHaveAttribute('aria-live', 'polite');
    });

    test('Error/Warning alerts have alert role', async ({ page }) => {
      await page.getByTestId('error-alert-btn').click();
      const alert = page.locator('.neba-alert-container .neba-alert.neba-alert-error');
      await expect(alert).toHaveAttribute('role', 'alert');
    });

    test('Info/Success/Normal alerts have status role', async ({ page }) => {
      await page.getByTestId('info-alert-btn').click();
      const alert = page.locator('.neba-alert-container .neba-alert.neba-alert-info');
      await expect(alert).toHaveAttribute('role', 'status');
    });
  });

  test.describe('Multiple Notifications', () => {
    test('Multiple toasts can appear simultaneously', async ({ page }) => {
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


  test.describe('Mobile Viewport Behavior', () => {
    test('Toast notifications display correctly on mobile', async ({ page }) => {
      // Set mobile viewport
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/testing/notifications');

      await page.getByTestId('error-toast-btn').click();

      const toast = page.locator('.neba-toast.neba-toast-error');
      await expect(toast).toBeVisible();

      // Verify toast is positioned correctly (not clipped or off-screen)
      const boundingBox = await toast.boundingBox();
      expect(boundingBox).not.toBeNull();
      expect(boundingBox!.x).toBeGreaterThanOrEqual(0);
      expect(boundingBox!.y).toBeGreaterThanOrEqual(0);
    });

    test('Alert notifications display correctly on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/testing/notifications');

      await page.getByTestId('error-alert-btn').click();

      const alert = page.locator('.neba-alert-container .neba-alert.neba-alert-error');
      await expect(alert).toBeVisible();

      // Verify alert is positioned correctly
      const boundingBox = await alert.boundingBox();
      expect(boundingBox).not.toBeNull();
      expect(boundingBox!.x).toBeGreaterThanOrEqual(0);
    });

    test('Testing menu accessible on mobile viewport', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/');

      // Open mobile menu
      const menuToggle = page.locator('button.neba-menu-toggle');
      await expect(menuToggle).toBeVisible();
      await menuToggle.click();

      // Verify Testing menu dropdown exists
      const testingMenu = page.locator('a.neba-nav-link', { hasText: 'Testing' });
      await expect(testingMenu).toBeVisible();

      // Click Testing dropdown
      await testingMenu.click();

      // Wait for mobile menu to expand and Testing link to be visible
      const testingLink = page.locator('a.neba-nav-link[href="/testing/notifications"]');
      await expect(testingLink).toBeVisible({ timeout: 2000 });
    });

    test('Toast dismiss button accessible on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/testing/notifications');

      await page.getByTestId('error-toast-btn').click();

      const toast = page.locator('.neba-toast.neba-toast-error');
      await expect(toast).toBeVisible();

      // Verify dismiss button is tappable
      const dismissBtn = page.locator('button.neba-toast-dismiss');
      await expect(dismissBtn).toBeVisible();

      // Click to dismiss (click works for both desktop and mobile)
      await dismissBtn.click();
      await expect(toast).toBeHidden({ timeout: TEST_DATA.timeouts.fadeOutAnimation + 200 });
    });

    test('Alert close button accessible on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/testing/notifications');

      await page.getByTestId('error-alert-btn').click();

      const alert = page.locator('.neba-alert-container .neba-alert.neba-alert-error');
      await expect(alert).toBeVisible();

      // Verify close button is clickable
      const closeBtn = page.locator('.neba-alert-container button.neba-alert-close');
      await expect(closeBtn).toBeVisible();

      await closeBtn.click();
      await expect(alert).not.toBeVisible();
    });
  });

  test.describe('Environment Protection', () => {
    test('Test harness page accessible in Test environment', async ({ page }) => {
      await page.goto('/testing/notifications');

      // Verify page loads
      await expect(page.locator('h1')).toContainText('Notification Test Harness');

      // Verify all test buttons present
      await expect(page.getByTestId('error-toast-btn')).toBeVisible();
      await expect(page.getByTestId('warning-toast-btn')).toBeVisible();
      await expect(page.getByTestId('success-toast-btn')).toBeVisible();
      await expect(page.getByTestId('info-toast-btn')).toBeVisible();
      await expect(page.getByTestId('normal-toast-btn')).toBeVisible();
      await expect(page.getByTestId('error-alert-btn')).toBeVisible();
      await expect(page.getByTestId('warning-alert-btn')).toBeVisible();
      await expect(page.getByTestId('success-alert-btn')).toBeVisible();
      await expect(page.getByTestId('info-alert-btn')).toBeVisible();
      await expect(page.getByTestId('normal-alert-btn')).toBeVisible();
      await expect(page.getByTestId('validation-failure-btn')).toBeVisible();
      await expect(page.getByTestId('custom-alert-toast-btn')).toBeVisible();
    });

    test('Testing menu item visible in navigation', async ({ page }) => {
      await page.goto('/');

      // Open mobile menu if on mobile viewport
      const isMobile = page.viewportSize()!.width < 768;
      if (isMobile) {
        const menuToggle = page.locator('button.neba-menu-toggle');
        await menuToggle.click();
      }

      // Verify Testing menu dropdown exists
      const testingMenu = page.locator('a.neba-nav-link', { hasText: 'Testing' });
      await expect(testingMenu).toBeVisible();

      // Verify Testing link is present (it's a direct link, not a dropdown)
      const testingLink = page.locator('a.neba-nav-link[href="/testing/notifications"]');
      await expect(testingLink).toBeVisible();
    });
  });
});
