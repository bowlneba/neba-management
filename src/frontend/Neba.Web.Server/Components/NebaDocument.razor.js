/**
 * NebaDocument - Reusable component for displaying documents with table of contents
 * Provides TOC generation, scroll spy, smooth scrolling, and hash navigation
 */

export function initializeToc(
    contentId,
    tocListId,
    tocMobileListId,
    tocMobileButtonId,
    tocModalId,
    tocModalOverlayId,
    tocModalCloseId,
    headingLevels = 'h1, h2'
) {
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

    if (!tocList) {
        console.error('[NebaDocument] TOC list element not found:', tocListId);
        return false;
    }

    // Extract headings based on the provided levels
    const headings = content.querySelectorAll(headingLevels);

    console.log('[NebaDocument] Found headings:', headings.length);

    if (headings.length === 0) {
        console.warn('[NebaDocument] No headings found in content');
        return false;
    }

    // Build the table of contents
    let tocHtml = '<ul class="toc-list">';

    headings.forEach((heading, index) => {
        const id = heading.getAttribute('id') || `heading-${index}`;
        if (!heading.getAttribute('id')) {
            heading.setAttribute('id', id);
        }

        const level = heading.tagName.toLowerCase();
        const text = heading.textContent;
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
                    const targetId = link.getAttribute('data-target');
                    const targetElement = document.getElementById(targetId);

                    if (targetElement) {
                        // Close the modal first
                        closeModal();

                        // Scroll to the target element on the page (using window scroll on mobile)
                        setTimeout(() => {
                            // Get the position of the element
                            const elementPosition = targetElement.getBoundingClientRect().top;
                            // Account for sticky navbar height (~72px) plus some padding
                            const navbarHeight = 80;
                            const offsetPosition = elementPosition + window.pageYOffset - navbarHeight;

                            window.scrollTo({
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
            const targetId = activeHeading.getAttribute('id');
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
            const targetId = link.getAttribute('data-target');
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
            window.requestAnimationFrame(() => {
                updateActiveLink();
                ticking = false;
            });
            ticking = true;
        }
    });

    // Initial update
    updateActiveLink();

    // Handle internal anchor links in content (same-page section navigation)
    setupInternalLinkNavigation(content);

    console.log('[NebaDocument] TOC initialized successfully');
    return true;
}

/**
 * Sets up smooth scrolling for internal anchor links within the document content
 * @param {HTMLElement} content - The document content container
 */
function setupInternalLinkNavigation(content) {
    // Find all links within the content
    const contentLinks = content.querySelectorAll('a[href^="#"]');

    console.log('[NebaDocument] Setting up internal link navigation for', contentLinks.length, 'links');

    contentLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            const href = link.getAttribute('href');

            // Skip if it's just "#" with no target
            if (!href || href === '#') {
                return;
            }

            e.preventDefault();

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

                // Update URL hash without jumping
                history.pushState(null, '', `#${targetId}`);
            } else {
                console.warn('[NebaDocument] Internal link target not found:', targetId);
            }
        });
    });
}

export function scrollToHash(contentId, tocListId) {
    // Check if there's a hash in the URL
    const hash = window.location.hash;

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
