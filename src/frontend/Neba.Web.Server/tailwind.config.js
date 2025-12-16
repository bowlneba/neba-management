/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.{razor,html,cshtml}",
    "./Components/**/*.{razor,html}",
    "./Pages/**/*.{razor,html}",
    "./Layout/**/*.{razor,html}",
  ],
  darkMode: 'class', // Enable class-based dark mode
  theme: {
    extend: {
      colors: {
        // NEBA brand colors using CSS variables
        'neba-blue': {
          700: 'var(--neba-blue-700)',
          600: 'var(--neba-blue-600)',
          500: 'var(--neba-blue-500)',
          300: 'var(--neba-blue-300)',
          100: 'var(--neba-blue-100)',
          brand: 'var(--neba-blue-brand)',
        },
        'neba-gray': {
          50: 'var(--neba-gray-050)',
          100: 'var(--neba-gray-100)',
          200: 'var(--neba-gray-200)',
          300: 'var(--neba-gray-300)',
          400: 'var(--neba-gray-400)',
          700: 'var(--neba-gray-700)',
          800: 'var(--neba-gray-800)',
        },
        'neba-accent': {
          red: 'var(--neba-accent-red)',
        },
        'neba-success': 'var(--neba-success)',
        'neba-warning': 'var(--neba-warning)',
        'neba-info': 'var(--neba-info)',
        // Semantic theme colors
        'neba-bg': 'var(--neba-bg)',
        'neba-bg-panel': 'var(--neba-bg-panel)',
        'neba-text': 'var(--neba-text)',
        'neba-border': 'var(--neba-border)',
      },
      borderRadius: {
        'neba': 'var(--neba-radius)',
        'neba-lg': 'var(--neba-radius-lg)',
      },
      fontFamily: {
        'neba': 'var(--neba-font-base)',
      },
      transitionDuration: {
        'neba': 'var(--neba-transition)',
      },
      maxWidth: {
        'neba-content': 'var(--neba-content-max-width)',
      },
      screens: {
        // NEBA responsive breakpoints
        'mobile': { 'max': '767px' },
        'tablet': { 'min': '768px', 'max': '1100px' },
        'desktop-tight': { 'min': '1101px', 'max': '1250px' },
        'desktop-medium': { 'min': '1251px', 'max': '1400px' },
        'desktop-wide': { 'min': '1401px' },
      },
    },
  },
  plugins: [],
}
