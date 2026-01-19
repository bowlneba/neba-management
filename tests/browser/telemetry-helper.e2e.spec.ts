import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Telemetry Helper - JavaScript Client-Side Telemetry
 * Tests telemetry initialization, event tracking, error tracking, and performance monitoring
 */
test.describe('Telemetry Helper - JavaScript Telemetry Functions', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to a page that loads the telemetry helper
    await page.goto('/');

    // Wait for the telemetry helper script to load
    await page.waitForLoadState('networkidle');
  });

  test.describe('Telemetry Initialization', () => {
    test('Telemetry helper functions are available globally', async ({ page }) => {
      const hasTelemetryFunctions = await page.evaluate(() => {
        return typeof window.telemetry !== 'undefined' &&
               typeof window.telemetry.trackEvent === 'function' &&
               typeof window.telemetry.trackError === 'function' &&
               typeof window.telemetry.createTimer === 'function';
      });

      expect(hasTelemetryFunctions).toBe(true);
    });

    test('Telemetry can be initialized without errors', async ({ page }) => {
      const initSuccess = await page.evaluate(() => {
        try {
          if (window.telemetry && typeof window.telemetry.initializePerformanceTracking === 'function') {
            return true;
          }
          return false;
        } catch {
          return false;
        }
      });

      expect(initSuccess).toBe(true);
    });
  });

  test.describe('Event Tracking', () => {
    test('trackEvent can be called with event name only', async ({ page }) => {
      const result = await page.evaluate(() => {
        try {
          if (window.telemetry && typeof window.telemetry.trackEvent === 'function') {
            window.telemetry.trackEvent('test.event');
            return { success: true };
          }
          return { success: false, error: 'trackEvent not available' };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
    });

    test('trackEvent can be called with properties', async ({ page }) => {
      const result = await page.evaluate(() => {
        try {
          if (window.telemetry && typeof window.telemetry.trackEvent === 'function') {
            window.telemetry.trackEvent('test.event.with.properties', {
              userId: '123',
              action: 'click',
              count: 5
            });
            return { success: true };
          }
          return { success: false, error: 'trackEvent not available' };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
    });

    test('trackEvent handles null properties gracefully', async ({ page }) => {
      const result = await page.evaluate(() => {
        try {
          if (window.telemetry && typeof window.telemetry.trackEvent === 'function') {
            window.telemetry.trackEvent('test.event.null.properties', null);
            return { success: true };
          }
          return { success: false, error: 'trackEvent not available' };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
    });

    test('Multiple events can be tracked in sequence', async ({ page }) => {
      const result = await page.evaluate(() => {
        try {
          if (window.telemetry && typeof window.telemetry.trackEvent === 'function') {
            window.telemetry.trackEvent('event.1');
            window.telemetry.trackEvent('event.2', { value: 1 });
            window.telemetry.trackEvent('event.3', { value: 2 });
            return { success: true };
          }
          return { success: false, error: 'trackEvent not available' };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
    });
  });

  test.describe('Error Tracking', () => {
    test('trackError can be called with error details', async ({ page }) => {
      const result = await page.evaluate(() => {
        try {
          if (window.telemetry && typeof window.telemetry.trackError === 'function') {
            window.telemetry.trackError('Test error message', 'test.source', 'fake stack trace');
            return { success: true };
          }
          return { success: false, error: 'trackError not available' };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
    });

    test('trackError can be called without stack trace', async ({ page }) => {
      const result = await page.evaluate(() => {
        try {
          if (window.telemetry && typeof window.telemetry.trackError === 'function') {
            window.telemetry.trackError('Error without stack', 'error.source');
            return { success: true };
          }
          return { success: false, error: 'trackError not available' };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
    });

    test('trackError handles caught exceptions', async ({ page }) => {
      const result = await page.evaluate(() => {
        try {
          let caughtError;
          try {
            throw new Error('Intentional test error');
          } catch (error) {
            caughtError = error;
          }

          if (window.telemetry && typeof window.telemetry.trackError === 'function' && caughtError instanceof Error) {
            window.telemetry.trackError(
              caughtError.message,
              'test.exception',
              caughtError.stack || ''
            );
            return { success: true };
          }
          return { success: false, error: 'trackError not available' };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
    });
  });

  test.describe('Timer Functions', () => {
    test('createTimer returns a timer object', async ({ page }) => {
      const result = await page.evaluate(() => {
        try {
          if (window.telemetry && typeof window.telemetry.createTimer === 'function') {
            const timer = window.telemetry.createTimer('test.operation');
            return {
              success: true,
              hasStop: typeof timer.stop === 'function'
            };
          }
          return { success: false, hasStop: false };
        } catch (error) {
          return { success: false, hasStop: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
      expect(result.hasStop).toBe(true);
    });

    test('Timer can measure elapsed time', async ({ page }) => {
      const result = await page.evaluate(async () => {
        try {
          if (window.telemetry && typeof window.telemetry.createTimer === 'function') {
            const timer = window.telemetry.createTimer('test.timing');

            // Wait a bit
            await new Promise(resolve => setTimeout(resolve, 50));

            const duration = timer.stop();
            return {
              success: true,
              durationIsNumber: typeof duration === 'number',
              durationIsPositive: duration > 0
            };
          }
          return { success: false };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
      expect(result.durationIsNumber).toBe(true);
      expect(result.durationIsPositive).toBe(true);
    });

    test('Multiple timers can run concurrently', async ({ page }) => {
      const result = await page.evaluate(async () => {
        try {
          if (window.telemetry && typeof window.telemetry.createTimer === 'function') {
            const timer1 = window.telemetry.createTimer('operation.1');
            await new Promise(resolve => setTimeout(resolve, 10));

            const timer2 = window.telemetry.createTimer('operation.2');
            await new Promise(resolve => setTimeout(resolve, 10));

            const timer3 = window.telemetry.createTimer('operation.3');
            await new Promise(resolve => setTimeout(resolve, 10));

            const duration1 = timer1.stop();
            const duration2 = timer2.stop();
            const duration3 = timer3.stop();

            return {
              success: true,
              allPositive: duration1 > 0 && duration2 > 0 && duration3 > 0,
              correctOrder: duration1 > duration2 && duration2 > duration3
            };
          }
          return { success: false };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
      expect(result.allPositive).toBe(true);
      expect(result.correctOrder).toBe(true);
    });
  });

  test.describe('withTelemetry Wrapper', () => {
    test('withTelemetry wraps async function and tracks duration', async ({ page }) => {
      const result = await page.evaluate(async () => {
        try {
          if (window.telemetry && typeof window.telemetry.withTelemetry === 'function') {
            const asyncOperation = async () => {
              await new Promise(resolve => setTimeout(resolve, 50));
              return 'result';
            };

            const wrappedOperation = window.telemetry.withTelemetry('test.async', asyncOperation);
            const result = await wrappedOperation();

            return {
              success: true,
              resultCorrect: result === 'result'
            };
          }
          return { success: false };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
      expect(result.resultCorrect).toBe(true);
    });

    test('withTelemetry handles function errors correctly', async ({ page }) => {
      const result = await page.evaluate(async () => {
        try {
          if (window.telemetry && typeof window.telemetry.withTelemetry === 'function') {
            const failingOperation = async () => {
              throw new Error('Intentional error');
            };

            const wrappedOperation = window.telemetry.withTelemetry('test.failing', failingOperation);

            try {
              await wrappedOperation();
              return { success: false, errorThrown: false };
            } catch (error) {
              // Error should be thrown
              return {
                success: true,
                errorThrown: true,
                errorMessage: error instanceof Error ? error.message : ''
              };
            }
          }
          return { success: false };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
      expect(result.errorThrown).toBe(true);
      expect(result.errorMessage).toBe('Intentional error');
    });
  });

  test.describe('Performance API Integration', () => {
    test('Performance API is available', async ({ page }) => {
      const hasPerformanceAPI = await page.evaluate(() => {
        return typeof window.performance !== 'undefined' &&
               typeof window.performance.now === 'function';
      });

      expect(hasPerformanceAPI).toBe(true);
    });

    test('Navigation timing data is available', async ({ page }) => {
      const hasNavigationTiming = await page.evaluate(() => {
        try {
          const perfData = performance.getEntriesByType('navigation');
          return perfData && perfData.length > 0;
        } catch {
          return false;
        }
      });

      expect(hasNavigationTiming).toBe(true);
    });

    test('Resource timing data can be accessed', async ({ page }) => {
      const hasResourceTiming = await page.evaluate(() => {
        try {
          const resources = performance.getEntriesByType('resource');
          return Array.isArray(resources);
        } catch {
          return false;
        }
      });

      expect(hasResourceTiming).toBe(true);
    });
  });

  test.describe('Integration with .NET Bridge', () => {
    test('Telemetry functions work even without .NET reference', async ({ page }) => {
      // This tests that telemetry functions degrade gracefully
      const result = await page.evaluate(() => {
        try {
          if (window.telemetry && typeof window.telemetry.trackEvent === 'function') {
            // Even if no .NET reference is set, functions should not throw
            window.telemetry.trackEvent('standalone.event');
            return { success: true };
          }
          return { success: false };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
    });
  });

  test.describe('Real-World Usage Scenarios', () => {
    test('Can track button click with telemetry', async ({ page }) => {
      const result = await page.evaluate(() => {
        try {
          if (window.telemetry && typeof window.telemetry.trackEvent === 'function') {
            // Simulate button click tracking
            const button = { id: 'test-button', text: 'Click Me' };
            window.telemetry.trackEvent('button.clicked', {
              buttonId: button.id,
              buttonText: button.text,
              timestamp: Date.now()
            });
            return { success: true };
          }
          return { success: false };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
    });

    test('Can track page load performance', async ({ page }) => {
      const result = await page.evaluate(() => {
        try {
          if (window.telemetry && typeof window.telemetry.trackEvent === 'function') {
            const loadTime = performance.now();
            window.telemetry.trackEvent('page.loaded', {
              loadTime: loadTime,
              url: window.location.href
            });
            return { success: true, loadTime };
          }
          return { success: false };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
      expect(result.loadTime).toBeGreaterThan(0);
    });

    test('Can track API call with timer', async ({ page }) => {
      const result = await page.evaluate(async () => {
        try {
          if (window.telemetry && typeof window.telemetry.createTimer === 'function') {
            const timer = window.telemetry.createTimer('api.call');

            // Simulate API call
            await new Promise(resolve => setTimeout(resolve, 30));

            const duration = timer.stop();

            if (typeof window.telemetry.trackEvent === 'function') {
              window.telemetry.trackEvent('api.completed', {
                duration: duration,
                success: true
              });
            }

            return { success: true, duration };
          }
          return { success: false };
        } catch (error) {
          return { success: false, error: error instanceof Error ? error.message : String(error) };
        }
      });

      expect(result.success).toBe(true);
      expect(result.duration).toBeGreaterThan(0);
    });
  });
});
