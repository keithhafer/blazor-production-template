/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Components/**/*.{razor,html,cshtml}",
    "./Pages/**/*.{razor,html,cshtml}",
    "./Shared/**/*.{razor,html,cshtml}",
    "./wwwroot/**/*.html",
  ],
  // Prefix Tailwind utilities to avoid conflicts with MudBlazor
  prefix: "tw-",
  important: false,
  theme: {
    extend: {
      colors: {
        // Add custom colors that complement MudBlazor's theme
        primary: {
          50: "#f0f9ff",
          100: "#e0f2fe",
          200: "#bae6fd",
          300: "#7dd3fc",
          400: "#38bdf8",
          500: "#0ea5e9",
          600: "#0284c7",
          700: "#0369a1",
          800: "#075985",
          900: "#0c4a6e",
        },
      },
    },
  },
  plugins: [],
  // Ensure Tailwind doesn't interfere with MudBlazor's base styles
  corePlugins: {
    preflight: false,
  },
};
