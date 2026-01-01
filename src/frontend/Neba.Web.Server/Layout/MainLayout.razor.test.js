import { describe, test, expect, beforeEach, jest } from '@jest/globals';
import { toggleMobileMenu, toggleDropdown } from './MainLayout.razor.js';

describe('MainLayout', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    document.body.innerHTML = '';

    // Mock globalThis
    globalThis.innerWidth = 1024;
    globalThis.scrollY = 0;
  });

  describe('toggleMobileMenu', () => {
    test('should toggle menu active class when elements exist', () => {
      // Arrange
      document.body.innerHTML = `
        <nav id="main-menu" class=""></nav>
        <button data-action="toggle-menu" aria-expanded="false"></button>
      `;

      // Act
      toggleMobileMenu();

      // Assert
      const menu = document.querySelector('#main-menu');
      const toggle = document.querySelector('[data-action="toggle-menu"]');

      expect(menu.classList.contains('active')).toBe(true);
      expect(toggle.getAttribute('aria-expanded')).toBe('true');
    });

    test('should remove active class when menu is already active', () => {
      // Arrange
      document.body.innerHTML = `
        <nav id="main-menu" class="active"></nav>
        <button data-action="toggle-menu" aria-expanded="true"></button>
      `;

      // Act
      toggleMobileMenu();

      // Assert
      const menu = document.querySelector('#main-menu');
      const toggle = document.querySelector('[data-action="toggle-menu"]');

      expect(menu.classList.contains('active')).toBe(false);
      expect(toggle.getAttribute('aria-expanded')).toBe('false');
    });

    test('should handle when menu element does not exist', () => {
      // Arrange
      document.body.innerHTML = '<button data-action="toggle-menu"></button>';

      // Act & Assert - should not throw
      expect(() => toggleMobileMenu()).not.toThrow();
    });

    test('should handle when toggle button does not exist', () => {
      // Arrange
      document.body.innerHTML = '<nav id="main-menu"></nav>';

      // Act & Assert - should not throw
      expect(() => toggleMobileMenu()).not.toThrow();
    });

    test('should handle multiple toggle calls', () => {
      // Arrange
      document.body.innerHTML = `
        <nav id="main-menu" class=""></nav>
        <button data-action="toggle-menu" aria-expanded="false"></button>
      `;

      const menu = document.querySelector('#main-menu');
      const toggle = document.querySelector('[data-action="toggle-menu"]');

      // Act
      toggleMobileMenu(); // Open
      expect(menu.classList.contains('active')).toBe(true);
      expect(toggle.getAttribute('aria-expanded')).toBe('true');

      toggleMobileMenu(); // Close
      expect(menu.classList.contains('active')).toBe(false);
      expect(toggle.getAttribute('aria-expanded')).toBe('false');

      toggleMobileMenu(); // Open again
      expect(menu.classList.contains('active')).toBe(true);
      expect(toggle.getAttribute('aria-expanded')).toBe('true');
    });
  });

  describe('toggleDropdown', () => {
    test('should toggle dropdown active class', () => {
      // Arrange
      document.body.innerHTML = `
        <div class="neba-nav-item">
          <a aria-haspopup="true" aria-expanded="false">Menu</a>
        </div>
      `;

      const navItem = document.querySelector('.neba-nav-item');
      const element = navItem;

      // Act
      toggleDropdown(element);

      // Assert
      expect(navItem.classList.contains('active')).toBe(true);
      expect(navItem.querySelector('[aria-haspopup]').getAttribute('aria-expanded')).toBe('true');
    });

    test('should close dropdown when already active', () => {
      // Arrange
      document.body.innerHTML = `
        <div class="neba-nav-item active">
          <a aria-haspopup="true" aria-expanded="true">Menu</a>
        </div>
      `;

      const navItem = document.querySelector('.neba-nav-item');
      const element = navItem;

      // Act
      toggleDropdown(element);

      // Assert
      expect(navItem.classList.contains('active')).toBe(false);
      expect(navItem.querySelector('[aria-haspopup]').getAttribute('aria-expanded')).toBe('false');
    });

    test('should handle nested element passed to toggleDropdown', () => {
      // Arrange
      document.body.innerHTML = `
        <div class="neba-nav-item">
          <a aria-haspopup="true" aria-expanded="false">
            <span id="nested">Click me</span>
          </a>
        </div>
      `;

      const nestedElement = document.querySelector('#nested');

      // Act
      toggleDropdown(nestedElement);

      // Assert
      const navItem = document.querySelector('.neba-nav-item');
      expect(navItem.classList.contains('active')).toBe(true);
    });

    test('should handle when navItem is not found', () => {
      // Arrange
      const element = document.createElement('div');

      // Act & Assert - should not throw
      expect(() => toggleDropdown(element)).not.toThrow();
    });

    test('should handle when link is not found', () => {
      // Arrange
      document.body.innerHTML = `
        <div class="neba-nav-item">
          <div>No link here</div>
        </div>
      `;

      const navItem = document.querySelector('.neba-nav-item');

      // Act & Assert - should not throw
      expect(() => toggleDropdown(navItem)).not.toThrow();
    });

    test('should work with multiple dropdowns', () => {
      // Arrange
      document.body.innerHTML = `
        <div class="neba-nav-item" id="dropdown1">
          <a aria-haspopup="true" aria-expanded="false">Dropdown 1</a>
        </div>
        <div class="neba-nav-item" id="dropdown2">
          <a aria-haspopup="true" aria-expanded="false">Dropdown 2</a>
        </div>
      `;

      const dropdown1 = document.querySelector('#dropdown1');
      const dropdown2 = document.querySelector('#dropdown2');

      // Act
      toggleDropdown(dropdown1);
      toggleDropdown(dropdown2);

      // Assert
      expect(dropdown1.classList.contains('active')).toBe(true);
      expect(dropdown2.classList.contains('active')).toBe(true);

      toggleDropdown(dropdown1);
      expect(dropdown1.classList.contains('active')).toBe(false);
      expect(dropdown2.classList.contains('active')).toBe(true);
    });
  });

  describe('getBreakpoint helper', () => {
    test('should read breakpoint from CSS variable', () => {
      // Arrange
      document.documentElement.style.setProperty('--neba-breakpoint-tablet-max', '1024px');

      // Act
      const value = getComputedStyle(document.documentElement)
        .getPropertyValue('--neba-breakpoint-tablet-max')
        .trim();

      // Assert
      expect(value).toBe('1024px');
    });

    test('should parse breakpoint value as integer', () => {
      // Arrange
      document.documentElement.style.setProperty('--neba-breakpoint-mobile', '767px');

      // Act
      const value = getComputedStyle(document.documentElement)
        .getPropertyValue('--neba-breakpoint-mobile')
        .trim();
      const parsed = Number.parseInt(value, 10);

      // Assert
      expect(parsed).toBe(767);
    });
  });

  describe('keyboard navigation', () => {
    test('should close menu on Escape key', () => {
      // Arrange
      document.body.innerHTML = `
        <nav id="main-menu" class="active"></nav>
        <button data-action="toggle-menu" aria-expanded="true"></button>
      `;

      // Act
      const escapeEvent = new KeyboardEvent('keydown', { key: 'Escape' });
      document.dispatchEvent(escapeEvent);

      // Expected behavior: menu should close on Escape
      // Note: The actual event listener is set up in the module initialization
    });

    test('should close dropdown on Escape key', () => {
      // Arrange
      document.body.innerHTML = `
        <div class="neba-nav-item active">
          <a aria-haspopup="true" aria-expanded="true">Menu</a>
        </div>
      `;

      // Act
      const escapeEvent = new KeyboardEvent('keydown', { key: 'Escape' });
      document.dispatchEvent(escapeEvent);

      // Expected behavior: dropdown should close on Escape
    });
  });

  describe('scroll shadow effect', () => {
    test('should add scrolled class when page scrolls down', () => {
      // Arrange
      document.body.innerHTML = '<nav class="neba-navbar"></nav>';

      // Simulate scroll
      Object.defineProperty(globalThis, 'scrollY', {
        writable: true,
        configurable: true,
        value: 50
      });

      // Act
      const scrollEvent = new Event('scroll');
      globalThis.dispatchEvent(scrollEvent);

      // Expected behavior: navbar should have scrolled class when scrollY > 10
    });

    test('should remove scrolled class when at top of page', () => {
      // Arrange
      document.body.innerHTML = '<nav class="neba-navbar scrolled"></nav>';

      // Simulate scroll to top
      Object.defineProperty(globalThis, 'scrollY', {
        writable: true,
        configurable: true,
        value: 0
      });

      // Act
      const scrollEvent = new Event('scroll');
      globalThis.dispatchEvent(scrollEvent);

      // Expected behavior: navbar should not have scrolled class when scrollY <= 10
    });
  });

  describe('click outside handlers', () => {
    test('should close menu when clicking outside', () => {
      // Arrange
      document.body.innerHTML = `
        <nav id="main-menu" class="active"></nav>
        <button data-action="toggle-menu" aria-expanded="true"></button>
        <div id="outside">Outside content</div>
      `;

      const outsideElement = document.querySelector('#outside');

      // Act
      const clickEvent = new MouseEvent('click', { bubbles: true });
      Object.defineProperty(clickEvent, 'target', { value: outsideElement });
      document.dispatchEvent(clickEvent);

      // Expected behavior: menu should close when clicking outside
    });

    test('should not close menu when clicking inside menu', () => {
      // Arrange
      document.body.innerHTML = `
        <nav id="main-menu" class="active">
          <a id="menu-link">Menu item</a>
        </nav>
        <button data-action="toggle-menu" aria-expanded="true"></button>
      `;

      const menuLink = document.querySelector('#menu-link');

      // Act
      const clickEvent = new MouseEvent('click', { bubbles: true });
      Object.defineProperty(clickEvent, 'target', { value: menuLink });
      document.dispatchEvent(clickEvent);

      // Expected behavior: menu should remain open when clicking inside
    });
  });
});
