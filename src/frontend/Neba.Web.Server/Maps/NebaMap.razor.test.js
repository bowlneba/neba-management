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

    test('should export setMapStyle function', async () => {
      const { setMapStyle } = await import('./NebaMap.razor.js');
      expect(typeof setMapStyle).toBe('function');
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

  describe('Map style switching', () => {
    test('should accept valid map styles', () => {
      const validStyles = [
        'road',
        'satellite',
        'satellite_road_labels',
        'grayscale_dark',
        'grayscale_light',
        'night',
        'road_shaded_relief'
      ];

      validStyles.forEach(style => {
        expect(typeof style).toBe('string');
        expect(style.length).toBeGreaterThan(0);
      });
    });

    test('should handle road style', async () => {
      const { setMapStyle } = await import('./NebaMap.razor.js');

      // Should not throw when called without initialized map
      expect(() => setMapStyle('road')).not.toThrow();

      expect(globalThis.console.warn).toHaveBeenCalledWith(
        expect.stringContaining('Cannot change map style - map not initialized')
      );
    });

    test('should handle satellite style', async () => {
      const { setMapStyle } = await import('./NebaMap.razor.js');

      expect(() => setMapStyle('satellite')).not.toThrow();
    });

    test('should handle hybrid (satellite_road_labels) style', async () => {
      const { setMapStyle } = await import('./NebaMap.razor.js');

      expect(() => setMapStyle('satellite_road_labels')).not.toThrow();
    });

    test('should reject invalid map style', async () => {
      // Create a mock map instance
      const mockMap = {
        setStyle: jest.fn(),
        events: {
          add: jest.fn()
        },
        layers: {
          add: jest.fn(),
          getLayers: jest.fn(() => []),
          remove: jest.fn()
        },
        sources: {
          add: jest.fn(),
          remove: jest.fn()
        },
        getCamera: jest.fn(() => ({ bounds: [0, 0, 1, 1] })),
        setCamera: jest.fn(),
        dispose: jest.fn(),
        getCanvasContainer: jest.fn(() => ({ style: {} }))
      };

      globalThis.atlas.Map = jest.fn(() => mockMap);

      const { initializeMap, setMapStyle } = await import('./NebaMap.razor.js');

      // Initialize map first
      const authConfig = { subscriptionKey: 'test-key' };
      const mapConfig = {
        containerId: 'test-map',
        center: [0, 0],
        zoom: 10,
        enableClustering: false,
        style: 'road'
      };

      await initializeMap(authConfig, mapConfig, [], { invokeMethodAsync: jest.fn() });

      // Clear previous warnings
      globalThis.console.warn.mockClear();

      // Try invalid style
      setMapStyle('invalid-style');

      expect(globalThis.console.warn).toHaveBeenCalledWith(
        '[NebaMap] Invalid map style:',
        'invalid-style'
      );

      // setStyle should not have been called with invalid style
      expect(mockMap.setStyle).not.toHaveBeenCalled();
    });

    test('should log style change', async () => {
      // Create a mock map instance
      const mockMap = {
        setStyle: jest.fn(),
        events: {
          add: jest.fn()
        },
        layers: {
          add: jest.fn(),
          getLayers: jest.fn(() => []),
          remove: jest.fn()
        },
        sources: {
          add: jest.fn(),
          remove: jest.fn()
        },
        getCamera: jest.fn(() => ({ bounds: [0, 0, 1, 1] })),
        setCamera: jest.fn(),
        dispose: jest.fn(),
        getCanvasContainer: jest.fn(() => ({ style: {} }))
      };

      globalThis.atlas.Map = jest.fn(() => mockMap);

      const { initializeMap, setMapStyle } = await import('./NebaMap.razor.js');

      // Initialize map first
      const authConfig = { subscriptionKey: 'test-key' };
      const mapConfig = {
        containerId: 'test-map',
        center: [0, 0],
        zoom: 10,
        enableClustering: false,
        style: 'road'
      };

      await initializeMap(authConfig, mapConfig, [], { invokeMethodAsync: jest.fn() });

      // Clear previous logs
      globalThis.console.log.mockClear();

      // Change style
      setMapStyle('satellite');

      expect(globalThis.console.log).toHaveBeenCalledWith(
        '[NebaMap] Changing map style to:',
        'satellite'
      );

      expect(mockMap.setStyle).toHaveBeenCalledWith({
        style: 'satellite'
      });
    });
  });

  describe('updateMarkers', () => {
    test('should warn when data source not initialized', async () => {
      const { updateMarkers } = await import('./NebaMap.razor.js');
      const locations = [
        { id: 1, latitude: 42.3601, longitude: -71.0589, title: 'Test', description: 'Desc' }
      ];

      // Act - try to update markers without initializing map
      updateMarkers(locations);

      // Assert - should warn
      expect(globalThis.console.warn).toHaveBeenCalledWith(
        expect.stringContaining('Data source not initialized')
      );
    });

    test('should filter out invalid coordinates', async () => {
      const { updateMarkers, initializeMap } = await import('./NebaMap.razor.js');

      // Initialize map first
      const mockDataSource = {
        add: jest.fn(),
        clear: jest.fn(),
        getShapes: jest.fn(() => [])
      };

      const mockMap = {
        events: { add: jest.fn((event, callback) => {
          if (event === 'ready') {
            callback(); // Immediately trigger ready event
          }
        }) },
        layers: { add: jest.fn(), getLayers: jest.fn(() => []) },
        sources: { add: jest.fn() },
        setCamera: jest.fn(),
        getCanvasContainer: jest.fn(() => ({ style: {} }))
      };

      globalThis.atlas.source.DataSource = jest.fn(() => mockDataSource);
      globalThis.atlas.Map = jest.fn(() => mockMap);

      const authConfig = { subscriptionKey: 'test-key' };
      const mapConfig = { containerId: 'map', center: [0, 0], zoom: 10, enableClustering: false };

      // Initialize map and wait for ready event
      await initializeMap(authConfig, mapConfig, [], { invokeMethodAsync: jest.fn() });

      const locations = [
        { id: 1, latitude: 42.3601, longitude: -71.0589, title: 'Valid' },
        { id: 2, latitude: NaN, longitude: -71.0589, title: 'Invalid NaN' },
        { id: 3, latitude: 42.3601, longitude: Infinity, title: 'Invalid Infinity' },
        { id: 4, latitude: null, longitude: -71.0589, title: 'Invalid null' }
      ];

      // Act
      updateMarkers(locations);

      // Assert
      expect(globalThis.console.warn).toHaveBeenCalledWith(
        expect.stringContaining('Skipping location with invalid coordinates'),
        expect.anything(),
        expect.anything(),
        expect.anything()
      );
    });
  });

  describe('focusOnLocation', () => {
    test('should warn when location not found', async () => {
      const { focusOnLocation } = await import('./NebaMap.razor.js');

      focusOnLocation('nonexistent');

      expect(globalThis.console.warn).toHaveBeenCalledWith(
        expect.stringContaining('Cannot focus on location'),
        'nonexistent'
      );
    });
  });

  describe('fitBounds', () => {
    test('should handle fitBounds with initialized map', async () => {
      const { fitBounds, initializeMap } = await import('./NebaMap.razor.js');

      // Initialize a map first
      const mockDataSource = {
        add: jest.fn(),
        clear: jest.fn(),
        getShapes: jest.fn(() => [])
      };

      const mockMap = {
        events: { add: jest.fn((event, callback) => {
          if (event === 'ready') {
            callback();
          }
        }) },
        layers: { add: jest.fn(), getLayers: jest.fn(() => []) },
        sources: { add: jest.fn() },
        setCamera: jest.fn(),
        getCanvasContainer: jest.fn(() => ({ style: {} }))
      };

      globalThis.atlas.source.DataSource = jest.fn(() => mockDataSource);
      globalThis.atlas.Map = jest.fn(() => mockMap);

      const authConfig = { subscriptionKey: 'test-key' };
      const mapConfig = { containerId: 'map', center: [0, 0], zoom: 10, enableClustering: false };
      await initializeMap(authConfig, mapConfig, [], { invokeMethodAsync: jest.fn() });

      // Should not throw when map and dataSource are initialized
      expect(() => fitBounds()).not.toThrow();
    });
  });

  describe('closePopup', () => {
    test('should not throw when no popup exists', async () => {
      const { closePopup } = await import('./NebaMap.razor.js');

      expect(() => closePopup()).not.toThrow();
    });
  });

  describe('enterDirectionsPreview', () => {
    test('should warn when location not found', async () => {
      const { enterDirectionsPreview } = await import('./NebaMap.razor.js');

      enterDirectionsPreview('nonexistent');

      expect(globalThis.console.warn).toHaveBeenCalledWith(
        expect.stringContaining('Cannot enter directions preview'),
        'nonexistent'
      );
    });
  });

  describe('exitDirectionsMode', () => {
    test('should handle exiting directions mode', async () => {
      const { exitDirectionsMode, initializeMap } = await import('./NebaMap.razor.js');

      // Initialize a map first
      const mockDataSource = {
        add: jest.fn(),
        clear: jest.fn(),
        getShapes: jest.fn(() => [])
      };

      const mockMap = {
        events: { add: jest.fn((event, callback) => {
          if (event === 'ready') {
            callback(); // Immediately trigger ready event
          }
        }) },
        layers: { add: jest.fn(), getLayers: jest.fn(() => []), remove: jest.fn() },
        sources: { add: jest.fn(), remove: jest.fn() },
        setCamera: jest.fn(),
        dispose: jest.fn(),
        getCanvasContainer: jest.fn(() => ({ style: {} }))
      };

      globalThis.atlas.source.DataSource = jest.fn(() => mockDataSource);
      globalThis.atlas.Map = jest.fn(() => mockMap);

      const authConfig = { subscriptionKey: 'test-key' };
      const mapConfig = { containerId: 'map', center: [0, 0], zoom: 10, enableClustering: false };
      await initializeMap(authConfig, mapConfig, [], { invokeMethodAsync: jest.fn() });

      // Should not throw
      expect(() => exitDirectionsMode()).not.toThrow();
    });
  });

  describe('showRoute', () => {
    test('should throw error when auth config not available', async () => {
      const { showRoute, dispose } = await import('./NebaMap.razor.js');

      // Without initializing map, auth config is not set
      const result = showRoute([0, 0], [1, 1]);
      await expect(result).rejects.toThrow('Authentication not configured');

      // Clean up after test
      dispose();
    });

    test('should throw error when no auth config available', async () => {
      const { showRoute, initializeMap } = await import('./NebaMap.razor.js');

      const mockMap = {
        events: { add: jest.fn() },
        layers: { add: jest.fn(), getLayers: jest.fn(() => []) },
        sources: { add: jest.fn() },
        setCamera: jest.fn(),
        getCanvasContainer: jest.fn(() => ({ style: {} }))
      };

      globalThis.atlas.Map = jest.fn(() => mockMap);

      const authConfig = { subscriptionKey: 'test-key' };
      const mapConfig = { containerId: 'map', center: [0, 0], zoom: 10, enableClustering: false };
      await initializeMap(authConfig, mapConfig, [], { invokeMethodAsync: jest.fn() });

      // Clear auth config
      delete globalThis.window.azureMapsAuthConfig;

      await expect(showRoute([0, 0], [1, 1])).rejects.toThrow('Authentication not configured');
    });

    test('should use subscription key for route API', async () => {
      const { showRoute, initializeMap } = await import('./NebaMap.razor.js');

      const mockDataSource = {
        add: jest.fn(),
        clear: jest.fn(),
        getShapes: jest.fn(() => [])
      };

      const mockMap = {
        events: { add: jest.fn() },
        layers: { add: jest.fn(), getLayers: jest.fn(() => []), remove: jest.fn() },
        sources: { add: jest.fn(), remove: jest.fn() },
        setCamera: jest.fn(),
        getCanvasContainer: jest.fn(() => ({ style: {} }))
      };

      globalThis.atlas.source.DataSource = jest.fn(() => mockDataSource);
      globalThis.atlas.Map = jest.fn(() => mockMap);

      globalThis.fetch = jest.fn().mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({
          routes: [{
            summary: { lengthInMeters: 1000, travelTimeInSeconds: 120 },
            legs: [{ points: [{ latitude: 0, longitude: 0 }, { latitude: 1, longitude: 1 }] }],
            guidance: { instructions: [{ message: 'Turn left', travelDistance: 100 }] }
          }]
        })
      });

      const authConfig = { subscriptionKey: 'test-key' };
      const mapConfig = { containerId: 'map', center: [0, 0], zoom: 10, enableClustering: false };
      await initializeMap(authConfig, mapConfig, [], { invokeMethodAsync: jest.fn() });

      const origin = [-71.0589, 42.3601];
      const destination = [-71.0550, 42.3650];

      await showRoute(origin, destination);

      expect(globalThis.fetch).toHaveBeenCalledWith(
        expect.stringContaining('subscription-key=test-key'),
        expect.any(Object)
      );
    });

    test('should handle route API errors', async () => {
      const { showRoute, initializeMap } = await import('./NebaMap.razor.js');

      const mockMap = {
        events: { add: jest.fn() },
        layers: { add: jest.fn(), getLayers: jest.fn(() => []) },
        sources: { add: jest.fn() },
        setCamera: jest.fn(),
        getCanvasContainer: jest.fn(() => ({ style: {} }))
      };

      globalThis.atlas.Map = jest.fn(() => mockMap);

      globalThis.fetch = jest.fn().mockResolvedValue({
        ok: false,
        status: 500,
        statusText: 'Server Error'
      });

      const authConfig = { subscriptionKey: 'test-key' };
      const mapConfig = { containerId: 'map', center: [0, 0], zoom: 10, enableClustering: false };
      await initializeMap(authConfig, mapConfig, [], { invokeMethodAsync: jest.fn() });

      await expect(showRoute([0, 0], [1, 1])).rejects.toThrow('Route API error');
    });

    test('should handle no routes found', async () => {
      const { showRoute, initializeMap } = await import('./NebaMap.razor.js');

      const mockMap = {
        events: { add: jest.fn() },
        layers: { add: jest.fn(), getLayers: jest.fn(() => []) },
        sources: { add: jest.fn() },
        setCamera: jest.fn(),
        getCanvasContainer: jest.fn(() => ({ style: {} }))
      };

      globalThis.atlas.Map = jest.fn(() => mockMap);

      globalThis.fetch = jest.fn().mockResolvedValue({
        ok: true,
        json: jest.fn().mockResolvedValue({ routes: [] })
      });

      const authConfig = { subscriptionKey: 'test-key' };
      const mapConfig = { containerId: 'map', center: [0, 0], zoom: 10, enableClustering: false };
      await initializeMap(authConfig, mapConfig, [], { invokeMethodAsync: jest.fn() });

      await expect(showRoute([0, 0], [1, 1])).rejects.toThrow('No route found');
    });
  });

  describe('Disposal cleanup', () => {
    test('should have dispose function for cleanup', async () => {
      const { dispose, initializeMap } = await import('./NebaMap.razor.js');

      // Initialize a map first so dispose has something to clean up
      const mockMap = {
        events: { add: jest.fn() },
        layers: { add: jest.fn(), getLayers: jest.fn(() => []), remove: jest.fn() },
        sources: { add: jest.fn(), remove: jest.fn() },
        setCamera: jest.fn(),
        dispose: jest.fn(),
        getCanvasContainer: jest.fn(() => ({ style: {} }))
      };

      globalThis.atlas.Map = jest.fn(() => mockMap);

      const authConfig = { subscriptionKey: 'test-key' };
      const mapConfig = { containerId: 'map', center: [0, 0], zoom: 10, enableClustering: false };
      await initializeMap(authConfig, mapConfig, [], { invokeMethodAsync: jest.fn() });

      // Should not throw when called
      expect(() => dispose()).not.toThrow();
    });

    test('should log disposal message', async () => {
      const { dispose, initializeMap } = await import('./NebaMap.razor.js');

      // Initialize a map first
      const mockMap = {
        events: { add: jest.fn() },
        layers: { add: jest.fn(), getLayers: jest.fn(() => []), remove: jest.fn() },
        sources: { add: jest.fn(), remove: jest.fn() },
        setCamera: jest.fn(),
        dispose: jest.fn(),
        getCanvasContainer: jest.fn(() => ({ style: {} }))
      };

      globalThis.atlas.Map = jest.fn(() => mockMap);

      const authConfig = { subscriptionKey: 'test-key' };
      const mapConfig = { containerId: 'map', center: [0, 0], zoom: 10, enableClustering: false };
      await initializeMap(authConfig, mapConfig, [], { invokeMethodAsync: jest.fn() });

      dispose();

      expect(globalThis.console.log).toHaveBeenCalledWith(
        expect.stringContaining('Map disposed successfully')
      );
    });

    test('should clean up map resources when map is initialized', async () => {
      const { initializeMap, dispose } = await import('./NebaMap.razor.js');

      const mockMap = {
        events: { add: jest.fn() },
        layers: { add: jest.fn(), getLayers: jest.fn(() => []), remove: jest.fn() },
        sources: { add: jest.fn(), remove: jest.fn() },
        setCamera: jest.fn(),
        dispose: jest.fn(),
        getCanvasContainer: jest.fn(() => ({ style: {} }))
      };

      globalThis.atlas.Map = jest.fn(() => mockMap);

      const authConfig = { subscriptionKey: 'test-key' };
      const mapConfig = { containerId: 'map', center: [0, 0], zoom: 10, enableClustering: false };
      await initializeMap(authConfig, mapConfig, [], { invokeMethodAsync: jest.fn() });

      dispose();

      expect(mockMap.dispose).toHaveBeenCalled();
    });
  });
});
