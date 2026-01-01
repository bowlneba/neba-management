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
