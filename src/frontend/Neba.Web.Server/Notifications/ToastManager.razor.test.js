import { describe, test, expect, beforeEach, jest } from '@jest/globals';
import { isMobile } from './ToastManager.razor.js';

describe('ToastManager', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('isMobile', () => {
    test('should return true when viewport width is mobile', () => {
      // Arrange
      const mockMatches = { matches: true };
      globalThis.matchMedia = jest.fn(() => mockMatches);

      // Act
      const result = isMobile();

      // Assert
      expect(result).toBe(true);
      expect(globalThis.matchMedia).toHaveBeenCalledWith('(max-width: 767px)');
    });

    test('should return false when viewport width is not mobile', () => {
      // Arrange
      const mockMatches = { matches: false };
      globalThis.matchMedia = jest.fn(() => mockMatches);

      // Act
      const result = isMobile();

      // Assert
      expect(result).toBe(false);
      expect(globalThis.matchMedia).toHaveBeenCalledWith('(max-width: 767px)');
    });

    test('should use BREAKPOINTS.MOBILE value from breakpoints.js', () => {
      // Arrange
      const mockMatches = { matches: false };
      globalThis.matchMedia = jest.fn(() => mockMatches);

      // Act
      isMobile();

      // Assert
      // Should use 767px from BREAKPOINTS.MOBILE
      expect(globalThis.matchMedia).toHaveBeenCalledWith('(max-width: 767px)');
    });

    test('should call matchMedia with correct media query', () => {
      // Arrange
      const mockMatches = { matches: true };
      const matchMediaSpy = jest.fn(() => mockMatches);
      globalThis.matchMedia = matchMediaSpy;

      // Act
      isMobile();

      // Assert
      expect(matchMediaSpy).toHaveBeenCalledTimes(1);
      expect(matchMediaSpy.mock.calls[0][0]).toMatch(/max-width.*767px/);
    });
  });
});
