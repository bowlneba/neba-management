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

    test('should handle when mobile list element is missing', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading 1</h1>
        </div>
        <ul id="toc-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'nonexistent-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close'
      };

      // Act & Assert - should not throw
      expect(() => initializeToc(config)).not.toThrow();
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

    test('should open internal links in slideover when configured', async () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws">Internal Link</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({
          html: '<p>Document content</p>',
          metadata: { LastUpdatedUtc: '2025-01-01T12:00:00Z', LastUpdatedBy: 'Test User' }
        })
      });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act
      const link = document.querySelector('a[href="/bylaws"]');
      const clickEvent = new MouseEvent('click', { bubbles: true, cancelable: true });
      link.dispatchEvent(clickEvent);

      await Promise.resolve();
      await Promise.resolve();

      // Assert
      expect(globalThis.fetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/documents/bylaws'),
        expect.any(Object)
      );

      const slideoverContent = document.getElementById('slideover-content');
      expect(slideoverContent.innerHTML).toContain('Document content');
      expect(slideoverContent.innerHTML).toContain('Test User');
    });

    test('should handle fetch errors when opening slideover', async () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws">Internal Link</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      globalThis.fetch.mockResolvedValue({
        ok: false,
        status: 404,
        statusText: 'Not Found'
      });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act
      const link = document.querySelector('a[href="/bylaws"]');
      link.click();

      await Promise.resolve();
      await Promise.resolve();
      await Promise.resolve();

      // Assert
      const slideoverTitle = document.getElementById('slideover-title');
      const slideoverContent = document.getElementById('slideover-content');

      expect(slideoverTitle.textContent).toContain('Error');
      expect(slideoverContent.innerHTML).toContain('Failed to load');
    });

    test('should close slideover on close button click', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover" class="active"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      document.body.style.overflow = 'hidden';

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      const slideover = document.getElementById('slideover');
      const closeButton = document.getElementById('slideover-close');

      // Act
      closeButton.click();

      // Assert
      expect(slideover.classList.contains('active')).toBe(false);
      expect(document.body.style.overflow).toBe('');
    });

    test('should close slideover on Escape key', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover" class="active"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      const slideover = document.getElementById('slideover');
      const escapeEvent = new KeyboardEvent('keydown', { key: 'Escape' });

      // Act
      document.dispatchEvent(escapeEvent);

      // Assert
      expect(slideover.classList.contains('active')).toBe(false);
    });

    test('should allow external protocol links like mailto and tel', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="mailto:test@example.com">Email</a>
          <a href="tel:+1234567890">Phone</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act/Assert - clicking links should not throw
      const emailLink = document.querySelector('a[href^="mailto:"]');
      const phoneLink = document.querySelector('a[href^="tel:"]');

      expect(() => emailLink.click()).not.toThrow();
      expect(() => phoneLink.click()).not.toThrow();
    });

    test('should handle opening slideover with hash in URL', async () => {
      // Arrange
      jest.useFakeTimers();

      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws#section-2">Link with hash</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content" style="overflow: auto;"></div>
      `;

      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({
          html: '<h2 id="section-2">Section 2</h2><p>Content</p>',
          metadata: {}
        })
      });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act
      const link = document.querySelector('a[href="/bylaws#section-2"]');
      link.click();

      await Promise.resolve();
      await Promise.resolve();

      const slideoverContent = document.getElementById('slideover-content');

      // Mock scrollIntoView on the target element
      const targetElement = slideoverContent.querySelector('#section-2');
      if (targetElement) {
        targetElement.scrollIntoView = jest.fn();
      }

      // Fast-forward the setTimeout in openInSlideover
      jest.advanceTimersByTime(100);

      // Assert
      expect(slideoverContent.innerHTML).toContain('Section 2');
      if (targetElement) {
        expect(targetElement.scrollIntoView).toHaveBeenCalled();
      }

      jest.useRealTimers();
    });

    test('should handle missing hash target in slideover', async () => {
      // Arrange
      jest.useFakeTimers();

      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws#nonexistent">Link with invalid hash</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({
          html: '<p>Content without target</p>',
          metadata: {}
        })
      });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act
      const link = document.querySelector('a[href="/bylaws#nonexistent"]');
      link.click();

      await Promise.resolve();
      await Promise.resolve();

      // Fast-forward the setTimeout - should not throw when target not found
      expect(() => jest.advanceTimersByTime(100)).not.toThrow();

      jest.useRealTimers();
    });
  });

  describe('scroll spy functionality', () => {
    test('should update active link when scrolling content', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content" style="height: 500px; overflow: auto;">
          <h1 id="heading1" style="margin-top: 50px;">Heading 1</h1>
          <div style="height: 300px;"></div>
          <h1 id="heading2" style="margin-top: 100px;">Heading 2</h1>
          <div style="height: 300px;"></div>
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

      // Act - trigger scroll event
      const scrollEvent = new Event('scroll');
      content.dispatchEvent(scrollEvent);

      // Assert - requestAnimationFrame should be called
      expect(globalThis.requestAnimationFrame).toHaveBeenCalled();
    });

    test('should click desktop TOC link to scroll to section', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content" style="height: 500px; overflow: auto;">
          <h1 id="heading1">Heading 1</h1>
          <div style="height: 500px;"></div>
          <h1 id="heading2">Heading 2</h1>
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
      const tocLink = document.querySelector('.toc-link[data-target="heading2"]');
      const clickEvent = new MouseEvent('click', { bubbles: true, cancelable: true });
      tocLink.dispatchEvent(clickEvent);

      // Assert
      expect(content.scrollTo).toHaveBeenCalledWith(
        expect.objectContaining({
          behavior: 'smooth'
        })
      );
    });

    test('should click mobile TOC link to scroll to section and close modal', () => {
      // Arrange
      jest.useFakeTimers();

      document.body.innerHTML = `
        <div id="content" style="height: 500px; overflow: auto;">
          <h1 id="heading1">Heading 1</h1>
          <div style="height: 500px;"></div>
          <h1 id="heading2">Heading 2</h1>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal" class="active"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      globalThis.scrollTo = jest.fn();
      globalThis.pageYOffset = 0;

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

      const modal = document.getElementById('toc-modal');

      // Act
      const mobileTocLink = document.querySelector('#toc-mobile-list .toc-link[data-target="heading2"]');
      mobileTocLink.click();

      // Assert - modal should close immediately
      expect(modal.classList.contains('active')).toBe(false);

      // Fast-forward the setTimeout
      jest.advanceTimersByTime(300);

      // Assert - should call window.scrollTo
      expect(globalThis.scrollTo).toHaveBeenCalledWith(
        expect.objectContaining({
          behavior: 'smooth'
        })
      );

      jest.useRealTimers();
    });

    test('should handle missing target when clicking mobile TOC link', () => {
      // Arrange
      jest.useFakeTimers();

      document.body.innerHTML = `
        <div id="content">
          <h1 id="heading1">Heading 1</h1>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list">
          <li><a class="toc-link" data-target="nonexistent">Bad Link</a></li>
        </ul>
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

      initializeToc(config);

      const modal = document.getElementById('toc-modal');

      // Act
      const mobileTocLink = document.querySelector('#toc-mobile-list .toc-link');
      mobileTocLink.click();

      // Assert - modal should still close even if target not found
      expect(modal.classList.contains('active')).toBe(false);

      jest.useRealTimers();
    });
  });

  describe('nested slideover navigation', () => {
    test('should navigate to nested internal link in slideover', async () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws">Open Bylaws</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      globalThis.fetch
        .mockResolvedValueOnce({
          ok: true,
          json: jest.fn().mockResolvedValue({
            html: '<p>Bylaws content</p><a href="/rules">See Rules</a>',
            metadata: {}
          })
        })
        .mockResolvedValueOnce({
          ok: true,
          json: jest.fn().mockResolvedValue({
            html: '<p>Rules content</p>',
            metadata: {}
          })
        });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act - click initial link
      const initialLink = document.querySelector('a[href="/bylaws"]');
      initialLink.click();

      await Promise.resolve();
      await Promise.resolve();

      const slideoverContent = document.getElementById('slideover-content');
      expect(slideoverContent.innerHTML).toContain('Bylaws content');

      // Act - click nested link in slideover
      const nestedLink = slideoverContent.querySelector('a[href="/rules"]');
      nestedLink.click();

      await Promise.resolve();
      await Promise.resolve();

      // Assert - slideover content should be replaced
      expect(slideoverContent.innerHTML).toContain('Rules content');
      expect(globalThis.fetch).toHaveBeenCalledTimes(2);
    });

    test('should handle hash links within slideover', async () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws">Open Bylaws</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({
          html: '<h2 id="section1">Section 1</h2><a href="#section1">Jump to section</a>',
          metadata: {}
        })
      });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act - open slideover
      const initialLink = document.querySelector('a[href="/bylaws"]');
      initialLink.click();

      await Promise.resolve();
      await Promise.resolve();

      const slideoverContent = document.getElementById('slideover-content');
      const section = slideoverContent.querySelector('#section1');
      section.scrollIntoView = jest.fn();

      // Act - click hash link within slideover
      const hashLink = slideoverContent.querySelector('a[href="#section1"]');
      const clickEvent = new MouseEvent('click', { bubbles: true, cancelable: true });
      hashLink.dispatchEvent(clickEvent);

      // Assert - should scroll to section
      expect(section.scrollIntoView).toHaveBeenCalledWith(
        expect.objectContaining({
          behavior: 'smooth',
          block: 'start'
        })
      );
    });

    test('should allow Ctrl+Click in slideover nested links', async () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws">Open Bylaws</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({
          html: '<a href="/rules">External in new tab</a>',
          metadata: {}
        })
      });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act - open slideover
      const initialLink = document.querySelector('a[href="/bylaws"]');
      initialLink.click();

      await Promise.resolve();
      await Promise.resolve();

      const slideoverContent = document.getElementById('slideover-content');

      // Act - Ctrl+click nested link
      const nestedLink = slideoverContent.querySelector('a[href="/rules"]');
      const clickEvent = new MouseEvent('click', {
        bubbles: true,
        cancelable: true,
        ctrlKey: true
      });

      const defaultPrevented = !nestedLink.dispatchEvent(clickEvent);

      // Assert - should NOT prevent default (allow browser to open in new tab)
      expect(defaultPrevented).toBe(false);
    });
  });

  describe('edge cases and uncovered branches', () => {
    test('should handle link with empty href', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="">Empty href</a>
          <a href="#">Hash only</a>
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

      // Act & Assert - clicking links with empty/hash-only href should not throw
      const emptyLink = document.querySelector('a[href=""]');
      const hashLink = document.querySelector('a[href="#"]');

      expect(() => emptyLink.click()).not.toThrow();
      expect(() => hashLink.click()).not.toThrow();
    });

    test('should allow Cmd+Click (metaKey) to open in new tab', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws">Internal Link</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act
      const link = document.querySelector('a[href="/bylaws"]');
      const clickEvent = new MouseEvent('click', {
        bubbles: true,
        cancelable: true,
        metaKey: true
      });

      const defaultPrevented = !link.dispatchEvent(clickEvent);

      // Assert - should NOT prevent default (allow browser to open in new tab)
      expect(defaultPrevented).toBe(false);
      expect(globalThis.fetch).not.toHaveBeenCalled();
    });

    test('should handle anchor navigation with non-scrollable content (page scroll)', () => {
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

      // Mock content as non-scrollable (scrollHeight equals clientHeight)
      const content = document.getElementById('content');
      Object.defineProperty(content, 'scrollHeight', { value: 100, configurable: true });
      Object.defineProperty(content, 'clientHeight', { value: 100, configurable: true });
      content.scrollTo = null; // Remove scrollTo to trigger page scroll

      globalThis.scrollTo = jest.fn();
      globalThis.scrollY = 0;

      initializeToc(config);

      // Act
      const link = content.querySelector('a[href="#section1"]');
      const clickEvent = new MouseEvent('click', { bubbles: true, cancelable: true });
      link.dispatchEvent(clickEvent);

      // Assert - should call window.scrollTo instead of content.scrollTo
      expect(globalThis.scrollTo).toHaveBeenCalledWith(
        expect.objectContaining({
          behavior: 'smooth'
        })
      );
    });

    test('should warn when anchor target not found', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="#nonexistent">Bad anchor</a>
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
      const link = document.querySelector('a[href="#nonexistent"]');
      link.click();

      // Assert
      expect(globalThis.console.warn).toHaveBeenCalledWith(
        expect.stringContaining('Internal link target not found'),
        'nonexistent'
      );
    });

    test('should transform pathname to page title correctly', async () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/some-page/nested-route">Link</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({
          html: '<p>Content</p>',
          metadata: {}
        })
      });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act
      const link = document.querySelector('a[href="/some-page/nested-route"]');
      link.click();

      await Promise.resolve();
      await Promise.resolve();

      // Assert - title should be transformed from pathname
      const slideoverTitle = document.getElementById('slideover-title');
      expect(slideoverTitle.textContent).toBe('Some Page - Nested Route');
    });

    test('should handle invalid date in buildLastUpdatedHeader', async () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws">Link</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({
          html: '<p>Content</p>',
          metadata: { LastUpdatedUtc: 'invalid-date-string' }
        })
      });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act
      const link = document.querySelector('a[href="/bylaws"]');
      link.click();

      await Promise.resolve();
      await Promise.resolve();

      // Assert - should not include invalid date header
      const slideoverContent = document.getElementById('slideover-content');
      expect(slideoverContent.innerHTML).not.toContain('neba-document-last-updated-header');
      expect(slideoverContent.innerHTML).toContain('Content');
    });

    test('should handle no content returned from API', async () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws">Link</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      // Return empty string from JSON response - this triggers the !html check
      // since: const html = data.html || data.content || data;
      // when data is '', html becomes '' (falsy)
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue('')
      });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act
      const link = document.querySelector('a[href="/bylaws"]');
      link.click();

      // Wait for all promises to resolve
      await new Promise(resolve => setTimeout(resolve, 0));
      await new Promise(resolve => setTimeout(resolve, 0));
      await new Promise(resolve => setTimeout(resolve, 0));

      // Assert - should show error
      const slideoverTitle = document.getElementById('slideover-title');
      const slideoverContent = document.getElementById('slideover-content');

      expect(slideoverTitle.textContent).toContain('Error');
      expect(slideoverContent.innerHTML).toContain('No content returned');
    });

    test('should handle nested link with empty href in slideover', async () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws">Open Bylaws</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({
          html: '<a href="">Empty link</a><a href="#">Hash only</a>',
          metadata: {}
        })
      });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act - open slideover
      const initialLink = document.querySelector('a[href="/bylaws"]');
      initialLink.click();

      await Promise.resolve();
      await Promise.resolve();

      const slideoverContent = document.getElementById('slideover-content');

      // Act - click empty/hash links - should not throw
      const emptyLink = slideoverContent.querySelector('a[href=""]');
      const hashLink = slideoverContent.querySelector('a[href="#"]');

      expect(() => emptyLink.click()).not.toThrow();
      expect(() => hashLink.click()).not.toThrow();
    });

    test('should initialize without TOC list but with headings (ensures heading IDs)', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading Without ID</h1>
          <h2>Another Heading</h2>
        </div>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      const config = {
        contentId: 'content',
        tocListId: null, // No TOC list
        tocMobileListId: null,
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close'
      };

      // Act
      const result = initializeToc(config);

      // Assert - should succeed and assign IDs to headings
      expect(result).toBe(true);
      const headings = document.querySelectorAll('#content h1, #content h2');
      expect(headings[0].id).toBe('heading-0');
      expect(headings[1].id).toBe('heading-1');
    });

    test('should not close modal on Escape when modal is not active', () => {
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

      initializeToc(config);

      const modal = document.getElementById('toc-modal');

      // Modal is not active initially
      expect(modal.classList.contains('active')).toBe(false);

      // Act - press escape when modal is not active
      const escapeEvent = new KeyboardEvent('keydown', { key: 'Escape' });
      document.dispatchEvent(escapeEvent);

      // Assert - modal should still not have 'active' class (nothing changed)
      expect(modal.classList.contains('active')).toBe(false);
    });

    test('should not close slideover on Escape when slideover is not active', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      const slideover = document.getElementById('slideover');

      // Slideover is not active initially
      expect(slideover.classList.contains('active')).toBe(false);

      // Act - press escape when slideover is not active
      const escapeEvent = new KeyboardEvent('keydown', { key: 'Escape' });
      document.dispatchEvent(escapeEvent);

      // Assert - slideover should still not have 'active' class
      expect(slideover.classList.contains('active')).toBe(false);
    });

    test('should close slideover on overlay click', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover" class="active"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      document.body.style.overflow = 'hidden';

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      const slideover = document.getElementById('slideover');
      const overlay = document.getElementById('slideover-overlay');

      // Act
      overlay.click();

      // Assert
      expect(slideover.classList.contains('active')).toBe(false);
      expect(document.body.style.overflow).toBe('');
    });

    test('should scroll to hash in non-scrollable content (page scroll)', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1 id="heading1">Heading 1</h1>
        </div>
        <ul id="toc-list"></ul>
      `;

      globalThis.location.hash = '#heading1';
      globalThis.scrollY = 0;
      globalThis.scrollTo = jest.fn();

      const content = document.getElementById('content');
      // Make content non-scrollable
      Object.defineProperty(content, 'scrollHeight', { value: 100, configurable: true });
      Object.defineProperty(content, 'clientHeight', { value: 100, configurable: true });
      content.scrollTo = null;

      // Act
      scrollToHash('content', 'toc-list');

      // Assert - should call window.scrollTo
      expect(globalThis.scrollTo).toHaveBeenCalledWith(
        expect.objectContaining({
          behavior: 'smooth'
        })
      );
    });

    test('should scroll TOC container when hash scrolls to active link', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1 id="heading1">Heading 1</h1>
          <h1 id="heading2">Heading 2</h1>
        </div>
        <div class="toc-sticky" style="height: 200px; overflow: auto;">
          <ul id="toc-list">
            <li><a class="toc-link" data-target="heading1">Heading 1</a></li>
            <li><a class="toc-link" data-target="heading2">Heading 2</a></li>
          </ul>
        </div>
      `;

      globalThis.location.hash = '#heading2';

      const content = document.getElementById('content');
      content.scrollTo = jest.fn();

      const tocContainer = document.querySelector('.toc-sticky');
      tocContainer.scrollTo = jest.fn();

      // Act
      scrollToHash('content', 'toc-list');

      // Assert - should update active link and scroll TOC
      const activeLink = document.querySelector('.toc-link.active');
      expect(activeLink.dataset.target).toBe('heading2');
      expect(tocContainer.scrollTo).toHaveBeenCalledWith(
        expect.objectContaining({
          behavior: 'smooth'
        })
      );
    });

    test('should handle Cmd+Click (metaKey) in nested slideover links', async () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws">Open Bylaws</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({
          html: '<a href="/rules">Nested Link</a>',
          metadata: {}
        })
      });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act - open slideover
      const initialLink = document.querySelector('a[href="/bylaws"]');
      initialLink.click();

      await Promise.resolve();
      await Promise.resolve();

      const slideoverContent = document.getElementById('slideover-content');

      // Act - Cmd+click nested link
      const nestedLink = slideoverContent.querySelector('a[href="/rules"]');
      const clickEvent = new MouseEvent('click', {
        bubbles: true,
        cancelable: true,
        metaKey: true
      });

      const defaultPrevented = !nestedLink.dispatchEvent(clickEvent);

      // Assert - should NOT prevent default (allow browser to open in new tab)
      expect(defaultPrevented).toBe(false);
    });

    test('should update URL hash when scrolling to anchor', () => {
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

      const content = document.getElementById('content');
      content.scrollTo = jest.fn();

      // Ensure location.hash is different initially
      globalThis.location.hash = '';

      initializeToc(config);

      // Act
      const link = content.querySelector('a[href="#section1"]');
      link.click();

      // Assert - hash should be updated
      expect(globalThis.location.hash).toBe('section1');
    });

    test('should not update URL hash when already matching', () => {
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

      const content = document.getElementById('content');
      content.scrollTo = jest.fn();

      // Set hash to already match
      globalThis.location.hash = '#section1';

      initializeToc(config);

      // Act
      const link = content.querySelector('a[href="#section1"]');
      link.click();

      // Assert - hash should remain the same
      expect(globalThis.location.hash).toBe('#section1');
    });

    test('should handle scrollTocToActiveLink when no toc-sticky container', () => {
      // Arrange - no .toc-sticky element
      document.body.innerHTML = `
        <div id="content">
          <h1 id="heading1">Heading 1</h1>
          <h1 id="heading2">Heading 2</h1>
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

      // Act - click TOC link, which calls scrollTocToActiveLink
      const tocLink = document.querySelector('.toc-link[data-target="heading1"]');
      tocLink.click();

      // Assert - should not throw, and content.scrollTo should still be called
      expect(content.scrollTo).toHaveBeenCalled();
    });

    test('should handle setupMobileModal when some modal elements are missing', () => {
      // Arrange - missing toc-modal-close element
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading 1</h1>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'nonexistent-close' // Missing element
      };

      // Act & Assert - should not throw
      expect(() => initializeToc(config)).not.toThrow();
    });

    test('should handle updateActiveLinkUI when currentActiveLink exists', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content" style="height: 500px; overflow: auto;">
          <h1 id="heading1" style="margin-top: 50px;">Heading 1</h1>
          <div style="height: 500px;"></div>
          <h1 id="heading2">Heading 2</h1>
        </div>
        <div class="toc-sticky" style="height: 200px; overflow: auto;">
          <ul id="toc-list"></ul>
        </div>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
      `;

      // Mock scrollTo on the toc-sticky container
      const tocContainer = document.querySelector('.toc-sticky');
      tocContainer.scrollTo = jest.fn();

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

      // Manually set one link as active (headings have IDs heading1, heading2)
      const firstLink = document.querySelector('.toc-link[data-target="heading1"]');
      firstLink.classList.add('active');

      const content = document.getElementById('content');

      // Trigger scroll event to update active link
      const scrollEvent = new Event('scroll');
      content.dispatchEvent(scrollEvent);

      // The test verifies that the currentActiveLink removal branch is covered
      // when updating to a new active link
      expect(tocContainer.scrollTo).toHaveBeenCalled();
    });

    test('should handle URL parse errors gracefully in internal link navigation', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="javascript:void(0)">JavaScript link</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act - clicking link with non-http protocol should not throw
      const link = document.querySelector('a[href="javascript:void(0)"]');
      expect(() => link.click()).not.toThrow();
    });

    test('should handle URL parse errors in nested slideover navigation', async () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws">Open Bylaws</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({
          html: '<a href="javascript:alert()">JS Link</a>',
          metadata: {}
        })
      });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act - open slideover
      const initialLink = document.querySelector('a[href="/bylaws"]');
      initialLink.click();

      await Promise.resolve();
      await Promise.resolve();

      const slideoverContent = document.getElementById('slideover-content');

      // Act - click javascript: link - should not throw
      const jsLink = slideoverContent.querySelector('a[href="javascript:alert()"]');
      expect(() => jsLink.click()).not.toThrow();
    });

    test('should scroll to top of slideover content when no hash in URL', async () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws">Internal Link</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({
          html: '<p>Document content</p>',
          metadata: {}
        })
      });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      const slideoverContent = document.getElementById('slideover-content');
      slideoverContent.scrollTop = 500; // Simulate scrolled position

      // Act
      const link = document.querySelector('a[href="/bylaws"]');
      link.click();

      await Promise.resolve();
      await Promise.resolve();

      // Assert - scrollTop should be reset to 0
      expect(slideoverContent.scrollTop).toBe(0);
    });
  });

  describe('helper functions', () => {
    test('should handle JSON parse error when fetching slideover content', async () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws">Open Bylaws</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      // Simulate JSON parse error
      globalThis.fetch.mockResolvedValue({
        ok: true,
        json: jest.fn().mockRejectedValue(new Error('Invalid JSON'))
      });

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act
      const link = document.querySelector('a[href="/bylaws"]');
      link.click();

      await Promise.resolve();
      await Promise.resolve();
      await Promise.resolve();

      // Assert
      const slideoverTitle = document.getElementById('slideover-title');
      const slideoverContent = document.getElementById('slideover-content');

      expect(slideoverTitle.textContent).toContain('Error');
      expect(slideoverContent.innerHTML).toContain('Invalid JSON');
    });

    test('should handle network error when fetching slideover content', async () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1>Heading</h1>
          <a href="/bylaws">Open Bylaws</a>
        </div>
        <ul id="toc-list"></ul>
        <ul id="toc-mobile-list"></ul>
        <button id="toc-mobile-button"></button>
        <div id="toc-modal"></div>
        <div id="toc-modal-overlay"></div>
        <button id="toc-modal-close"></button>
        <div id="slideover"></div>
        <div id="slideover-overlay"></div>
        <button id="slideover-close"></button>
        <div id="slideover-title"></div>
        <div id="slideover-content"></div>
      `;

      globalThis.fetch.mockRejectedValue(new Error('Network error'));

      const config = {
        contentId: 'content',
        tocListId: 'toc-list',
        tocMobileListId: 'toc-mobile-list',
        tocMobileButtonId: 'toc-mobile-button',
        tocModalId: 'toc-modal',
        tocModalOverlayId: 'toc-modal-overlay',
        tocModalCloseId: 'toc-modal-close',
        slideoverId: 'slideover',
        slideoverOverlayId: 'slideover-overlay',
        slideoverCloseId: 'slideover-close',
        slideoverTitleId: 'slideover-title',
        slideoverContentId: 'slideover-content'
      };

      initializeToc(config);

      // Act
      const link = document.querySelector('a[href="/bylaws"]');
      link.click();

      await Promise.resolve();
      await Promise.resolve();

      // Assert
      const slideoverTitle = document.getElementById('slideover-title');
      const slideoverContent = document.getElementById('slideover-content');

      expect(slideoverTitle.textContent).toContain('Error');
      expect(slideoverContent.innerHTML).toContain('Network error');
    });

    test('should initialize without slideover elements', () => {
      // Arrange
      document.body.innerHTML = `
        <div id="content">
          <h1 id="heading1">Heading 1</h1>
          <a href="#heading1">Anchor link</a>
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
        // No slideover config
      };

      const content = document.getElementById('content');
      content.scrollTo = jest.fn();

      // Act
      const result = initializeToc(config);

      // Assert
      expect(result).toBe(true);

      const link = content.querySelector('a[href="#heading1"]');

      // Act - click anchor link (should use fallback navigation)
      const clickEvent = new MouseEvent('click', { bubbles: true, cancelable: true });
      link.dispatchEvent(clickEvent);

      // Assert - should prevent default and call scrollTo
      expect(content.scrollTo).toHaveBeenCalledWith(
        expect.objectContaining({
          behavior: 'smooth'
        })
      );
    });
  });
});
