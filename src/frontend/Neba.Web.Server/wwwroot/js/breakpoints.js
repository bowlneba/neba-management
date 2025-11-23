// Centralized breakpoint constants for JavaScript-based responsive behavior
//
// IMPORTANT: These values MUST be kept in sync with CSS variables in neba_theme.css (:root section)
//
// Synchronization checklist when updating breakpoints:
// 1. Update the CSS variables in neba_theme.css (lines 39-47)
// 2. Update these JavaScript constants below
// 3. Update any media queries in CSS files that reference these breakpoints
// 4. Test responsive behavior on mobile, tablet, and desktop
//
// Note: CSS variables use 'px' units, JavaScript constants are unitless numbers

export const BREAKPOINTS = {
    MOBILE: 767,           // --neba-breakpoint-mobile (767px)
    TABLET_MIN: 768,       // --neba-breakpoint-tablet-min (768px)
    TABLET_MAX: 1100,      // --neba-breakpoint-tablet-max (1100px)
    DESKTOP_TIGHT_MIN: 1101,    // --neba-breakpoint-desktop-tight-min (1101px)
    DESKTOP_TIGHT_MAX: 1250,    // --neba-breakpoint-desktop-tight-max (1250px)
    DESKTOP_MEDIUM_MIN: 1251,   // --neba-breakpoint-desktop-medium-min (1251px)
    DESKTOP_MEDIUM_MAX: 1400,   // --neba-breakpoint-desktop-medium-max (1400px)
    DESKTOP_WIDE_MIN: 1401      // --neba-breakpoint-desktop-wide-min (1401px)
};
