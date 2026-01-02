import { describe, test, expect, beforeEach, jest } from '@jest/globals';
import { scrollToTop } from './BowlingCenters.razor.js';

describe('BowlingCenters', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // Clean up DOM
    document.body.innerHTML = '';
  });

  describe('scrollToTop', () => {
    test('should scroll container to top when element exists', () => {
      // Arrange
      const container = document.createElement('div');
      container.id = 'centers-scroll-container';
      container.scrollTop = 500;
      document.body.appendChild(container);

      // Act
      scrollToTop();

      // Assert
      expect(container.scrollTop).toBe(0);
    });

    test('should handle when element does not exist', () => {
      // Act - should not throw error
      expect(() => scrollToTop()).not.toThrow();
    });

    test('should reset scroll position from any value', () => {
      // Arrange
      const container = document.createElement('div');
      container.id = 'centers-scroll-container';
      container.scrollTop = 1000;
      document.body.appendChild(container);

      // Act
      scrollToTop();

      // Assert
      expect(container.scrollTop).toBe(0);
    });

    test('should work when scrollTop is already 0', () => {
      // Arrange
      const container = document.createElement('div');
      container.id = 'centers-scroll-container';
      container.scrollTop = 0;
      document.body.appendChild(container);

      // Act
      scrollToTop();

      // Assert
      expect(container.scrollTop).toBe(0);
    });
  });
});
