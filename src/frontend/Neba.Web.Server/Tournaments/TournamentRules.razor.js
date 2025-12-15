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
        const scrollPosition = content.scrollTop + 50; // Offset for better visual alignment

        let activeHeading = null;
        headings.forEach(heading => {
            const headingTop = heading.offsetTop;
            if (scrollPosition >= headingTop - 100) {
                activeHeading = heading;
            }
        });

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
