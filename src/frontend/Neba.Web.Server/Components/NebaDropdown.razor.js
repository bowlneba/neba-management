/**
 * NebaDropdown JavaScript Interop Module
 * Provides click-outside detection and focus management for the dropdown component.
 */

let dropdownElement = null;
let dotNetHelper = null;
let clickOutsideHandler = null;

/**
 * Initialize dropdown click-outside detection
 * @param {HTMLElement} element - The dropdown container element
 * @param {any} helper - DotNet object reference for callbacks
 */
export function initializeDropdown(element, helper) {
    // Clean up any existing handlers
    cleanup();

    dropdownElement = element;
    dotNetHelper = helper;

    // Add click-outside handler with a small delay to avoid immediate triggering
    setTimeout(() => {
        clickOutsideHandler = (event) => handleClickOutside(event);
        document.addEventListener('click', clickOutsideHandler, true);
    }, 100);
}

/**
 * Handle click outside the dropdown
 * @param {MouseEvent} event - The click event
 */
function handleClickOutside(event) {
    if (!dropdownElement || !dotNetHelper) {
        return;
    }

    // Check if click is outside the dropdown element
    if (!dropdownElement.contains(event.target)) {
        dotNetHelper.invokeMethodAsync('HandleClickOutsideAsync');
    }
}

/**
 * Clean up event listeners and references
 */
export function cleanup() {
    if (clickOutsideHandler) {
        document.removeEventListener('click', clickOutsideHandler, true);
        clickOutsideHandler = null;
    }

    dropdownElement = null;
    dotNetHelper = null;
}
