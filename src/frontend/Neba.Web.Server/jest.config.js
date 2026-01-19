export default {
  testEnvironment: 'jsdom',

  // Module path mappings for imports
  moduleNameMapper: {
    '^/js/(.*)$': '<rootDir>/wwwroot/js/$1',
    '^\\.\\./js/(.*)$': '<rootDir>/wwwroot/js/$1'
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
    '!**/coverage/**',
    '!jest.config.js',
    '!tailwind.config.js'
  ],

  coverageThreshold: {
    global: {
      branches: 80,
      functions: 80,
      lines: 80,
      statements: 80
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
