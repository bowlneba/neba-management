import { BREAKPOINTS } from '/js/breakpoints.js';

// Check if viewport is mobile
export function isMobile() {
    return globalThis.matchMedia(`(max-width: ${BREAKPOINTS.MOBILE}px)`).matches;
}
