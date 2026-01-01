import { describe, test, expect, beforeEach, jest } from '@jest/globals';
import { initializeToc, scrollToHash } from './NebaDocument.razor.js';

describe('NebaDocument', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    document.body.innerHTML = '';

    // Mock console methods
    globalThis.console.log = jest.fn();
    globalThis.console.warn = jest.fn();
    globalThis.console.error = jest.fn();

    // Mock requestAnimationFrame
    globalThis.requestAnimationFrame = jest.fn((cb) => {
      cb();
      return 1;
    });

    // Mock history
    globalThis.history = {
      pushState: jest.fn()
    };

    // Mock location
    delete globalThis.location;
    globalThis.location = {
      href: 'http://localhost',
      origin: 'http://localhost',
      hash: ''
    };

    // Mock fetch
    globalThis.fetch = jest.fn();
  });

  describe('initializeToc', () => {
    test('should initialize TOC with headings from content', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1 id="heading1">Heading 1</h1>
          <h2 id="heading2">Heading 2</h2>
          <h1 id="heading3">Another Heading 1</h1>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        headingLevels: 'h1, h2'
      };

      // Act
      const result = initializeToc(config);

      // Assert
      expect(result).toBe(true);
      const tocList = document.getElementById('toc-list');
      expect(tocList.innerHTML).toContain('Heading 1');
      expect(tocList.innerHTML).toContain('Heading 2');
      expect(tocList.innerHTML).toContain('Another Heading 1');
    });

    test('should return false when content element not found', () => {
      // Arrange
      document.body.innerHTML = `
        <ul id="toc-list"></ul>
      `;

      const config = {
        contentId: 'nonexistent',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close'
      };

      // Act
      const result = initializeToc(config);

      // Assert
      expect(result).toBe(false);
      expect(globalThis.console.error).toHaveBeenCalledWith(
        expect.stringContaining('Content element not found'),
        'nonexistent'
      );
    });

    test('should return false when TOC list element not found', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content"></div>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'nonexistent',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close'
      };

      // Act
      const result = initializeToc(config);

      // Assert
      expect(result).toBe(false);
      expect(globalThis.console.error).toHaveBeenCalledWith(
        expect.stringContaining('TOC list element not found'),
        'nonexistent'
      );
    });

    test('should return false when no headings found', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <p>No headings here</p>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close'
      };

      // Act
      const result = initializeToc(config);

      // Assert
      expect(result).toBe(false);
      expect(globalThis.console.warn).toHaveBeenCalledWith(
        expect.stringContaining('No headings found')
      );
    });

    test('should escape HTML in heading text to prevent XSS', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1 id="heading1"><script>alert('xss')</script>Test Heading</h1>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close'
      };

      // Act
      initializeToc(config);

      // Assert
      const tocList = document.getElementById('toc-list');
      // The escapeHtml function uses textContent which strips script tags
      // The text content is escaped when inserted into innerHTML
      expect(tocList.innerHTML).not.toContain('<script>');
      // Single quotes are not HTML-encoded in innerHTML, just the script tags are removed
      expect(tocList.innerHTML).toContain("alert('xss')Test Heading");
    });

    test('should support legacy positional arguments', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1 id="heading1">Heading 1</h1>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      // Act - call with legacy positional arguments
      const result = initializeToc(
        'content',
        'toc-list',
        'toc-mobile-list',
        'toc-mobile-button',
        'toc-modal',
        'toc-modal-overlay',
        'toc-modal-close',
        'h1, h2'
      );

      // Assert
      expect(result).toBe(true);
      const tocList = document.getElementById('toc-list');
      expect(tocList.innerHTML).toContain('Heading 1');
    });

    test('should auto-generate IDs for headings without IDs', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>No ID Heading 1</h1>
          <h2>No ID Heading 2</h2>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close'
      };

      // Act
      initializeToc(config);

      // Assert
      const headings = document.querySelectorAll('#content h1, #content h2');
      expect(headings[0].id).toBe('heading-0');
      expect(headings[1].id).toBe('heading-1');
    });

    test('should set up mobile modal open/close handlers', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading 1</h1>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close'
      };

      // Act
      initializeToc(config);

      const mobileButton = document.getElementById('toc-mobile-button');
      const modal = document.getElementById('toc-modal');

      // Simulate click on mobile button
      mobileButton.click();

      // Assert - modal should be opened (have 'active' class)
      expect(modal.classList.contains('active')).toBe(true);
      expect(document.body.style.overflow).toBe('hidden');
    });

    test('should close modal on overlay click', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading 1</h1>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal" class="active"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      document.body.style.overflow = 'hidden';

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close'
      };

      // Act
      initializeToc(config);

      const overlay = document.getElementById('toc-modal-overlay');
      const modal = document.getElementById('toc-modal');

      // Simulate click on overlay
      overlay.click();

      // Assert
      expect(modal.classList.contains('active')).toBe(false);
      expect(document.body.style.overflow).toBe('');
    });

    test('should close modal on Escape key', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading 1</h1>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal" class="active"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close'
      };

      // Act
      initializeToc(config);

      const modal = document.getElementById('toc-modal');
      const escapeEvent = new KeyboardEvent('keydown', { key: 'Escape' });
      document.dispatchEvent(escapeEvent);

      // Expected: modal should close
      expect(modal.classList.contains('active')).toBe(false);
    });

    test('should populate both desktop and mobile TOC lists', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading 1</h1>
          <h2>Heading 2</h2>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close'
      };

      // Act
      initializeToc(config);

      // Assert
      const tocList = document.getElementById('toc-list');
      const mobileTocList = document.getElementById('toc-mobile-list');

      expect(tocList.innerHTML).toContain('Heading 1');
      expect(tocList.innerHTML).toContain('Heading 2');
      expect(mobileTocList.innerHTML).toContain('Heading 1');
      expect(mobileTocList.innerHTML).toContain('Heading 2');
    });
  });

  describe('scrollToHash', () => {
    test('should scroll to element when hash exists', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content" style="height: 2000px; overflow: auto;">
          <h1 id="heading1" style="margin-top: 1000px;">Heading 1</h1>
        </div>
        <ul id="toc-list"></ul>
      `;

      globalThis.location.hash = '#heading1';
      globalThis.pageYOffset = 0;

      const content = document.getElementById('content');
      content.scrollTo = jest.fn();

      // Act
      scrollToHash('content', 'toc-list');

      // Assert
      expect(content.scrollTo).toHaveBeenCalledWith(
        expect.objectContaining({
          behavior: 'smooth'
        })
      );
    });

    test('should log error when content container not found', () => {
      // Arrange
      globalThis.location.hash = '#heading1';

      // Act
      scrollToHash('nonexistent', 'toc-list');

      // Assert
      expect(globalThis.console.error).toHaveBeenCalledWith(
        expect.stringContaining('Content container not found'),
        'nonexistent'
      );
    });

    test('should log error when target element not found', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1 id="heading1">Heading 1</h1>
        </div>
        <ul id="toc-list"></ul>
      `;

      globalThis.location.hash = '#nonexistent';

      // Act
      scrollToHash('content', 'toc-list');

      // Assert
      expect(globalThis.console.error).toHaveBeenCalledWith(
        expect.stringContaining('Target element not found'),
        'nonexistent'
      );
    });

    test('should return early when no hash in URL', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1 id="heading1">Heading 1</h1>
        </div>
        <ul id="toc-list"></ul>
      `;

      globalThis.location.hash = '';

      // Act
      scrollToHash('content', 'toc-list');

      // Assert
      expect(globalThis.console.log).toHaveBeenCalledWith(
        expect.stringContaining('No hash in URL')
      );
    });

    test('should update active link in TOC', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1 id="heading1">Heading 1</h1>
          <h1 id="heading2">Heading 2</h1>
        </div>
        <ul id="toc-list">
          <li><a class="toc-link" data-target="heading1">Heading 1</a></li>
          <li><a class="toc-link active" data-target="heading2">Heading 2</a></li>
        </ul>
      `;

      globalThis.location.hash = '#heading1';

      const content = document.getElementById('content');
      content.scrollTo = jest.fn();

      // Act
      scrollToHash('content', 'toc-list');

      // Assert
      const activeLink = document.querySelector('.toc-link.active');
      expect(activeLink.dataset.target).toBe('heading1');
    });
  });

  describe('internal link navigation', () => {
    test('should handle anchor links within content', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1 id="section1">Section 1</h1>
          <a href="#section1">Link to Section 1</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close'
      };

      initializeToc(config);

      const content = document.getElementById('content');
      content.scrollTo = jest.fn();

      // Act
      const link = content.querySelector('a[href="#section1"]');
      const clickEvent = new MouseEvent('click', { bubbles: true, cancelable: true });
      link.dispatchEvent(clickEvent);

      // Expected: scroll to section1
    });

    test('should allow Ctrl/Cmd+Click to open in new tab', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <a href="https://example.com">External Link</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close'
      };

      initializeToc(config);

      // Act
      const link = document.querySelector('a[href="https://example.com"]');
      const clickEvent = new MouseEvent('click', {
        bubbles: true,
        cancelable: true,
        ctrlKey: true
      });

      const defaultPrevented = !link.dispatchEvent(clickEvent);

      // Expected: browser default behavior (open in new tab)
      // Event should NOT be prevented when Ctrl is pressed
      expect(defaultPrevented).toBe(false);
    });
  });
});
