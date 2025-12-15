export function initializeToc() {
    console.log('[TOC] Initializing...');
    console.log('[TOC] Looking for element: tournament-rules-content');
    console.log('[TOC] Looking for element: toc-list');

    const content = document.getElementById('tournament-rules-content');
    const tocList = document.getElementById('toc-list');

    console.log('[TOC] Content element:', content);
    console.log('[TOC] TOC list element:', tocList);

    if (!content) {
        console.error('[TOC] Tournament rules content not found');
        console.log('[TOC] All elements with tournament-rules:', document.querySelectorAll('[id*="tournament-rules"]'));
        return false;
    }

    if (!tocList) {
        console.error('[TOC] TOC list not found');
        console.log('[TOC] All elements with toc:', document.querySelectorAll('[id*="toc"]'));
        return false;
    }

    console.log('[TOC] Content HTML length:', content.innerHTML?.length || 0);

    // Extract all h1 and h2 headings
    const headings = content.querySelectorAll('h1, h2');

    console.log('[TOC] Found headings:', headings.length);
    if (headings.length > 0) {
        console.log('[TOC] First heading:', headings[0]);
    }

    if (headings.length === 0) {
        console.warn('[TOC] No headings found in content');
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
        const className = level === 'h1' ? 'toc-item-h1' : 'toc-item-h2';

        tocHtml += `<li class="${className}">
            <a href="#${id}" class="toc-link" data-target="${id}">${text}</a>
        </li>`;
    });

    tocHtml += '</ul>';
    tocList.innerHTML = tocHtml;

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

    console.log('TOC initialized successfully');
    return true;
}

export function scrollToHash() {
    // Check if there's a hash in the URL
    const hash = window.location.hash;

    if (!hash) {
        console.log('[TOC] No hash in URL');
        return;
    }

    // Remove the '#' from the hash to get the ID
    const targetId = hash.substring(1);
    console.log('[TOC] Found hash in URL:', targetId);

    const content = document.getElementById('tournament-rules-content');
    const targetElement = document.getElementById(targetId);

    if (!content) {
        console.error('[TOC] Content container not found');
        return;
    }

    if (!targetElement) {
        console.error('[TOC] Target element not found:', targetId);
        return;
    }

    // Scroll to the target element
    const contentRect = content.getBoundingClientRect();
    const targetRect = targetElement.getBoundingClientRect();
    const currentScroll = content.scrollTop;

    const offset = 20; // Small offset from the top of the container
    const scrollPosition = currentScroll + (targetRect.top - contentRect.top) - offset;

    console.log('[TOC] Scrolling to position:', scrollPosition);

    content.scrollTo({
        top: scrollPosition,
        behavior: 'smooth'
    });

    // Also update the active link in TOC
    const tocList = document.getElementById('toc-list');
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
