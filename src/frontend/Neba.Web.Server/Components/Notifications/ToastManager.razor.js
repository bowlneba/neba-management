import { BREAKPOINTS } from '../../wwwroot/js/breakpoints.js';

// Check if viewport is mobile
export function isMobile() {
    return window.matchMedia(`(max-width: ${BREAKPOINTS.MOBILE}px)`).matches;
}
