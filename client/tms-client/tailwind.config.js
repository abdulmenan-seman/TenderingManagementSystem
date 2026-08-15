/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          bg: '#e8fdfc',       // Soft mint / ice-blue banner background
          navy: '#1d4ed8',     // Deep blue header text ("Welcome to")
          green: '#16a34a',    // Vibrant green brand text ("Mamina Supermarket")
          blue: '#0284c7',     // Sky blue action text ("Shop, save...")
          muted: '#475569',    // Muted slate body text ("Discover fresh...")
        },
        primary: {
          50: '#e8fdfc',
          100: '#ccfbf1',
          500: '#16a34a',
          600: '#15803d',
          900: '#1e3a8a',
        }
      }
    },
  },
  plugins: [],
}