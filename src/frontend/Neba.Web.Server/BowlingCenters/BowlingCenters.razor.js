// BowlingCenters - Component-scoped JavaScript module
// Handles list scrolling functionality

/**
 * Scrolls the centers list container to the top
 */
export function scrollToTop() {
    const element = document.querySelector('#centers-scroll-container');
    if (element) {
        element.scrollTop = 0;
    }
}
