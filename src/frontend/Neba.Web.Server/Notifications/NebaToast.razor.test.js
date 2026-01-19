import { describe, test, expect, beforeEach, jest } from '@jest/globals';
import { createTimer } from './NebaToast.razor.js';

describe('NebaToast', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  describe('createTimer', () => {
    test('should create timer with correct initial properties', () => {
      // Arrange
      const mockComponentRef = {
        invokeMethodAsync: jest.fn()
      };
      const duration = 5000;

      // Act
      const timer = createTimer(mockComponentRef, duration);

      // Assert
      expect(timer.dotNetRef).toBe(mockComponentRef);
      expect(timer.remainingTime).toBe(duration);
      expect(timer.isPaused).toBe(false);
      expect(timer.timerId).not.toBeNull();
    });

    test('should start timer automatically on creation', () => {
      // Arrange
      const mockComponentRef = {
        invokeMethodAsync: jest.fn()
      };
      const duration = 5000;

      // Act
      const timer = createTimer(mockComponentRef, duration);

      // Assert
      expect(timer.timerId).not.toBeNull();
    });

    test('should invoke OnTimerExpired when timer completes', async () => {
      // Arrange
      const mockComponentRef = {
        invokeMethodAsync: jest.fn().mockResolvedValue(undefined)
      };
      const duration = 5000;

      // Act
      createTimer(mockComponentRef, duration);
      jest.advanceTimersByTime(duration);

      // Wait for promise to resolve
      await Promise.resolve();

      // Assert
      expect(mockComponentRef.invokeMethodAsync).toHaveBeenCalledWith('OnTimerExpired');
    });

    test('should pause timer and update remaining time', () => {
      // Arrange
      const mockComponentRef = {
        invokeMethodAsync: jest.fn()
      };
      const duration = 5000;
      const timer = createTimer(mockComponentRef, duration);

      // Act
      jest.advanceTimersByTime(2000);
      timer.pause();

      // Assert
      expect(timer.isPaused).toBe(true);
      expect(timer.remainingTime).toBeLessThan(duration);
      expect(timer.remainingTime).toBeGreaterThan(0);
    });

    test('should not pause if already paused', () => {
      // Arrange
      const mockComponentRef = {
        invokeMethodAsync: jest.fn()
      };
      const duration = 5000;
      const timer = createTimer(mockComponentRef, duration);

      // Act
      timer.pause();
      const firstRemainingTime = timer.remainingTime;
      timer.pause(); // Try to pause again

      // Assert
      expect(timer.remainingTime).toBe(firstRemainingTime);
    });

    test('should resume timer after pause', async () => {
      // Arrange
      const mockComponentRef = {
        invokeMethodAsync: jest.fn().mockResolvedValue(undefined)
      };
      const duration = 5000;
      const timer = createTimer(mockComponentRef, duration);

      // Act
      jest.advanceTimersByTime(2000);
      timer.pause();
      const remainingAfterPause = timer.remainingTime;

      timer.resume();
      expect(timer.isPaused).toBe(false);

      jest.advanceTimersByTime(remainingAfterPause);
      await Promise.resolve();

      // Assert
      expect(mockComponentRef.invokeMethodAsync).toHaveBeenCalledWith('OnTimerExpired');
    });

    test('should not resume if not paused', () => {
      // Arrange
      const mockComponentRef = {
        invokeMethodAsync: jest.fn()
      };
      const duration = 5000;
      const timer = createTimer(mockComponentRef, duration);
      const initialTimerId = timer.timerId;

      // Act
      timer.resume(); // Try to resume without pausing

      // Assert
      expect(timer.isPaused).toBe(false);
      expect(timer.timerId).toBe(initialTimerId);
    });

    test('should cancel timer and clear resources', () => {
      // Arrange
      const mockComponentRef = {
        invokeMethodAsync: jest.fn()
      };
      const duration = 5000;
      const timer = createTimer(mockComponentRef, duration);

      // Act
      timer.cancel();

      // Assert
      expect(timer.timerId).toBeNull();
      expect(timer.dotNetRef).toBeNull();
      expect(timer.remainingTime).toBe(0);
      expect(timer.isPaused).toBe(false);
    });

    test('should not invoke callback after cancellation', async () => {
      // Arrange
      const mockComponentRef = {
        invokeMethodAsync: jest.fn()
      };
      const duration = 5000;
      const timer = createTimer(mockComponentRef, duration);

      // Act
      timer.cancel();
      jest.advanceTimersByTime(duration);
      await Promise.resolve();

      // Assert
      expect(mockComponentRef.invokeMethodAsync).not.toHaveBeenCalled();
    });

    test('should handle multiple pause/resume cycles', async () => {
      // Arrange
      const mockComponentRef = {
        invokeMethodAsync: jest.fn().mockResolvedValue(undefined)
      };
      const duration = 10000;
      const timer = createTimer(mockComponentRef, duration);

      // Act - First pause/resume cycle
      jest.advanceTimersByTime(3000);
      timer.pause();
      timer.resume();

      // Second pause/resume cycle
      jest.advanceTimersByTime(3000);
      timer.pause();
      timer.resume();

      // Let remaining time complete
      jest.advanceTimersByTime(10000);
      await Promise.resolve();

      // Assert
      expect(mockComponentRef.invokeMethodAsync).toHaveBeenCalledWith('OnTimerExpired');
      expect(timer.isPaused).toBe(false);
    });

    test('should preserve dotNetRef after pause', () => {
      // Arrange
      const mockComponentRef = {
        invokeMethodAsync: jest.fn()
      };
      const duration = 5000;
      const timer = createTimer(mockComponentRef, duration);

      // Act
      timer.pause();

      // Assert
      expect(timer.dotNetRef).toBe(mockComponentRef);
    });

    test('should update startTime when resuming', () => {
      // Arrange
      const mockComponentRef = {
        invokeMethodAsync: jest.fn()
      };
      const duration = 5000;
      const timer = createTimer(mockComponentRef, duration);
      const initialStartTime = timer.startTime;

      // Act
      jest.advanceTimersByTime(2000);
      timer.pause();
      jest.advanceTimersByTime(1000); // Time passes while paused
      timer.resume();

      // Assert
      expect(timer.startTime).toBeGreaterThan(initialStartTime);
    });

    test('should handle pause after timer completes', async () => {
      // Arrange
      const mockComponentRef = {
        invokeMethodAsync: jest.fn().mockResolvedValue(undefined)
      };
      const duration = 5000;
      const timer = createTimer(mockComponentRef, duration);

      // Act
      jest.advanceTimersByTime(duration);
      await Promise.resolve();
      timer.pause(); // Try to pause after completion

      // Assert
      // Should not throw error
      expect(timer.isPaused).toBe(true);
    });

    test('should handle cancel when no timer is active', () => {
      // Arrange
      const mockComponentRef = {
        invokeMethodAsync: jest.fn()
      };
      const duration = 5000;
      const timer = createTimer(mockComponentRef, duration);

      // Act
      timer.cancel();
      timer.cancel(); // Cancel again

      // Assert
      expect(timer.timerId).toBeNull();
      expect(timer.dotNetRef).toBeNull();
    });
  });
});
