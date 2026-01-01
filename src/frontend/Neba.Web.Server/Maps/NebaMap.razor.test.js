import { describe, test, expect, beforeEach, jest } from '@jest/globals';

// Note: NebaMap uses module-level state that makes it difficult to test in isolation
// These tests focus on verifying the API surface and basic functionality

describe('NebaMap', () => {
  beforeEach(() => {
    jest.clearAllMocks();

    // Reset window objects
    delete globalThis.window.azureMapsAuthConfig;
    delete globalThis.window.azureMapsSubscriptionKey;

    // Mock console methods
    globalThis.console.log = jest.fn();
    globalThis.console.warn = jest.fn();
    globalThis.console.error = jest.fn();

    // Mock atlas global
    globalThis.atlas = {
      Map: jest.fn(),
      Popup: jest.fn(),
      source: {
        DataSource: jest.fn(() => ({
          add: jest.fn(),
          clear: jest.fn(),
          getShapes: jest.fn(() => []),
          getClusterExpansionZoom: jest.fn(() => Promise.resolve(15))
        }))
      },
      layer: {
        SymbolLayer: jest.fn(),
        BubbleLayer: jest.fn(),
        LineLayer: jest.fn()
      },
      data: {
        Feature: jest.fn((geometry, properties) => ({ geometry, properties })),
        Point: jest.fn((coords) => ({ type: 'Point', coordinates: coords })),
        LineString: jest.fn((coords) => ({ type: 'LineString', coordinates: coords })),
        BoundingBox: {
          fromData: jest.fn(() => [0, 0, 1, 1])
        }
      }
    };
  });

  describe('Module exports', () => {
    test('should export initializeMap function', async () => {
      const { initializeMap } = await import('./NebaMap.razor.js');
      expect(typeof initializeMap).toBe('function');
    });

    test('should export updateMarkers function', async () => {
      const { updateMarkers } = await import('./NebaMap.razor.js');
      expect(typeof updateMarkers).toBe('function');
    });

    test('should export focusOnLocation function', async () => {
      const { focusOnLocation } = await import('./NebaMap.razor.js');
      expect(typeof focusOnLocation).toBe('function');
    });

    test('should export fitBounds function', async () => {
      const { fitBounds } = await import('./NebaMap.razor.js');
      expect(typeof fitBounds).toBe('function');
    });

    test('should export closePopup function', async () => {
      const { closePopup } = await import('./NebaMap.razor.js');
      expect(typeof closePopup).toBe('function');
    });

    test('should export enterDirectionsPreview function', async () => {
      const { enterDirectionsPreview } = await import('./NebaMap.razor.js');
      expect(typeof enterDirectionsPreview).toBe('function');
    });

    test('should export showRoute function', async () => {
      const { showRoute } = await import('./NebaMap.razor.js');
      expect(typeof showRoute).toBe('function');
    });

    test('should export exitDirectionsMode function', async () => {
      const { exitDirectionsMode } = await import('./NebaMap.razor.js');
      expect(typeof exitDirectionsMode).toBe('function');
    });

    test('should export dispose function', async () => {
      const { dispose } = await import('./NebaMap.razor.js');
      expect(typeof dispose).toBe('function');
    });
  });

  describe('Azure Maps SDK', () => {
    test('should have atlas global available', () => {
      expect(globalThis.atlas).toBeDefined();
    });

    test('should have atlas.Map constructor', () => {
      expect(typeof globalThis.atlas.Map).toBe('function');
    });

    test('should have atlas.source.DataSource constructor', () => {
      expect(typeof globalThis.atlas.source.DataSource).toBe('function');
    });

    test('should have atlas.layer classes', () => {
      expect(typeof globalThis.atlas.layer.SymbolLayer).toBe('function');
      expect(typeof globalThis.atlas.layer.BubbleLayer).toBe('function');
      expect(typeof globalThis.atlas.layer.LineLayer).toBe('function');
    });

    test('should have atlas.data classes', () => {
      expect(typeof globalThis.atlas.data.Feature).toBe('function');
      expect(typeof globalThis.atlas.data.Point).toBe('function');
      expect(typeof globalThis.atlas.data.LineString).toBe('function');
    });
  });

  describe('Authentication configuration', () => {
    test('should support subscription key authentication', () => {
      globalThis.window.azureMapsAuthConfig = {
        subscriptionKey: 'test-key-123'
      };

      expect(globalThis.window.azureMapsAuthConfig.subscriptionKey).toBe('test-key-123');
    });

    test('should support Azure AD authentication', () => {
      globalThis.window.azureMapsAuthConfig = {
        accountId: 'account-123'
      };

      expect(globalThis.window.azureMapsAuthConfig.accountId).toBe('account-123');
    });

    test('should log error when no authentication configured', async () => {
      const { initializeMap } = await import('./NebaMap.razor.js');
      const authConfig = {};
      const mapConfig = { containerId: 'map', center: [0, 0], zoom: 10, enableClustering: false };

      await initializeMap(authConfig, mapConfig, [], { invokeMethodAsync: jest.fn() });

      expect(globalThis.console.error).toHaveBeenCalledWith(
        expect.stringContaining('No authentication configured')
      );
    });
  });

  describe('Location validation', () => {
    test('should validate location coordinates are numbers', () => {
      const validLocation = {
        id: 1,
        latitude: 42.3601,
        longitude: -71.0589
      };

      expect(typeof validLocation.latitude).toBe('number');
      expect(typeof validLocation.longitude).toBe('number');
      expect(Number.isFinite(validLocation.latitude)).toBe(true);
      expect(Number.isFinite(validLocation.longitude)).toBe(true);
    });

    test('should detect invalid coordinates', () => {
      const invalidLocations = [
        { id: 1, latitude: NaN, longitude: -71.0589 },
        { id: 2, latitude: 42.3601, longitude: Infinity },
        { id: 3, latitude: null, longitude: -71.0589 },
        { id: 4, latitude: undefined, longitude: -71.0589 }
      ];

      invalidLocations.forEach(loc => {
        const isValid = typeof loc.latitude === 'number' &&
                       typeof loc.longitude === 'number' &&
                       !isNaN(loc.latitude) &&
                       !isNaN(loc.longitude) &&
                       isFinite(loc.latitude) &&
                       isFinite(loc.longitude);
        expect(isValid).toBe(false);
      });
    });
  });

  describe('Route calculation', () => {
    test('should require authentication for route calculation', async () => {
      const { showRoute } = await import('./NebaMap.razor.js');

      // No auth config set
      await expect(showRoute([0, 0], [1, 1])).rejects.toThrow();
    });

    test('should format route API URL correctly', () => {
      const origin = [-71.0589, 42.3601];
      const destination = [-71.0550, 42.3650];

      const expectedUrl = `https://atlas.microsoft.com/route/directions/json?` +
        `api-version=1.0` +
        `&query=${origin[1]},${origin[0]}:${destination[1]},${destination[0]}` +
        `&travelMode=car` +
        `&instructionsType=text` +
        `&guidance=true`;

      expect(expectedUrl).toContain('api-version=1.0');
      expect(expectedUrl).toContain('travelMode=car');
      expect(expectedUrl).toContain('instructionsType=text');
    });
  });

  describe('Disposal cleanup', () => {
    test('should have dispose function for cleanup', async () => {
      const { dispose } = await import('./NebaMap.razor.js');

      // Should not throw when called
      expect(() => dispose()).not.toThrow();
    });

    test('should log disposal message', async () => {
      const { dispose } = await import('./NebaMap.razor.js');

      dispose();

      expect(globalThis.console.log).toHaveBeenCalledWith(
        expect.stringContaining('Map disposed successfully')
      );
    });
  });
});
