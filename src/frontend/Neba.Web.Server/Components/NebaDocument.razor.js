/**
 * NebaDocument - Reusable component for displaying documents with table of contents
 * Provides TOC generation, scroll spy, smooth scrolling, and hash navigation
 */

import { trackError, createTimer } from '../js/telemetry-helper.js';

/**
 * Escapes HTML special characters to prevent XSS attacks
 * @param {string} text - The text to escape
 * @returns {string} The escaped text safe for HTML insertion
 */
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

/**
 * Initialize the table-of-contents and related interactions.
 * Accepts a single `config` object for maintainability. For backward
 * compatibility, the function still supports the original positional
 * argument form (legacy callers) by detecting a string first argument
 * and converting it into the `config` object.
 *
 * Config object shape:
 * {
 *   contentId,
 *   tocListId,
 *   tocMobileListId,
 *   tocMobileButtonId,
 *   tocModalId,
 *   tocModalOverlayId,
 *   tocModalCloseId,
 *   headingLevels = 'h1, h2',
 *   slideoverId = null,
 *   slideoverOverlayId = null,
 *   slideoverCloseId = null,
 *   slideoverTitleId = null,
 *   slideoverContentId = null
 * }
 */
/**
 * Normalizes config from legacy positional args or object form
 * @param {string|Object} configOrContentId - Config object or first positional arg
 * @returns {Object} Normalized config object
 */
function normalizeConfig(configOrContentId) {
    if (typeof configOrContentId !== 'string') {
        return configOrContentId;
    }

    const args = Array.from(arguments);
    const [
        contentId,
        tocListId,
        tocMobileListId,
        tocMobileButtonId,
        tocModalId,
        tocModalOverlayId,
        tocModalCloseId,
        headingLevels = 'h1, h2',
        slideoverId = null,
        slideoverOverlayId = null,
        slideoverCloseId = null,
        slideoverTitleId = null,
        slideoverContentId = null
    ] = args;

    return {
        contentId,
        tocListId,
        tocMobileListId,
        tocMobileButtonId,
        tocModalId,
        tocModalOverlayId,
        tocModalCloseId,
        headingLevels,
        slideoverId,
        slideoverOverlayId,
        slideoverCloseId,
        slideoverTitleId,
        slideoverContentId
    };
}

/**
 * Validates required DOM elements exist
 * @param {Object} elements - Object with element references
 * @returns {boolean} True if validation passed
 */
function validateElements(elements) {
    const { content, contentId, tocList, tocListId } = elements;

    if (!content) {
        console.error('[NebaDocument] Content element not found:', contentId);
        return false;
    }

    if (tocListId && !tocList) {
        console.error('[NebaDocument] TOC list element not found:', tocListId);
        return false;
    }

    return true;
}

/**
 * Generates and populates TOC HTML from headings
 * @param {NodeList} headings - Collection of heading elements
 * @param {HTMLElement} tocList - TOC list container
 * @param {HTMLElement|null} tocMobileList - Mobile TOC list container
 * @returns {boolean} True if TOC was generated
 */
function generateAndPopulateToc(headings, tocList, tocMobileList) {
    if (!tocList || headings.length === 0) {
        return false;
    }

    let tocHtml = '<ul class="toc-list">';

    headings.forEach((heading, index) => {
        const id = heading.id || `heading-${index}`;
        if (!heading.id) {
            heading.id = id;
        }

        const level = heading.tagName.toLowerCase();
        const text = escapeHtml(heading.textContent);
        const className = level === 'h1' ? 'toc-item-h1' : `toc-item-${level}`;

        tocHtml += `<li class="${className}">
            <a href="#${id}" class="toc-link" data-target="${id}">${text}</a>
        </li>`;
    });

    tocHtml += '</ul>';
    tocList.innerHTML = tocHtml;

    if (tocMobileList) {
        tocMobileList.innerHTML = tocHtml;
    }

    return true;
}

/**
 * Assigns IDs to headings that don't have them
 * @param {NodeList} headings - Collection of heading elements
 */
function ensureHeadingIds(headings) {
    headings.forEach((heading, index) => {
        if (!heading.id) {
            heading.id = `heading-${index}`;
        }
    });
}

/**
 * Sets up mobile modal interactions
 * @param {HTMLElement} tocMobileButton - Button to open modal
 * @param {HTMLElement} tocModal - Modal container
 * @param {HTMLElement} tocModalOverlay - Modal overlay
 * @param {HTMLElement} tocModalClose - Close button
 * @param {HTMLElement|null} tocMobileList - Mobile TOC list
 */
function setupMobileModal(tocMobileButton, tocModal, tocModalOverlay, tocModalClose, tocMobileList) {
    if (!tocMobileButton || !tocModal || !tocModalOverlay || !tocModalClose) {
        return;
    }

    const closeModal = () => {
        tocModal.classList.remove('active');
        document.body.style.overflow = '';
    };

    tocMobileButton.addEventListener('click', () => {
        tocModal.classList.add('active');
        document.body.style.overflow = 'hidden';
    });

    tocModalClose.addEventListener('click', closeModal);
    tocModalOverlay.addEventListener('click', closeModal);

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && tocModal.classList.contains('active')) {
            closeModal();
        }
    });

    if (tocMobileList) {
        setupMobileTocLinkHandlers(tocMobileList, tocModal, closeModal);
    }
}

/**
 * Sets up click handlers for mobile TOC links
 * @param {HTMLElement} tocMobileList - Mobile TOC list
 * @param {HTMLElement} tocModal - Modal container
 * @param {Function} closeModal - Function to close modal
 */
function setupMobileTocLinkHandlers(tocMobileList, tocModal, closeModal) {
    const mobileTocLinks = tocMobileList.querySelectorAll('.toc-link');

    mobileTocLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const targetId = link.dataset.target;
            const targetElement = document.getElementById(targetId);

            if (targetElement) {
                closeModal();
                setTimeout(() => {
                    const elementPosition = targetElement.getBoundingClientRect().top;
                    const navbarHeight = 80;
                    const offsetPosition = elementPosition + globalThis.pageYOffset - navbarHeight;

                    globalThis.scrollTo({
                        top: offsetPosition,
                        behavior: 'smooth'
                    });
                }, 300);
            }
        });
    });
}

/**
 * Scrolls the TOC container to show the active link
 * @param {HTMLElement} link - The TOC link element to scroll to
 */
function scrollTocToActiveLink(link) {
    const tocContainer = document.querySelector('.toc-sticky');
    if (!tocContainer || !link) return;

    const tocRect = tocContainer.getBoundingClientRect();
    const linkRect = link.getBoundingClientRect();
    const linkTop = linkRect.top - tocRect.top + tocContainer.scrollTop;
    const targetScroll = linkTop - 60;

    tocContainer.scrollTo({
        top: Math.max(0, targetScroll),
        behavior: 'smooth'
    });
}

/**
 * Sets up scroll spy functionality for TOC
 * @param {HTMLElement} content - Content container
 * @param {HTMLElement} tocList - TOC list container
 * @param {NodeList} headings - Collection of heading elements
 */
function setupScrollSpy(content, tocList, headings) {
    if (!tocList) {
        return;
    }

    const tocLinks = tocList.querySelectorAll('.toc-link');
    let currentActiveLink = null;

    function updateActiveLink() {
        const contentRect = content.getBoundingClientRect();
        const activeHeading = findActiveHeading(headings, contentRect);

        if (activeHeading) {
            updateActiveLinkUI(activeHeading.id, tocList, currentActiveLink);
            currentActiveLink = tocList.querySelector(`[data-target="${activeHeading.id}"]`);
        }
    }

    tocLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const targetId = link.dataset.target;
            const targetElement = document.getElementById(targetId);

            if (targetElement) {
                const contentRect = content.getBoundingClientRect();
                const targetRect = targetElement.getBoundingClientRect();
                const currentScroll = content.scrollTop;
                const offset = 20;
                const scrollPosition = currentScroll + (targetRect.top - contentRect.top) - offset;

                content.scrollTo({
                    top: scrollPosition,
                    behavior: 'smooth'
                });

                scrollTocToActiveLink(link);
            }
        });
    });

    let ticking = false;
    content.addEventListener('scroll', () => {
        if (!ticking) {
            globalThis.requestAnimationFrame(() => {
                updateActiveLink();
                ticking = false;
            });
            ticking = true;
        }
    });

    updateActiveLink();
}

/**
 * Finds the heading currently visible in viewport
 * @param {NodeList} headings - Collection of heading elements
 * @param {DOMRect} contentRect - Bounding rect of content container
 * @returns {HTMLElement|null} The active heading element
 */
function findActiveHeading(headings, contentRect) {
    let activeHeading = null;
    let minDistance = Infinity;

    headings.forEach(heading => {
        const headingRect = heading.getBoundingClientRect();
        const distanceFromTop = headingRect.top - contentRect.top;

        if (distanceFromTop <= 100 && distanceFromTop >= -headingRect.height) {
            if (Math.abs(distanceFromTop) < minDistance) {
                minDistance = Math.abs(distanceFromTop);
                activeHeading = heading;
            }
        }
    });

    if (!activeHeading) {
        headings.forEach(heading => {
            const headingRect = heading.getBoundingClientRect();
            const distanceFromTop = headingRect.top - contentRect.top;

            if (distanceFromTop >= 0 && distanceFromTop < minDistance) {
                minDistance = distanceFromTop;
                activeHeading = heading;
            }
        });
    }

    return activeHeading;
}

/**
 * Updates the active link styling in TOC
 * @param {string} targetId - ID of the target element
 * @param {HTMLElement} tocList - TOC list container
 * @param {HTMLElement|null} currentActiveLink - Currently active link
 */
function updateActiveLinkUI(targetId, tocList, currentActiveLink) {
    const newActiveLink = tocList.querySelector(`[data-target="${targetId}"]`);

    if (newActiveLink !== currentActiveLink) {
        if (currentActiveLink) {
            currentActiveLink.classList.remove('active');
        }
        if (newActiveLink) {
            newActiveLink.classList.add('active');

            const tocContainer = document.querySelector('.toc-sticky');
            if (tocContainer) {
                const tocRect = tocContainer.getBoundingClientRect();
                const linkRect = newActiveLink.getBoundingClientRect();
                const linkTop = linkRect.top - tocRect.top + tocContainer.scrollTop;
                const targetScroll = linkTop - 60;

                tocContainer.scrollTo({
                    top: Math.max(0, targetScroll),
                    behavior: 'smooth'
                });
            }
        }
    }
}

export function initializeToc(configOrContentId) {
    const config = normalizeConfig(...arguments);

    const {
        contentId,
        tocListId,
        tocMobileListId,
        tocMobileButtonId,
        tocModalId,
        tocModalOverlayId,
        tocModalCloseId,
        headingLevels = 'h1, h2',
        slideoverId = null,
        slideoverOverlayId = null,
        slideoverCloseId = null,
        slideoverTitleId = null,
        slideoverContentId = null
    } = config || {};

    console.log('[NebaDocument] Initializing TOC...');
    console.log('[NebaDocument] Content ID:', contentId);
    console.log('[NebaDocument] TOC List ID:', tocListId);
    console.log('[NebaDocument] Mobile TOC List ID:', tocMobileListId);
    console.log('[NebaDocument] Heading Levels:', headingLevels);

    const content = document.getElementById(contentId);
    const tocList = document.getElementById(tocListId);
    const tocMobileList = document.getElementById(tocMobileListId);
    const tocMobileButton = document.getElementById(tocMobileButtonId);
    const tocModal = document.getElementById(tocModalId);
    const tocModalOverlay = document.getElementById(tocModalOverlayId);
    const tocModalClose = document.getElementById(tocModalCloseId);

    if (!validateElements({ content, contentId, tocList, tocListId })) {
        return false;
    }

    const hasToc = !!tocList;
    const headings = content.querySelectorAll(headingLevels);

    console.log('[NebaDocument] Found headings:', headings.length);

    if (hasToc && headings.length > 0) {
        generateAndPopulateToc(headings, tocList, tocMobileList);
    } else {
        if (!hasToc) {
            console.log('[NebaDocument] TOC not present, skipping TOC generation but will set up link navigation');
        } else if (headings.length === 0) {
            console.warn('[NebaDocument] No headings found in content');
            return false;
        }

        ensureHeadingIds(headings);
    }

    setupMobileModal(tocMobileButton, tocModal, tocModalOverlay, tocModalClose, tocMobileList);
    setupScrollSpy(content, tocList, headings);

    if (slideoverId && slideoverOverlayId && slideoverCloseId && slideoverTitleId && slideoverContentId) {
        setupInternalLinkNavigation(content, slideoverId, slideoverOverlayId, slideoverCloseId, slideoverTitleId, slideoverContentId);
    } else {
        setupInternalLinkNavigation(content);
    }

    console.log('[NebaDocument] TOC initialized successfully');
    return true;
}

/**
 * Sets up navigation for internal links within the document content
 * Handles both anchor links (same-page) and internal page links (slide-over)
 * @param {HTMLElement} content - The document content container
 * @param {string} slideoverId - ID of the slide-over container (optional)
 * @param {string} slideoverOverlayId - ID of the slide-over overlay (optional)
 * @param {string} slideoverCloseId - ID of the slide-over close button (optional)
 * @param {string} slideoverTitleId - ID of the slide-over title element (optional)
 * @param {string} slideoverContentId - ID of the slide-over content element (optional)
 */
function setupInternalLinkNavigation(content, slideoverId, slideoverOverlayId, slideoverCloseId, slideoverTitleId, slideoverContentId) {
    // Find all links within the content
    const contentLinks = content.querySelectorAll('a[href]');

    console.log('[NebaDocument] Setting up internal link navigation for', contentLinks.length, 'links');

    // Get slide-over elements if IDs are provided
    const slideover = slideoverId ? document.getElementById(slideoverId) : null;
    const slideoverOverlay = slideoverOverlayId ? document.getElementById(slideoverOverlayId) : null;
    const slideoverClose = slideoverCloseId ? document.getElementById(slideoverCloseId) : null;
    const slideoverTitle = slideoverTitleId ? document.getElementById(slideoverTitleId) : null;
    const slideoverContent = slideoverContentId ? document.getElementById(slideoverContentId) : null;

    // Set up slide-over close handlers if elements exist
    if (slideover && slideoverOverlay && slideoverClose) {
        const closeSlideover = () => {
            slideover.classList.remove('active');
            document.body.style.overflow = ''; // Restore scrolling
        };

        slideoverClose.addEventListener('click', closeSlideover);
        slideoverOverlay.addEventListener('click', closeSlideover);

        // Close on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && slideover.classList.contains('active')) {
                closeSlideover();
            }
        });
    }

    contentLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            const href = link.getAttribute('href');

            // Skip if no href
            if (!href || href === '#') {
                return;
            }

            // Allow Ctrl/Cmd+Click to open in new tab (browser default)
            if (e.ctrlKey || e.metaKey) {
                return;
            }

            // Check if it's a hash link (anchor navigation)
            if (href.startsWith('#')) {
                e.preventDefault();
                handleAnchorNavigation(content, href);
                return;
            }

            // Check if it's an internal link (same origin)
            try {
                const linkUrl = new URL(href, globalThis.location.href);
                const isInternal = linkUrl.origin === globalThis.location.origin;
                const isExternalProtocol = linkUrl.protocol === 'mailto:' || linkUrl.protocol === 'tel:';

                if (isInternal && !isExternalProtocol && slideover && slideoverContent && slideoverTitle) {
                    // Internal link - open in slide-over
                    e.preventDefault();
                    console.log('[NebaDocument] Opening internal link in slide-over:', href);
                    openInSlideover(linkUrl, slideover, slideoverContent, slideoverTitle, content);
                }
                // Otherwise, let the link work normally (external links, downloads, etc.)
            } catch (err) {
                console.error('[NebaDocument] Error parsing link URL:', href, err);
            }
        });
    });
}

/**
 * Handles anchor navigation within the current document
 * @param {HTMLElement} content - The document content container
 * @param {string} href - The hash href (e.g., "#section-1")
 */
function handleAnchorNavigation(content, href) {
    // Extract the target ID (remove the '#')
    const targetId = href.substring(1);
    const targetElement = document.getElementById(targetId);

    if (targetElement) {
        console.log('[NebaDocument] Scrolling to internal link target:', targetId);

        // Determine if we should scroll the content container or the whole page
        // Check if content is scrollable by checking if it has scroll height greater than client height
        // In test environments where layout isn't calculated, we assume a content container with scrollTo is scrollable
        const isContentScrollable =
            (content.scrollHeight > content.clientHeight) ||
            (content.scrollHeight === 0 && content.scrollTo); // Fallback for test environments

        if (isContentScrollable) {
            // Content container is scrollable: scroll within the container
            const contentRect = content.getBoundingClientRect();
            const targetRect = targetElement.getBoundingClientRect();
            const currentScroll = content.scrollTop;

            // Calculate the scroll position
            const offset = 20; // Small offset from the top of the container
            const scrollPosition = currentScroll + (targetRect.top - contentRect.top) - offset;

            content.scrollTo({
                top: scrollPosition,
                behavior: 'smooth'
            });
        } else {
            // Scroll the whole page, accounting for sticky navbar
            const navbarHeight = 80; // Height of sticky navbar
            const offset = 10; // Additional offset for spacing
            const targetPosition = targetElement.getBoundingClientRect().top + globalThis.scrollY - navbarHeight - offset;

            globalThis.scrollTo({
                top: targetPosition,
                behavior: 'smooth'
            });
        }

        // Update URL hash without triggering navigation
        // Simply setting location.hash updates the URL without full page navigation
        // and works correctly with Blazor's enhanced navigation
        if (globalThis.location.hash !== `#${targetId}`) {
            globalThis.location.hash = targetId;
        }
    } else {
        console.warn('[NebaDocument] Internal link target not found:', targetId);
    }
}

/**
 * Maps route paths to friendly page titles
 * @param {string} pathname - The route pathname (e.g., "bylaws", "tournaments/rules")
 * @returns {string} The friendly page title
 */
function getPageTitle(pathname) {
    // Map of routes to page titles
    const titleMap = {
        'bylaws': 'NEBA Bylaws',
        'tournaments/rules': 'NEBA Tournament Rules',
        'rules': 'NEBA Rules',
        // Add more mappings as needed
    };

    // Return the mapped title or create a title from the pathname
    return titleMap[pathname] || pathname
        .split('/')
        .map(part => part.replaceAll('-', ' '))
        .map(part => part.replaceAll(/\b\w/g, l => l.toUpperCase()))
        .join(' - ');
}

/**
 * Builds the last updated header HTML for slide-out documents
 * @param {Object} metadata - The document metadata containing LastUpdatedUtc and LastUpdatedBy
 * @returns {string} HTML string for the last updated header
 */
function buildLastUpdatedHeader(metadata) {
    if (!metadata?.LastUpdatedUtc) {
        return '';
    }

    try {
        // Parse the UTC date
        const utcDate = new Date(metadata.LastUpdatedUtc);

        // Check if date is valid
        if (Number.isNaN(utcDate.getTime())) {
            return '';
        }

        // Format date as yyyy-MM-dd
        const year = utcDate.getFullYear();
        const month = String(utcDate.getMonth() + 1).padStart(2, '0');
        const day = String(utcDate.getDate()).padStart(2, '0');
        const dateStr = `${year}-${month}-${day}`;

        // For now, authorization is always true
        // TODO: Replace with actual authorization check when implemented
        const isAuthorized = true;

        let lastUpdatedText = dateStr;

        if (isAuthorized && metadata.LastUpdatedBy) {
            // Format time as hh:mmtt (12-hour with AM/PM)
            const hours24 = utcDate.getHours();
            const hours12 = hours24 % 12 || 12;
            const minutes = String(utcDate.getMinutes()).padStart(2, '0');
            const ampm = hours24 >= 12 ? 'PM' : 'AM';
            const timeStr = `${String(hours12).padStart(2, '0')}:${minutes}${ampm}`;

            lastUpdatedText = `${dateStr} ${timeStr} by ${escapeHtml(metadata.LastUpdatedBy)}`;
        }

        return `<div class="neba-document-last-updated-header">${lastUpdatedText}</div>`;
    } catch (error) {
        console.error('[NebaDocument] Error building last updated header:', error);
        return '';
    }
}

/**
 * Opens an internal link in the slide-over panel
 * @param {URL} url - The URL to open
 * @param {HTMLElement} slideover - The slide-over container
 * @param {HTMLElement} slideoverContent - The slide-over content container
 * @param {HTMLElement} slideoverTitle - The slide-over title element
 * @param {HTMLElement} originalContent - The original document content (for context)
 */
async function openInSlideover(url, slideover, slideoverContent, slideoverTitle, originalContent) {
    const timer = createTimer('document.slideover_open');

    // Show loading state
    slideoverTitle.textContent = 'Loading...';
    slideoverContent.innerHTML = '<div style="padding: 2rem; text-align: center; color: var(--neba-gray-600);">Loading document...</div>';
    slideover.classList.add('active');
    document.body.style.overflow = 'hidden'; // Prevent background scrolling

    try {
        // Convert page route to API endpoint
        // e.g., /bylaws -> /api/documents/bylaws
        const pathname = url.pathname.replace(/^\//, ''); // Remove leading slash
        const apiUrl = `/api/documents/${pathname}`;

        console.log('[NebaDocument] Fetching document from API:', apiUrl);

        // Fetch the document content from the API
        const response = await fetch(apiUrl, {
            headers: {
                'Accept': 'application/json'
            }
        });

        if (!response.ok) {
            const error = new Error(`Failed to load: ${response.status} ${response.statusText}`);
            trackError(error.message, 'document.slideover', error.stack);
            timer.stop(false, { error: 'api_error', status_code: response.status, document: pathname });
            throw error;
        }

        const data = await response.json();

        // The API should return an object with an 'html' property
        const html = data.html || data.content || data;
        const metadata = data.metadata || {};

        if (!html) {
            const error = new Error('No content returned from API');
            trackError(error.message, 'document.slideover', error.stack);
            timer.stop(false, { error: 'no_content', document: pathname });
            throw error;
        }

        // Get the page title from the route name
        // This will show "NEBA Bylaws" instead of "NEBA Board of Directors"
        const pageTitle = getPageTitle(pathname);

        slideoverTitle.textContent = pageTitle;

        // Build the last updated header for slide-out
        const lastUpdatedHtml = buildLastUpdatedHeader(metadata);

        // Set the content with last updated header at the top
        // WARNING: This sets HTML from an API response. Ensure the API returns properly sanitized HTML
        // or the source documents are trusted. Consider using DOMPurify if the content is user-generated.
        slideoverContent.innerHTML = lastUpdatedHtml + html;

        // If there's a hash in the URL, scroll to it
        if (url.hash) {
            setTimeout(() => {
                const targetId = url.hash.substring(1);
                const targetElement = slideoverContent.querySelector(`#${targetId}`);
                if (targetElement) {
                    targetElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            }, 100);
        } else {
            // Scroll to top of slide-over content
            slideoverContent.scrollTop = 0;
        }

        // Set up nested link navigation within the slide-over
        setupNestedLinkNavigation(slideoverContent, slideover, slideoverTitle, originalContent);

        // Track successful document load
        timer.stop(true, {
            document: pathname,
            has_hash: !!url.hash,
            content_length: html.length
        });
    } catch (error) {
        console.error('[NebaDocument] Error loading slide-over content:', error);
        trackError(error.message, 'document.slideover', error.stack);
        timer.stop(false, { error: error.message });
        slideoverTitle.textContent = 'Error Loading Document';
        slideoverContent.innerHTML = `
            <div style="padding: 2rem; text-align: center;">
                <p style="color: var(--neba-accent-red); margin-bottom: 1rem;">Failed to load document</p>
                <p style="color: var(--neba-gray-600); font-size: 0.875rem;">${escapeHtml(error.message)}</p>
            </div>
        `;
    }
}

/**
 * Sets up link navigation within the slide-over content
 * @param {HTMLElement} slideoverContent - The slide-over content container
 * @param {HTMLElement} slideover - The slide-over container
 * @param {HTMLElement} slideoverTitle - The slide-over title element
 * @param {HTMLElement} originalContent - The original document content
 */
function setupNestedLinkNavigation(slideoverContent, slideover, slideoverTitle, originalContent) {
    const links = slideoverContent.querySelectorAll('a[href]');

    links.forEach(link => {
        link.addEventListener('click', (e) => {
            const href = link.getAttribute('href');

            if (!href || href === '#') {
                return;
            }

            // Allow Ctrl/Cmd+Click to open in new tab
            if (e.ctrlKey || e.metaKey) {
                return;
            }

            // Handle hash links within the slide-over
            if (href.startsWith('#')) {
                e.preventDefault();
                const targetId = href.substring(1);
                const targetElement = slideoverContent.querySelector(`#${targetId}`);
                if (targetElement) {
                    targetElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
                return;
            }

            // Handle internal page links - replace slide-over content
            try {
                const linkUrl = new URL(href, globalThis.location.href);
                const isInternal = linkUrl.origin === globalThis.location.origin;
                const isExternalProtocol = linkUrl.protocol === 'mailto:' || linkUrl.protocol === 'tel:';

                if (isInternal && !isExternalProtocol) {
                    e.preventDefault();
                    console.log('[NebaDocument] Opening nested internal link in slide-over:', href);
                    openInSlideover(linkUrl, slideover, slideoverContent, slideoverTitle, originalContent);
                }
            } catch (err) {
                console.error('[NebaDocument] Error parsing nested link URL:', href, err);
            }
        });
    });
}

export function scrollToHash(contentId, tocListId) {
    // Check if there's a hash in the URL
    const hash = globalThis.location.hash;

    if (!hash) {
        console.log('[NebaDocument] No hash in URL');
        return;
    }

    // Remove the '#' from the hash to get the ID
    const targetId = hash.substring(1);
    console.log('[NebaDocument] Found hash in URL:', targetId);

    const content = document.getElementById(contentId);
    const targetElement = document.getElementById(targetId);

    if (!content) {
        console.error('[NebaDocument] Content container not found:', contentId);
        return;
    }

    if (!targetElement) {
        console.error('[NebaDocument] Target element not found:', targetId);
        return;
    }

    // Determine if we should scroll the content container or the whole page
    // Check if content is scrollable by checking if it has scroll height greater than client height
    // In test environments where layout isn't calculated, we assume a content container with scrollTo is scrollable
    const isContentScrollable =
        (content.scrollHeight > content.clientHeight) ||
        (content.scrollHeight === 0 && content.scrollTo); // Fallback for test environments

    console.log('[NebaDocument] Scrolling to position, content scrollable:', isContentScrollable);

    if (isContentScrollable) {
        // Content container is scrollable: scroll within the container
        const contentRect = content.getBoundingClientRect();
        const targetRect = targetElement.getBoundingClientRect();
        const currentScroll = content.scrollTop;
        const offset = 20;
        const scrollPosition = currentScroll + (targetRect.top - contentRect.top) - offset;

        content.scrollTo({
            top: scrollPosition,
            behavior: 'smooth'
        });
    } else {
        // Mobile or no TOC: scroll the whole page, accounting for sticky navbar
        const navbarHeight = 80;
        const offset = 10; // Additional offset for spacing
        const targetPosition = targetElement.getBoundingClientRect().top + globalThis.scrollY - navbarHeight - offset;

        globalThis.scrollTo({
            top: targetPosition,
            behavior: 'smooth'
        });
    }

    // Also update the active link in TOC
    const tocList = document.getElementById(tocListId);
    if (tocList) {
        const activeLink = tocList.querySelector('.toc-link.active');
        const newActiveLink = tocList.querySelector(`[data-target="${targetId}"]`);

        if (activeLink) {
            activeLink.classList.remove('active');
        }
        if (newActiveLink) {
            newActiveLink.classList.add('active');

            // Scroll TOC to show the active link
            const tocContainer = document.querySelector('.toc-sticky');
            if (tocContainer) {
                const tocRect = tocContainer.getBoundingClientRect();
                const linkRect = newActiveLink.getBoundingClientRect();

                // Calculate the position of the link relative to the TOC container
                const linkTop = linkRect.top - tocRect.top + tocContainer.scrollTop;

                // Try to position the link near the top, but with some padding
                const targetScroll = linkTop - 60; // 60px from the top to leave some context

                tocContainer.scrollTo({
                    top: Math.max(0, targetScroll),
                    behavior: 'smooth'
                });
            }
        }
    }
}
