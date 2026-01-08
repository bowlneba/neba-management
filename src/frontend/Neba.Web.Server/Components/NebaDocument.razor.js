/**
 * NebaDocument - Reusable component for displaying documents with table of contents
 * Provides TOC generation, scroll spy, smooth scrolling, and hash navigation
 */

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
export function initializeToc(configOrContentId) {
    // Backward compatibility: support legacy positional args
    let config = configOrContentId;
    if (typeof configOrContentId === 'string') {
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

        config = {
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

    if (!content) {
        console.error('[NebaDocument] Content element not found:', contentId);
        return false;
    }

    // TOC is optional - if not present, we'll still set up link navigation
    const hasToc = !!tocList;

    // Extract headings based on the provided levels - need this for both TOC and link navigation
    const headings = content.querySelectorAll(headingLevels);

    console.log('[NebaDocument] Found headings:', headings.length);

    if (hasToc && headings.length > 0) {
        // Build the table of contents
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

        // Also populate the mobile TOC
        if (tocMobileList) {
            tocMobileList.innerHTML = tocHtml;
        }
    } else {
        if (!hasToc) {
            console.log('[NebaDocument] TOC not present, skipping TOC generation but will set up link navigation');
        } else if (headings.length === 0) {
            console.warn('[NebaDocument] No headings found in content');
        }

        // Still need to assign IDs to headings for anchor navigation to work
        headings.forEach((heading, index) => {
            if (!heading.id) {
                heading.id = `heading-${index}`;
            }
        });
    }

    // Mobile modal functionality
    if (tocMobileButton && tocModal && tocModalOverlay && tocModalClose) {
        // Open modal
        tocMobileButton.addEventListener('click', () => {
            tocModal.classList.add('active');
            document.body.style.overflow = 'hidden'; // Prevent background scrolling
        });

        // Close modal functions
        const closeModal = () => {
            tocModal.classList.remove('active');
            document.body.style.overflow = ''; // Restore scrolling
        };

        tocModalClose.addEventListener('click', closeModal);
        tocModalOverlay.addEventListener('click', closeModal);

        // Close on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && tocModal.classList.contains('active')) {
                closeModal();
            }
        });

        // Close modal when clicking a TOC link in mobile
        if (tocMobileList) {
            const mobileTocLinks = tocMobileList.querySelectorAll('.toc-link');
                mobileTocLinks.forEach(link => {
                link.addEventListener('click', (e) => {
                    e.preventDefault();
                    const targetId = link.dataset.target;
                    const targetElement = document.getElementById(targetId);

                    if (targetElement) {
                        // Close the modal first
                        closeModal();

                        // Scroll to the target element on the page (using globalThis scroll on mobile)
                        setTimeout(() => {
                            // Get the position of the element
                            const elementPosition = targetElement.getBoundingClientRect().top;
                            // Account for sticky navbar height (~72px) plus some padding
                            const navbarHeight = 80;
                            const offsetPosition = elementPosition + globalThis.pageYOffset - navbarHeight;

                            globalThis.scrollTo({
                                top: offsetPosition,
                                behavior: 'smooth'
                            });
                        }, 300); // Small delay to allow modal to close
                    }
                });
            });
        }
    }

    // Add scroll spy functionality
    const tocLinks = tocList.querySelectorAll('.toc-link');
    let currentActiveLink = null;

    function updateActiveLink() {
        const contentRect = content.getBoundingClientRect();

        // Find the heading that's currently at the top of the viewport (with some offset)
        let activeHeading = null;
        let minDistance = Infinity;

        headings.forEach(heading => {
            const headingRect = heading.getBoundingClientRect();
            // Calculate distance from heading to the top of the content container
            const distanceFromTop = headingRect.top - contentRect.top;

            // If heading is visible or just above the viewport, and closest to top
            if (distanceFromTop <= 100 && distanceFromTop >= -headingRect.height) {
                if (Math.abs(distanceFromTop) < minDistance) {
                    minDistance = Math.abs(distanceFromTop);
                    activeHeading = heading;
                }
            }
        });

        // If no heading found near top, use the first visible one
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

        if (activeHeading) {
            const targetId = activeHeading.id;
            const newActiveLink = tocList.querySelector(`[data-target="${targetId}"]`);

            if (newActiveLink !== currentActiveLink) {
                if (currentActiveLink) {
                    currentActiveLink.classList.remove('active');
                }
                if (newActiveLink) {
                    newActiveLink.classList.add('active');
                    currentActiveLink = newActiveLink;

                    // Auto-scroll the TOC to show the active link
                    scrollTocToActiveLink(newActiveLink);
                }
            }
        }
    }

    // Helper function to scroll TOC to show active link
    function scrollTocToActiveLink(link) {
        const tocContainer = document.querySelector('.toc-sticky');
        if (!tocContainer || !link) return;

        const tocRect = tocContainer.getBoundingClientRect();
        const linkRect = link.getBoundingClientRect();

        // Calculate the position of the link relative to the TOC container
        const linkTop = linkRect.top - tocRect.top + tocContainer.scrollTop;

        // Try to position the link near the top, but with some padding
        const targetScroll = linkTop - 60; // 60px from the top to leave some context

        tocContainer.scrollTo({
            top: Math.max(0, targetScroll),
            behavior: 'smooth'
        });
    }

    // Smooth scroll to section when clicking TOC links
    tocLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const targetId = link.dataset.target;
            const targetElement = document.getElementById(targetId);

            if (targetElement) {
                // Get the position of the target relative to the scrollable content
                const contentRect = content.getBoundingClientRect();
                const targetRect = targetElement.getBoundingClientRect();
                const currentScroll = content.scrollTop;

                // Calculate the scroll position: current scroll + (target position - content position) - offset
                const offset = 20; // Small offset from the top of the container
                const scrollPosition = currentScroll + (targetRect.top - contentRect.top) - offset;

                content.scrollTo({
                    top: scrollPosition,
                    behavior: 'smooth'
                });

                // Scroll the TOC to show the clicked link
                scrollTocToActiveLink(link);
            }
        });
    });

    // Listen for scroll events on the content container
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

    // Initial update
    updateActiveLink();

    // Handle internal links in content (both anchor navigation and page links)
    if (slideoverId && slideoverOverlayId && slideoverCloseId && slideoverTitleId && slideoverContentId) {
        setupInternalLinkNavigation(content, slideoverId, slideoverOverlayId, slideoverCloseId, slideoverTitleId, slideoverContentId);
    } else {
        // Fallback to simple anchor navigation if slide-over not available
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
        // Get the position of the target relative to the scrollable content
        const contentRect = content.getBoundingClientRect();
        const targetRect = targetElement.getBoundingClientRect();
        const currentScroll = content.scrollTop;

        // Calculate the scroll position
        const offset = 20; // Small offset from the top of the container
        const scrollPosition = currentScroll + (targetRect.top - contentRect.top) - offset;

        console.log('[NebaDocument] Scrolling to internal link target:', targetId);

        content.scrollTo({
            top: scrollPosition,
            behavior: 'smooth'
        });

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
    if (!metadata || !metadata.LastUpdatedUtc) {
        return '';
    }

    try {
        // Parse the UTC date
        const utcDate = new Date(metadata.LastUpdatedUtc);

        // Check if date is valid
        if (isNaN(utcDate.getTime())) {
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
            throw new Error(`Failed to load: ${response.status} ${response.statusText}`);
        }

        const data = await response.json();

        // The API should return an object with an 'html' property
        const html = data.html || data.content || data;
        const metadata = data.metadata || {};

        if (!html) {
            throw new Error('No content returned from API');
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
    } catch (error) {
        console.error('[NebaDocument] Error loading slide-over content:', error);
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

    // Scroll to the target element
    const contentRect = content.getBoundingClientRect();
    const targetRect = targetElement.getBoundingClientRect();
    const currentScroll = content.scrollTop;

    const offset = 20; // Small offset from the top of the container
    const scrollPosition = currentScroll + (targetRect.top - contentRect.top) - offset;

    console.log('[NebaDocument] Scrolling to position:', scrollPosition);

    content.scrollTo({
        top: scrollPosition,
        behavior: 'smooth'
    });

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
