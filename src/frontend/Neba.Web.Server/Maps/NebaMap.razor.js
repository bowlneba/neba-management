// NebaMap - Reusable Azure Maps component
// Displays locations on an interactive map with clustering, popups, and focus capabilities
// Note: Assumes 'atlas' is available globally from the Azure Maps SDK CDN

let map = null;
let dataSource = null;
let markers = new Map(); // Track markers by location ID
let currentPopup = null; // Track the currently open popup
let dotNetHelper = null; // Reference to .NET component for callbacks
let boundsChangeTimeout = null; // Timeout for debouncing bounds changes
let lastLocationHash = null; // Hash of last locations to detect changes
let markerClickInProgress = false; // Flag to track if a marker/cluster was just clicked

// Directions mode state
let routeDataSource = null; // Data source for route line
let routeLayer = null; // Layer for route display
let startMarker = null; // Starting location marker
let subscriptionKey = null; // Azure Maps subscription key (stored for routing API)
let authConfig = null; // Full auth config (stored for other modules to access)

/**
 * Waits for the Azure Maps SDK to be loaded
 * @returns {Promise} Promise that resolves when atlas is available
 */
function waitForAtlas() {
    return new Promise((resolve) => {
        if (typeof atlas !== 'undefined') {
            resolve();
            return;
        }

        const checkAtlas = setInterval(() => {
            if (typeof atlas !== 'undefined') {
                clearInterval(checkAtlas);
                resolve();
            }
        }, 100);

        // Timeout after 10 seconds
        setTimeout(() => {
            clearInterval(checkAtlas);
            if (typeof atlas === 'undefined') {
                console.error('[NebaMap] Azure Maps SDK failed to load within timeout');
            }
        }, 10000);
    });
}

/**
 * Initializes the Azure Maps instance with authentication and initial markers
 * @param {Object} authConfig - Authentication configuration { accountId?, subscriptionKey? }
 * @param {Object} mapConfig - Map configuration { containerId, center, zoom, enableClustering }
 * @param {Array} locations - Array of location objects with coordinates and metadata
 * @param {Object} dotNetRef - Reference to .NET component for callbacks
 */
export async function initializeMap(authConfig, mapConfig, locations, dotNetRef) {
    console.log('[NebaMap] Initializing Azure Maps...');
    console.log('[NebaMap] Auth config:', { hasAccountId: !!authConfig.accountId, hasSubscriptionKey: !!authConfig.subscriptionKey });
    console.log('[NebaMap] Locations count:', locations.length);

    // Wait for Azure Maps SDK to be loaded
    await waitForAtlas();
    console.log('[NebaMap] Azure Maps SDK loaded');

    dotNetHelper = dotNetRef;

    // Store auth config for other modules to access
    window.azureMapsAuthConfig = authConfig; // Make available globally

    // Determine authentication method
    let authOptions;
    if (authConfig.subscriptionKey) {
        // Local development or fallback: Use subscription key
        console.log('[NebaMap] Using subscription key authentication');
        subscriptionKey = authConfig.subscriptionKey; // Store for routing API
        window.azureMapsSubscriptionKey = subscriptionKey; // Make available to other modules
        authOptions = {
            authType: 'subscriptionKey',
            subscriptionKey: authConfig.subscriptionKey
        };
    } else if (authConfig.accountId) {
        // Azure deployment: Use Azure AD authentication with managed identity
        console.log('[NebaMap] Using Azure AD authentication with account:', authConfig.accountId);
        authOptions = {
            authType: 'aad',
            clientId: authConfig.accountId,
            getToken: async function(resolve, reject) {
                // Get token from Azure AD
                // In production, this would use the managed identity token
                try {
                    const response = await fetch('/.auth/me');
                    const data = await response.json();
                    if (data && data[0] && data[0].access_token) {
                        resolve(data[0].access_token);
                    } else {
                        reject(new Error('No access token available'));
                    }
                } catch (error) {
                    reject(error);
                }
            }
        };
    } else {
        console.error('[NebaMap] No authentication configured for Azure Maps');
        return;
    }

    // Initialize the map with tile caching optimizations
    try {
        map = new atlas.Map(mapConfig.containerId, {
            authOptions: authOptions,
            center: mapConfig.center,
            zoom: mapConfig.zoom,
            language: 'en-US',
            style: 'road',
            showLogo: false,
            showFeedbackLink: false,
            // Performance optimizations for tile caching
            renderWorldCopies: false, // Don't render multiple copies of the world (saves tiles)
            preserveDrawingBuffer: true, // Better caching and performance
            refreshExpiredTiles: false // Don't auto-refresh expired tiles (saves requests)
        });

        // Wait for map to be ready
        map.events.add('ready', () => {
            console.log('[NebaMap] Map ready');

            // Create a data source for markers
            dataSource = new atlas.source.DataSource(null, {
                cluster: mapConfig.enableClustering,
                clusterRadius: 50,
                clusterMaxZoom: 14,
                // Reduce buffer to prevent "Geometry exceeds allowed extent" errors
                buffer: 64,
                tolerance: 0.375
            });
            map.sources.add(dataSource);

            if (mapConfig.enableClustering) {
                addClusterLayers();
            }

            // Add symbol layer for individual markers
            const symbolLayer = new atlas.layer.SymbolLayer(dataSource, null, {
                iconOptions: {
                    image: 'pin-red',
                    anchor: 'center',
                    allowOverlap: false
                },
                filter: mapConfig.enableClustering ? ['!', ['has', 'point_count']] : null
            });
            map.layers.add(symbolLayer);

            // Add click event for individual markers to show popup
            map.events.add('click', symbolLayer, (e) => {
                if (e.shapes && e.shapes.length > 0) {
                    markerClickInProgress = true;
                    const properties = e.shapes[0].getProperties();
                    showPopup(e.shapes[0].getCoordinates(), properties);
                }
            });

            // Change cursor to pointer on hover
            map.events.add('mouseenter', symbolLayer, () => {
                map.getCanvasContainer().style.cursor = 'pointer';
            });

            map.events.add('mouseleave', symbolLayer, () => {
                map.getCanvasContainer().style.cursor = 'grab';
            });

            // Add initial markers
            updateMarkers(locations);

            // Fit bounds to show all initial markers
            fitBounds();

            // Add event listeners for map bounds changes
            // Note: moveend fires after pan, zoom, and fitBounds animations complete
            map.events.add('moveend', () => {
                notifyBoundsChanged();
            });

            // Close popup when clicking on empty map area
            map.events.add('click', () => {
                // Use setTimeout to let layer click events fire first
                setTimeout(() => {
                    if (!markerClickInProgress && currentPopup) {
                        currentPopup.close();
                        currentPopup = null;
                    }
                    // Reset the flag for next click
                    markerClickInProgress = false;
                }, 0);
            });
        });

    } catch (error) {
        console.error('[NebaMap] Failed to initialize map:', error);
    }
}

/**
 * Adds cluster visualization layers to the map
 */
function addClusterLayers() {
    // Create a bubble layer for clustered points
    const clusterLayer = new atlas.layer.BubbleLayer(dataSource, null, {
        radius: 18,
        color: [
            'step',
            ['get', 'point_count'],
            '#0066b2', // NEBA blue for small clusters
            5, '#004080', // Darker blue for medium clusters
            10, '#002040' // Darkest blue for large clusters
        ],
        strokeWidth: 0,
        filter: ['has', 'point_count']
    });
    map.layers.add(clusterLayer);

    // Create a symbol layer for cluster count
    const clusterCountLayer = new atlas.layer.SymbolLayer(dataSource, null, {
        iconOptions: {
            image: 'none'
        },
        textOptions: {
            textField: ['get', 'point_count_abbreviated'],
            offset: [0, 0],
            color: '#ffffff',
            size: 12
        },
        filter: ['has', 'point_count']
    });
    map.layers.add(clusterCountLayer);

    // Add click event for clusters to zoom in
    map.events.add('click', clusterLayer, (e) => {
        if (e.shapes && e.shapes.length > 0) {
            const shape = e.shapes[0];
            const properties = shape.getProperties ? shape.getProperties() : shape.properties;

            if (properties && properties.cluster) {
                markerClickInProgress = true;
                const clusterId = properties.cluster_id;

                // Use the event position as the cluster center coordinates
                const coordinates = e.position;

                dataSource.getClusterExpansionZoom(clusterId).then((zoom) => {
                    map.setCamera({
                        center: coordinates,
                        zoom: zoom,
                        type: 'ease',
                        duration: 500
                    });
                });
            }
        }
    });

    // Change cursor to pointer on cluster hover
    map.events.add('mouseenter', clusterLayer, () => {
        map.getCanvasContainer().style.cursor = 'pointer';
    });

    map.events.add('mouseleave', clusterLayer, () => {
        map.getCanvasContainer().style.cursor = 'grab';
    });
}

/**
 * Updates the markers on the map with new location data
 * Caches results to avoid unnecessary re-rendering and tile requests
 * @param {Array} locations - Array of location objects
 */
export function updateMarkers(locations) {
    if (!dataSource) {
        console.warn('[NebaMap] Data source not initialized');
        return;
    }

    // Create a hash of location IDs to detect if data has actually changed
    // This prevents unnecessary marker updates that trigger tile reloads
    const locationHash = locations.map(l => l.id).sort().join('|');
    if (locationHash === lastLocationHash) {
        console.log('[NebaMap] Locations unchanged, skipping marker update (cached)');
        return;
    }
    lastLocationHash = locationHash;

    console.log('[NebaMap] Updating markers:', locations.length);

    // Clear existing markers
    dataSource.clear();
    markers.clear();

    // Add new markers with validation
    const features = locations
        .filter(location => {
            // Validate coordinates are valid numbers
            const isValid = typeof location.latitude === 'number' &&
                          typeof location.longitude === 'number' &&
                          !isNaN(location.latitude) &&
                          !isNaN(location.longitude) &&
                          isFinite(location.latitude) &&
                          isFinite(location.longitude);

            if (!isValid) {
                console.warn(`[NebaMap] Skipping location with invalid coordinates:`, location.id, location.latitude, location.longitude);
            }
            return isValid;
        })
        .map(location => {
            const feature = new atlas.data.Feature(
                new atlas.data.Point([location.longitude, location.latitude]),
                {
                    id: location.id,
                    title: location.title,
                    description: location.description,
                    ...location.metadata
                }
            );
            markers.set(location.id, feature);
            return feature;
        });

    console.log(`[NebaMap] Adding ${features.length} valid markers to map`);
    dataSource.add(features);
}

/**
 * Focuses the map on a specific location
 * @param {string} locationId - The ID of the location to focus on
 */
export function focusOnLocation(locationId) {
    if (!map || !markers.has(locationId)) {
        console.warn('[NebaMap] Cannot focus on location:', locationId);
        return;
    }

    console.log('[NebaMap] Focusing on location:', locationId);

    const feature = markers.get(locationId);
    // Access coordinates and properties directly from the feature
    const coordinates = feature.geometry.coordinates;
    const properties = feature.properties;

    // Zoom to the location
    map.setCamera({
        center: coordinates,
        zoom: 15,
        type: 'ease',
        duration: 1000
    });

    // Show popup after zoom completes
    setTimeout(() => {
        showPopup(coordinates, properties);
    }, 1100);
}

/**
 * Fits the map bounds to show all markers
 */
export function fitBounds() {
    if (!map || !dataSource) {
        return;
    }

    const shapes = dataSource.getShapes();
    if (shapes.length > 0) {
        const bounds = atlas.data.BoundingBox.fromData(shapes);
        map.setCamera({
            bounds: bounds,
            padding: 50
        });
    }
}

/**
 * Closes any open popup on the map
 */
export function closePopup() {
    if (currentPopup) {
        currentPopup.close();
        currentPopup = null;
        console.log('[NebaMap] Popup closed');
    }
}

/**
 * Shows an info popup for a location
 * @param {Array} coordinates - [longitude, latitude]
 * @param {Object} properties - Location properties
 */
function showPopup(coordinates, properties) {
    // Close any existing popup before opening a new one
    if (currentPopup) {
        currentPopup.close();
    }

    const content = `
        <div style="padding: 12px; max-width: 280px;">
            <div style="font-weight: 700; font-size: 16px; color: #0066b2; margin-bottom: 8px;">
                ${properties.title}
            </div>
            <div style="font-size: 14px; color: #4b5563;">
                ${properties.description}
            </div>
        </div>
    `;

    currentPopup = new atlas.Popup({
        position: coordinates,
        content: content,
        pixelOffset: [0, -18]
    });

    currentPopup.open(map);
}

/**
 * Notifies the Blazor component about map bounds changes
 * Debounced to prevent excessive updates during continuous pan/zoom
 */
function notifyBoundsChanged() {
    if (!map || !dotNetHelper) {
        return;
    }

    // Clear any pending notification
    if (boundsChangeTimeout) {
        clearTimeout(boundsChangeTimeout);
    }

    // Debounce the notification by 150ms
    boundsChangeTimeout = setTimeout(() => {
        const camera = map.getCamera();
        const bounds = camera.bounds;

        if (bounds) {
            const mapBounds = {
                north: bounds[3],  // maxLatitude
                south: bounds[1],  // minLatitude
                east: bounds[2],   // maxLongitude
                west: bounds[0]    // minLongitude
            };

            console.log('[NebaMap] Bounds changed:', mapBounds);

            dotNetHelper.invokeMethodAsync('NotifyBoundsChanged', mapBounds)
                .catch(error => {
                    console.error('[NebaMap] Error notifying bounds changed:', error);
                });
        }
    }, 150);
}

/**
 * Enters directions preview mode - zooms to selected location and dims other markers
 * @param {string} locationId - The ID of the destination location
 */
export function enterDirectionsPreview(locationId) {
    if (!map || !markers.has(locationId)) {
        console.warn('[NebaMap] Cannot enter directions preview for location:', locationId);
        return;
    }

    console.log('[NebaMap] Entering directions preview mode for:', locationId);

    const feature = markers.get(locationId);
    const coordinates = feature.geometry.coordinates;

    // Close any open popup
    closePopup();

    // Zoom to the selected location
    map.setCamera({
        center: coordinates,
        zoom: 13,
        type: 'ease',
        duration: 1000
    });

    // Dim other markers by reducing their opacity
    // We'll update the symbol layer to make non-selected markers semi-transparent
    const symbolLayers = map.layers.getLayers().filter(l => l instanceof atlas.layer.SymbolLayer);
    symbolLayers.forEach(layer => {
        layer.setOptions({
            iconOptions: {
                opacity: ['case',
                    ['==', ['get', 'id'], locationId],
                    1.0, // Selected marker - full opacity
                    0.3  // Other markers - dimmed
                ]
            }
        });
    });
}

/**
 * Calculates and displays a route from origin to destination using Azure Maps Route API
 * Supports both subscription key (local dev) and Azure AD (production) authentication
 * @param {number[]} origin - Origin coordinates [longitude, latitude]
 * @param {number[]} destination - Destination coordinates [longitude, latitude]
 * @returns {Promise<Object>} Route data with distance, time, and instructions
 */
export async function showRoute(origin, destination) {
    if (!map) {
        console.error('[NebaMap] Cannot show route - map not initialized');
        throw new Error('Map not initialized');
    }

    const authConfig = window.azureMapsAuthConfig;
    if (!authConfig) {
        console.error('[NebaMap] Cannot show route - no auth configuration available');
        throw new Error('Authentication not configured');
    }

    console.log('[NebaMap] Calculating route from', origin, 'to', destination);

    try {
        // Build base URL
        let url = `https://atlas.microsoft.com/route/directions/json?` +
            `api-version=1.0` +
            `&query=${origin[1]},${origin[0]}:${destination[1]},${destination[0]}` +
            `&travelMode=car` +
            `&instructionsType=text` +
            `&guidance=true`;

        let headers = {};

        // Add authentication
        if (authConfig.subscriptionKey) {
            // Local development: Use subscription key
            console.log('[NebaMap] Using subscription key for route');
            url += `&subscription-key=${authConfig.subscriptionKey}`;
        } else if (authConfig.accountId) {
            // Production: Use Azure AD authentication
            console.log('[NebaMap] Using Azure AD for route');
            const token = await getAzureADTokenForRoute();
            if (!token) {
                throw new Error('Failed to get Azure AD token for route calculation');
            }
            headers['Authorization'] = `Bearer ${token}`;
            headers['x-ms-client-id'] = authConfig.accountId;
        }

        const response = await fetch(url, { headers });

        if (!response.ok) {
            throw new Error(`Route API error: ${response.status} ${response.statusText}`);
        }

        const data = await response.json();

        if (!data.routes || data.routes.length === 0) {
            throw new Error('No route found');
        }

        const route = data.routes[0];
        const summary = route.summary;

        // Extract turn-by-turn instructions
        let instructions = [];

        // Try route.guidance.instructions first
        if (route.guidance?.instructions && route.guidance.instructions.length > 0) {
            instructions = route.guidance.instructions.map(instruction => ({
                Text: instruction.message || instruction.instructionType || instruction.text || 'Continue',
                DistanceMeters: instruction.travelDistance || instruction.routeOffsetInMeters || 0
            }));
        }
        // Fallback to route.guidance.instructionGroups
        else if (route.guidance?.instructionGroups && route.guidance.instructionGroups.length > 0) {
            const allInstructions = route.guidance.instructionGroups.flatMap(group => group.instructions || []);
            instructions = allInstructions.map(instruction => ({
                Text: instruction.message || instruction.instructionType || instruction.text || 'Continue',
                DistanceMeters: instruction.travelDistance || instruction.routeOffsetInMeters || 0
            }));
        }

        // Create route data object (exclude routeGeoJson to reduce payload size)
        const routeData = {
            DistanceMeters: summary.lengthInMeters,
            TravelTimeSeconds: summary.travelTimeInSeconds,
            Instructions: instructions,
            RouteGeoJson: null // Don't send full route back - too large for SignalR
        };

        // Draw the route on the map
        drawRoute(route, origin, destination);

        console.log('[NebaMap] Route calculated:', routeData);
        return routeData;
    } catch (error) {
        console.error('[NebaMap] Error calculating route:', error);
        throw error;
    }
}

/**
 * Draws the route line and markers on the map
 * @param {Object} route - Route data from Azure Maps API
 * @param {number[]} origin - Origin coordinates [longitude, latitude]
 * @param {number[]} destination - Destination coordinates [longitude, latitude]
 */
function drawRoute(route, origin, destination) {
    // Create route data source if it doesn't exist
    if (!routeDataSource) {
        routeDataSource = new atlas.source.DataSource();
        map.sources.add(routeDataSource);

        // Create route line layer
        routeLayer = new atlas.layer.LineLayer(routeDataSource, null, {
            strokeColor: '#0066b2',
            strokeWidth: 5,
            strokeOpacity: 0.8
        });
        map.layers.add(routeLayer, 'labels'); // Add below labels
    }

    // Clear existing route
    routeDataSource.clear();

    // Add route line
    const routeLine = route.legs[0].points.map(p => [p.longitude, p.latitude]);
    routeDataSource.add(new atlas.data.Feature(
        new atlas.data.LineString(routeLine)
    ));

    // Add start marker
    const startPoint = new atlas.data.Feature(
        new atlas.data.Point(origin),
        { type: 'start' }
    );
    routeDataSource.add(startPoint);

    // Add start marker layer if not exists
    if (!startMarker) {
        startMarker = new atlas.layer.SymbolLayer(routeDataSource, null, {
            iconOptions: {
                image: 'pin-blue',
                anchor: 'center',
                size: 1.0
            },
            filter: ['==', ['get', 'type'], 'start']
        });
        map.layers.add(startMarker);
    }

    // Fit map bounds to show entire route
    const bounds = atlas.data.BoundingBox.fromData([
        new atlas.data.Point(origin),
        new atlas.data.Point(destination)
    ]);

    map.setCamera({
        bounds: bounds,
        padding: 80,
        type: 'ease',
        duration: 1000
    });
}

/**
 * Exits directions mode and returns to overview
 */
export function exitDirectionsMode() {
    console.log('[NebaMap] Exiting directions mode');

    // Remove route layer and data source
    if (routeLayer) {
        map.layers.remove(routeLayer);
        routeLayer = null;
    }

    if (startMarker) {
        map.layers.remove(startMarker);
        startMarker = null;
    }

    if (routeDataSource) {
        map.sources.remove(routeDataSource);
        routeDataSource = null;
    }

    // Restore marker opacity
    const symbolLayers = map.layers.getLayers().filter(l => l instanceof atlas.layer.SymbolLayer);
    symbolLayers.forEach(layer => {
        layer.setOptions({
            iconOptions: {
                opacity: 1.0
            }
        });
    });

    // Fit bounds to show all markers again
    fitBounds();
}

/**
 * Gets an Azure AD access token for Azure Maps API calls
 * In production with Azure App Service, this uses the built-in authentication
 * @returns {Promise<string|null>} The access token or null if not available
 */
async function getAzureADTokenForRoute() {
    try {
        // In Azure App Service with Easy Auth enabled, we can get the token from /.auth/me
        const response = await fetch('/.auth/me');

        if (!response.ok) {
            console.warn('[NebaMap] Could not fetch auth info from /.auth/me');
            return null;
        }

        const data = await response.json();

        if (data && data[0] && data[0].access_token) {
            return data[0].access_token;
        }

        console.warn('[NebaMap] No access token found in auth response');
        return null;
    } catch (error) {
        console.error('[NebaMap] Error getting Azure AD token:', error);
        return null;
    }
}

/**
 * Cleans up the map instance and removes all event listeners
 * Called when the component is disposed to prevent memory leaks and callback errors
 */
export function dispose() {
    console.log('[NebaMap] Disposing map resources...');

    // Clear any pending bounds change notifications
    if (boundsChangeTimeout) {
        clearTimeout(boundsChangeTimeout);
        boundsChangeTimeout = null;
    }

    // Close any open popup
    if (currentPopup) {
        currentPopup.close();
        currentPopup = null;
    }

    // Clean up directions mode resources
    if (routeLayer) {
        map?.layers.remove(routeLayer);
        routeLayer = null;
    }

    if (startMarker) {
        map?.layers.remove(startMarker);
        startMarker = null;
    }

    if (routeDataSource) {
        map?.sources.remove(routeDataSource);
        routeDataSource = null;
    }

    // Dispose of the map instance
    if (map) {
        // Remove all event listeners (moveend, click, mouseenter, mouseleave)
        // Note: Azure Maps doesn't have a removeAllListeners method, but dispose() handles cleanup
        map.dispose();
        map = null;
    }

    // Clear data structures
    if (dataSource) {
        dataSource = null;
    }
    markers.clear();
    dotNetHelper = null;
    lastLocationHash = null;
    markerClickInProgress = false;
    subscriptionKey = null;

    console.log('[NebaMap] Map disposed successfully');
}
