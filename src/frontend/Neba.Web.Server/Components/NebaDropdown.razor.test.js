import { describe, test, expect, beforeEach, afterEach, jest } from '@jest/globals';
import { initializeDropdown, cleanup } from './NebaDropdown.razor.js';

describe('NebaDropdown', () => {
  let mockDotNetHelper;

  beforeEach(() => {
    jest.useFakeTimers();
    document.body.innerHTML = '';

    mockDotNetHelper = {
      invokeMethodAsync: jest.fn().mockResolvedValue(undefined)
    };
  });

  afterEach(() => {
    cleanup();
    jest.useRealTimers();
    jest.clearAllMocks();
  });

  describe('initializeDropdown', () => {
    test('should focus search input when present', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="dropdown">
          <input type="search" id="search" />
        </div>
      `;

      const element = document.querySelector('#dropdown');
      const searchInput = document.querySelector('#search');
      searchInput.focus = jest.fn();

      // Act
      initializeDropdown(element, mockDotNetHelper);

      // Fast-forward past the focus delay (50ms)
      jest.advanceTimersByTime(50);

      // Assert
      expect(searchInput.focus).toHaveBeenCalled();
    });

    test('should focus text input when search input not present', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="dropdown">
          <input type="text" id="text-input" />
        </div>
      `;

      const element = document.querySelector('#dropdown');
      const textInput = document.querySelector('#text-input');
      textInput.focus = jest.fn();

      // Act
      initializeDropdown(element, mockDotNetHelper);

      // Fast-forward past the focus delay
      jest.advanceTimersByTime(50);

      // Assert
      expect(textInput.focus).toHaveBeenCalled();
    });

    test('should handle element without search input', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="dropdown">
          <button>Click me</button>
        </div>
      `;

      const element = document.querySelector('#dropdown');

      // Act & Assert - should not throw
      expect(() => {
        initializeDropdown(element, mockDotNetHelper);
        jest.advanceTimersByTime(50);
      }).not.toThrow();
    });

    test('should handle null element gracefully', () => {
      // Act & Assert - should not throw
      expect(() => {
        initializeDropdown(null, mockDotNetHelper);
        jest.advanceTimersByTime(150);
      }).not.toThrow();
    });

    test('should clean up existing handlers before initializing', () => {
      // Arrange
      document.body.innerHTML = `<div id="dropdown"></div>`;
      const element = document.querySelector('#dropdown');

      // Act - initialize twice
      initializeDropdown(element, mockDotNetHelper);
      jest.advanceTimersByTime(150);

      initializeDropdown(element, mockDotNetHelper);
      jest.advanceTimersByTime(150);

      // Assert - should not throw and cleanup should work
      expect(() => cleanup()).not.toThrow();
    });

    test('should set up click-outside handler after delay', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="dropdown"></div>
        <div id="outside">Outside</div>
      `;

      const element = document.querySelector('#dropdown');
      const addEventListenerSpy = jest.spyOn(document, 'addEventListener');

      // Act
      initializeDropdown(element, mockDotNetHelper);

      // Fast-forward past the click handler setup delay (100ms)
      jest.advanceTimersByTime(100);

      // Assert - should have added click listener
      expect(addEventListenerSpy).toHaveBeenCalledWith('click', expect.any(Function), true);

      addEventListenerSpy.mockRestore();
    });
  });

  describe('click-outside detection', () => {
    test('should call dotNetHelper when clicking outside dropdown', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="dropdown">Dropdown content</div>
        <div id="outside">Outside content</div>
      `;

      const element = document.querySelector('#dropdown');
      const outsideElement = document.querySelector('#outside');

      initializeDropdown(element, mockDotNetHelper);

      // Fast-forward past the click handler setup delay
      jest.advanceTimersByTime(100);

      // Act - click outside
      const clickEvent = new MouseEvent('click', { bubbles: true });
      Object.defineProperty(clickEvent, 'target', { value: outsideElement, configurable: true });
      document.dispatchEvent(clickEvent);

      // Assert
      expect(mockDotNetHelper.invokeMethodAsync).toHaveBeenCalledWith('HandleClickOutsideAsync');
    });

    test('should not call dotNetHelper when clicking inside dropdown', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="dropdown">
          <button id="inside-btn">Inside button</button>
        </div>
      `;

      const element = document.querySelector('#dropdown');
      const insideButton = document.querySelector('#inside-btn');

      initializeDropdown(element, mockDotNetHelper);

      // Fast-forward past the click handler setup delay
      jest.advanceTimersByTime(100);

      // Act - click inside
      const clickEvent = new MouseEvent('click', { bubbles: true });
      Object.defineProperty(clickEvent, 'target', { value: insideButton, configurable: true });
      document.dispatchEvent(clickEvent);

      // Assert
      expect(mockDotNetHelper.invokeMethodAsync).not.toHaveBeenCalled();
    });

    test('should not call dotNetHelper when clicking on dropdown element itself', () => {
      // Arrange
      document.body.innerHTML = `<div id="dropdown">Content</div>`;
      const element = document.querySelector('#dropdown');

      initializeDropdown(element, mockDotNetHelper);
      jest.advanceTimersByTime(100);

      // Act - click on element itself
      const clickEvent = new MouseEvent('click', { bubbles: true });
      Object.defineProperty(clickEvent, 'target', { value: element, configurable: true });
      document.dispatchEvent(clickEvent);

      // Assert
      expect(mockDotNetHelper.invokeMethodAsync).not.toHaveBeenCalled();
    });

    test('should handle click when dropdown element is null', () => {
      // Arrange
      document.body.innerHTML = `<div id="outside">Outside</div>`;
      const outsideElement = document.querySelector('#outside');

      // Initialize with null element
      initializeDropdown(null, mockDotNetHelper);
      jest.advanceTimersByTime(100);

      // Act - click somewhere
      const clickEvent = new MouseEvent('click', { bubbles: true });
      Object.defineProperty(clickEvent, 'target', { value: outsideElement, configurable: true });
      document.dispatchEvent(clickEvent);

      // Assert - should not throw and should not call
      expect(mockDotNetHelper.invokeMethodAsync).not.toHaveBeenCalled();
    });

    test('should handle click when dotNetHelper is null', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="dropdown">Content</div>
        <div id="outside">Outside</div>
      `;

      const element = document.querySelector('#dropdown');
      const outsideElement = document.querySelector('#outside');

      initializeDropdown(element, null);
      jest.advanceTimersByTime(100);

      // Act & Assert - should not throw
      const clickEvent = new MouseEvent('click', { bubbles: true });
      Object.defineProperty(clickEvent, 'target', { value: outsideElement, configurable: true });
      expect(() => document.dispatchEvent(clickEvent)).not.toThrow();
    });
  });

  describe('cleanup', () => {
    test('should remove event listener', () => {
      // Arrange
      document.body.innerHTML = `<div id="dropdown"></div>`;
      const element = document.querySelector('#dropdown');
      const removeEventListenerSpy = jest.spyOn(document, 'removeEventListener');

      initializeDropdown(element, mockDotNetHelper);
      jest.advanceTimersByTime(100);

      // Act
      cleanup();

      // Assert
      expect(removeEventListenerSpy).toHaveBeenCalledWith('click', expect.any(Function), true);

      removeEventListenerSpy.mockRestore();
    });

    test('should handle cleanup when no handler exists', () => {
      // Act & Assert - should not throw
      expect(() => cleanup()).not.toThrow();
    });

    test('should reset all module state', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="dropdown">
          <input type="search" />
        </div>
        <div id="outside">Outside</div>
      `;

      const element = document.querySelector('#dropdown');
      const outsideElement = document.querySelector('#outside');

      initializeDropdown(element, mockDotNetHelper);
      jest.advanceTimersByTime(100);

      // Act
      cleanup();

      // Assert - clicking outside should not trigger callback after cleanup
      const clickEvent = new MouseEvent('click', { bubbles: true });
      Object.defineProperty(clickEvent, 'target', { value: outsideElement, configurable: true });
      document.dispatchEvent(clickEvent);

      expect(mockDotNetHelper.invokeMethodAsync).not.toHaveBeenCalled();
    });

    test('should allow re-initialization after cleanup', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="dropdown"></div>
        <div id="outside">Outside</div>
      `;

      const element = document.querySelector('#dropdown');
      const outsideElement = document.querySelector('#outside');

      initializeDropdown(element, mockDotNetHelper);
      jest.advanceTimersByTime(100);

      // Act - cleanup and reinitialize
      cleanup();
      initializeDropdown(element, mockDotNetHelper);
      jest.advanceTimersByTime(100);

      // Click outside
      const clickEvent = new MouseEvent('click', { bubbles: true });
      Object.defineProperty(clickEvent, 'target', { value: outsideElement, configurable: true });
      document.dispatchEvent(clickEvent);

      // Assert - should work after re-initialization
      expect(mockDotNetHelper.invokeMethodAsync).toHaveBeenCalledWith('HandleClickOutsideAsync');
    });
  });

  describe('focus error handling', () => {
    test('should silently ignore focus errors', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="dropdown">
          <input type="search" id="search" />
        </div>
      `;

      const element = document.querySelector('#dropdown');
      const searchInput = document.querySelector('#search');

      // Make focus throw an error
      searchInput.focus = jest.fn(() => {
        throw new Error('Focus error');
      });

      // Act & Assert - should not throw
      expect(() => {
        initializeDropdown(element, mockDotNetHelper);
        jest.advanceTimersByTime(50);
      }).not.toThrow();
    });
  });
});
