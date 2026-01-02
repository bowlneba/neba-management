import { describe, test, expect, beforeEach, jest } from '@jest/globals';
import { setBodyOverflow } from './nebaModal.js';

describe('nebaModal', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // Reset body overflow style
    document.body.style.overflow = '';
  });

  describe('setBodyOverflow', () => {
    test('should set body overflow to hidden when passed true', () => {
      // Arrange
      const hidden = true;

      // Act
      setBodyOverflow(hidden);

      // Assert
      expect(document.body.style.overflow).toBe('hidden');
    });

    test('should set body overflow to empty string when passed false', () => {
      // Arrange
      document.body.style.overflow = 'hidden';
      const hidden = false;

      // Act
      setBodyOverflow(hidden);

      // Assert
      expect(document.body.style.overflow).toBe('');
    });

    test('should restore body scrolling after hiding it', () => {
      // Arrange
      setBodyOverflow(true);
      expect(document.body.style.overflow).toBe('hidden');

      // Act
      setBodyOverflow(false);

      // Assert
      expect(document.body.style.overflow).toBe('');
    });

    test('should handle multiple calls with true', () => {
      // Act
      setBodyOverflow(true);
      setBodyOverflow(true);
      setBodyOverflow(true);

      // Assert
      expect(document.body.style.overflow).toBe('hidden');
    });

    test('should handle multiple calls with false', () => {
      // Arrange
      document.body.style.overflow = 'hidden';

      // Act
      setBodyOverflow(false);
      setBodyOverflow(false);
      setBodyOverflow(false);

      // Assert
      expect(document.body.style.overflow).toBe('');
    });
  });
});
