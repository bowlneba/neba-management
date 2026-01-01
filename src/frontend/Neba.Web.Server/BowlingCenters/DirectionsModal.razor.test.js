import { describe, test, expect, beforeEach, jest } from '@jest/globals';
import { getCurrentLocation, searchAddress, openInNewTab } from './DirectionsModal.razor.js';

describe('DirectionsModal', () => {
  beforeEach(() => {
    jest.clearAllMocks();

    // Mock window.open
    globalThis.window = globalThis.window || {};
    globalThis.window.open = jest.fn();

    // Reset Azure Maps auth config
    delete globalThis.window.azureMapsAuthConfig;
  });

  describe('getCurrentLocation', () => {
    test('should return coordinates when geolocation succeeds', async () => {
      // Arrange
      const mockPosition = {
        coords: {
          longitude: -71.0589,
          latitude: 42.3601
        }
      };

      const mockGetCurrentPosition = jest.fn((success) => {
        success(mockPosition);
      });

      Object.defineProperty(globalThis.navigator, 'geolocation', {
        writable: true,
        configurable: true,
        value: {
          getCurrentPosition: mockGetCurrentPosition
        }
      });

      // Act
      const result = await getCurrentLocation();

      // Assert
      expect(result).toEqual([-71.0589, 42.3601]);
      expect(mockGetCurrentPosition).toHaveBeenCalled();
    });

    test('should reject when geolocation is not supported', async () => {
      // Arrange
      Object.defineProperty(globalThis.navigator, 'geolocation', {
        writable: true,
        configurable: true,
        value: undefined
      });

      // Act & Assert
      await expect(getCurrentLocation()).rejects.toThrow('Geolocation is not supported by your browser');
    });

    test('should reject with permission denied error', async () => {
      // Arrange
      const mockError = {
        code: 1, // PERMISSION_DENIED
        PERMISSION_DENIED: 1,
        POSITION_UNAVAILABLE: 2,
        TIMEOUT: 3
      };

      const mockGetCurrentPosition = jest.fn((success, error) => {
        error(mockError);
      });

      Object.defineProperty(globalThis.navigator, 'geolocation', {
        writable: true,
        configurable: true,
        value: {
          getCurrentPosition: mockGetCurrentPosition
        }
      });

      // Act & Assert
      await expect(getCurrentLocation()).rejects.toThrow('Location access denied');
    });

    test('should reject with position unavailable error', async () => {
      // Arrange
      const mockError = {
        code: 2, // POSITION_UNAVAILABLE
        PERMISSION_DENIED: 1,
        POSITION_UNAVAILABLE: 2,
        TIMEOUT: 3
      };

      const mockGetCurrentPosition = jest.fn((success, error) => {
        error(mockError);
      });

      Object.defineProperty(globalThis.navigator, 'geolocation', {
        writable: true,
        configurable: true,
        value: {
          getCurrentPosition: mockGetCurrentPosition
        }
      });

      // Act & Assert
      await expect(getCurrentLocation()).rejects.toThrow('Location information unavailable');
    });

    test('should reject with timeout error', async () => {
      // Arrange
      const mockError = {
        code: 3, // TIMEOUT
        PERMISSION_DENIED: 1,
        POSITION_UNAVAILABLE: 2,
        TIMEOUT: 3
      };

      const mockGetCurrentPosition = jest.fn((success, error) => {
        error(mockError);
      });

      Object.defineProperty(globalThis.navigator, 'geolocation', {
        writable: true,
        configurable: true,
        value: {
          getCurrentPosition: mockGetCurrentPosition
        }
      });

      // Act & Assert
      await expect(getCurrentLocation()).rejects.toThrow('Location request timed out');
    });

    test('should use correct geolocation options', async () => {
      // Arrange
      const mockPosition = {
        coords: { longitude: -71.0589, latitude: 42.3601 }
      };

      const mockGetCurrentPosition = jest.fn((success) => {
        success(mockPosition);
      });

      Object.defineProperty(globalThis.navigator, 'geolocation', {
        writable: true,
        configurable: true,
        value: {
          getCurrentPosition: mockGetCurrentPosition
        }
      });

      // Act
      await getCurrentLocation();

      // Assert
      const options = mockGetCurrentPosition.mock.calls[0][2];
      expect(options.enableHighAccuracy).toBe(false);
      expect(options.timeout).toBe(10000);
      expect(options.maximumAge).toBe(300000);
    });
  });

  describe('searchAddress', () => {
    beforeEach(() => {
      // Mock atlas
      globalThis.atlas = {};
      globalThis.fetch = jest.fn();
    });

    test('should return empty array when atlas is not loaded', async () => {
      // Arrange
      delete globalThis.atlas;

      // Act
      const result = await searchAddress('Boston, MA');

      // Assert
      expect(result).toEqual([]);
    });

    test('should return empty array when no auth config available', async () => {
      // Act
      const result = await searchAddress('Boston, MA');

      // Assert
      expect(result).toEqual([]);
    });

    test('should search with subscription key authentication', async () => {
      // Arrange
      globalThis.window.azureMapsAuthConfig = {
        subscriptionKey: 'test-key-123'
      };

      const mockResponse = {
        ok: true,
        json: jest.fn().mockResolvedValue({
          results: [
            {
              address: {
                freeformAddress: '123 Main St, Boston, MA',
                municipality: 'Boston',
                countrySubdivision: 'MA'
              },
              position: { lat: 42.3601, lon: -71.0589 }
            }
          ]
        })
      };

      globalThis.fetch = jest.fn().mockResolvedValue(mockResponse);

      // Act
      const result = await searchAddress('Boston, MA');

      // Assert
      expect(result).toHaveLength(1);
      expect(result[0]).toEqual({
        Address: '123 Main St, Boston, MA',
        Locality: 'Boston',
        Latitude: 42.3601,
        Longitude: -71.0589
      });

      const fetchCall = globalThis.fetch.mock.calls[0][0];
      expect(fetchCall).toContain('subscription-key=test-key-123');
      expect(fetchCall).toContain(encodeURIComponent('Boston, MA'));
    });

    test('should return empty array when API returns error', async () => {
      // Arrange
      globalThis.window.azureMapsAuthConfig = {
        subscriptionKey: 'test-key-123'
      };

      const mockResponse = {
        ok: false,
        status: 401,
        statusText: 'Unauthorized',
        text: jest.fn().mockResolvedValue('Unauthorized')
      };

      globalThis.fetch = jest.fn().mockResolvedValue(mockResponse);

      // Act
      const result = await searchAddress('Boston, MA');

      // Assert
      expect(result).toEqual([]);
    });

    test('should return empty array when no results found', async () => {
      // Arrange
      globalThis.window.azureMapsAuthConfig = {
        subscriptionKey: 'test-key-123'
      };

      const mockResponse = {
        ok: true,
        json: jest.fn().mockResolvedValue({ results: [] })
      };

      globalThis.fetch = jest.fn().mockResolvedValue(mockResponse);

      // Act
      const result = await searchAddress('NonexistentPlace12345');

      // Assert
      expect(result).toEqual([]);
    });

    test('should handle fetch exception gracefully', async () => {
      // Arrange
      globalThis.window.azureMapsAuthConfig = {
        subscriptionKey: 'test-key-123'
      };

      globalThis.fetch = jest.fn().mockRejectedValue(new Error('Network error'));

      // Act
      const result = await searchAddress('Boston, MA');

      // Assert
      expect(result).toEqual([]);
    });

    test('should limit results to 5', async () => {
      // Arrange
      globalThis.window.azureMapsAuthConfig = {
        subscriptionKey: 'test-key-123'
      };

      globalThis.fetch = jest.fn().mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({ results: [] })
      });

      // Act
      await searchAddress('Boston');

      // Assert
      const fetchCall = globalThis.fetch.mock.calls[0][0];
      expect(fetchCall).toContain('limit=5');
    });

    test('should restrict search to US', async () => {
      // Arrange
      globalThis.window.azureMapsAuthConfig = {
        subscriptionKey: 'test-key-123'
      };

      globalThis.fetch = jest.fn().mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({ results: [] })
      });

      // Act
      await searchAddress('Boston');

      // Assert
      const fetchCall = globalThis.fetch.mock.calls[0][0];
      expect(fetchCall).toContain('countrySet=US');
    });
  });

  describe('openInNewTab', () => {
    test('should open URL in new tab with correct parameters', () => {
      // Arrange
      const url = 'https://maps.google.com/directions';

      // Act
      openInNewTab(url);

      // Assert
      expect(globalThis.window.open).toHaveBeenCalledWith(url, '_blank', 'noopener,noreferrer');
    });

    test('should handle different URL formats', () => {
      // Arrange
      const urls = [
        'https://example.com',
        'http://localhost:3000',
        '/relative/path',
        'mailto:test@example.com'
      ];

      // Act
      urls.forEach(url => {
        openInNewTab(url);
      });

      // Assert
      expect(globalThis.window.open).toHaveBeenCalledTimes(4);
      urls.forEach(url => {
        expect(globalThis.window.open).toHaveBeenCalledWith(url, '_blank', 'noopener,noreferrer');
      });
    });
  });
});
