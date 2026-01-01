import { describe, test, expect } from '@jest/globals';
import { BREAKPOINTS } from './breakpoints.js';

describe('breakpoints', () => {
  describe('BREAKPOINTS object', () => {
    test('should export BREAKPOINTS object with correct values', () => {
      // Assert
      expect(BREAKPOINTS).toBeDefined();
      expect(typeof BREAKPOINTS).toBe('object');
    });

    test('should have MOBILE breakpoint at 767px', () => {
      // Assert
      expect(BREAKPOINTS.MOBILE).toBe(767);
    });

    test('should have TABLET_MIN breakpoint at 768px', () => {
      // Assert
      expect(BREAKPOINTS.TABLET_MIN).toBe(768);
    });

    test('should have TABLET_MAX breakpoint at 1100px', () => {
      // Assert
      expect(BREAKPOINTS.TABLET_MAX).toBe(1100);
    });

    test('should have DESKTOP_TIGHT_MIN breakpoint at 1101px', () => {
      // Assert
      expect(BREAKPOINTS.DESKTOP_TIGHT_MIN).toBe(1101);
    });

    test('should have DESKTOP_TIGHT_MAX breakpoint at 1250px', () => {
      // Assert
      expect(BREAKPOINTS.DESKTOP_TIGHT_MAX).toBe(1250);
    });

    test('should have DESKTOP_MEDIUM_MIN breakpoint at 1251px', () => {
      // Assert
      expect(BREAKPOINTS.DESKTOP_MEDIUM_MIN).toBe(1251);
    });

    test('should have DESKTOP_MEDIUM_MAX breakpoint at 1400px', () => {
      // Assert
      expect(BREAKPOINTS.DESKTOP_MEDIUM_MAX).toBe(1400);
    });

    test('should have DESKTOP_WIDE_MIN breakpoint at 1401px', () => {
      // Assert
      expect(BREAKPOINTS.DESKTOP_WIDE_MIN).toBe(1401);
    });

    test('should have all breakpoint values as numbers', () => {
      // Assert
      Object.values(BREAKPOINTS).forEach(value => {
        expect(typeof value).toBe('number');
        expect(Number.isFinite(value)).toBe(true);
      });
    });

    test('should have breakpoints in ascending order for range pairs', () => {
      // Assert
      expect(BREAKPOINTS.MOBILE).toBeLessThan(BREAKPOINTS.TABLET_MIN);
      expect(BREAKPOINTS.TABLET_MIN).toBeLessThan(BREAKPOINTS.TABLET_MAX);
      expect(BREAKPOINTS.TABLET_MAX).toBeLessThan(BREAKPOINTS.DESKTOP_TIGHT_MIN);
      expect(BREAKPOINTS.DESKTOP_TIGHT_MIN).toBeLessThan(BREAKPOINTS.DESKTOP_TIGHT_MAX);
      expect(BREAKPOINTS.DESKTOP_TIGHT_MAX).toBeLessThan(BREAKPOINTS.DESKTOP_MEDIUM_MIN);
      expect(BREAKPOINTS.DESKTOP_MEDIUM_MIN).toBeLessThan(BREAKPOINTS.DESKTOP_MEDIUM_MAX);
      expect(BREAKPOINTS.DESKTOP_MEDIUM_MAX).toBeLessThan(BREAKPOINTS.DESKTOP_WIDE_MIN);
    });
  });
});
