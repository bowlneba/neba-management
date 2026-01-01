export default {
  testEnvironment: 'jsdom',

  // Module path mappings for imports
  moduleNameMapper: {
    '^/js/(.*)$': '<rootDir>/wwwroot/js/$1'
  },

  // Test file patterns
  testMatch: [
    '**/*.test.js'
  ],

  // Coverage configuration
  collectCoverageFrom: [
    '**/*.js',
    '!**/*.test.js',
    '!**/node_modules/**',
    '!**/bin/**',
    '!**/obj/**',
    '!**/wwwroot/**',
    '!jest.config.js',
    '!tailwind.config.js'
  ],

  coverageThreshold: {
    global: {
      branches: 70,
      functions: 70,
      lines: 70,
      statements: 70
    }
  },

  // Transform configuration - empty object for native ESM
  transform: {},

  // Setup files
  setupFilesAfterEnv: [],

  // Clear mocks between tests
  clearMocks: true,
  resetMocks: true,
  restoreMocks: true,

  // Verbose output
  verbose: true
};
