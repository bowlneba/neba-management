// Modal JavaScript interop module for NebaModal component
// Provides safe methods for controlling body overflow state

/**
 * Sets the body overflow style to prevent/allow scrolling.
 * @param {boolean} hidden - True to hide overflow (prevent scrolling), false to restore scrolling.
 */
export function setBodyOverflow(hidden) {
    document.body.style.overflow = hidden ? 'hidden' : '';
}
