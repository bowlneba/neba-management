/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.{razor,html,cshtml}",
    "./Components/**/*.{razor,html}",
    "./Pages/**/*.{razor,html}",
    "./Layout/**/*.{razor,html}",
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
