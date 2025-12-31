// NebaMap - Reusable Azure Maps component
// Displays locations on an interactive map with clustering, popups, and focus capabilities
// Note: Assumes 'atlas' is available globally from the Azure Maps SDK CDN

let map = null;
let dataSource = null;
let markers = new Map(); // Track markers by location ID
let currentConfig = null;
let currentPopup = null; // Track the currently open popup

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
 */
export async function initializeMap(authConfig, mapConfig, locations) {
    console.log('[NebaMap] Initializing Azure Maps...');
    console.log('[NebaMap] Auth config:', { hasAccountId: !!authConfig.accountId, hasSubscriptionKey: !!authConfig.subscriptionKey });
    console.log('[NebaMap] Locations count:', locations.length);

    // Wait for Azure Maps SDK to be loaded
    await waitForAtlas();
    console.log('[NebaMap] Azure Maps SDK loaded');

    currentConfig = mapConfig;

    // Determine authentication method
    let authOptions;
    if (authConfig.subscriptionKey) {
        // Local development or fallback: Use subscription key
        console.log('[NebaMap] Using subscription key authentication');
        authOptions = {
            authType: 'subscriptionKey',
            subscriptionKey: authConfig.subscriptionKey
        };
    } else if (authConfig.accountId) {
        // Azure deployment: Use Azure AD authentication with managed identity
        // For MVP, we'll still use subscription key. Token-based auth is Phase 2.
        console.log('[NebaMap] AccountId provided but subscription key authentication required for MVP');
        console.error('[NebaMap] No subscription key available for authentication');
        return;
    } else {
        console.error('[NebaMap] No authentication configured for Azure Maps');
        return;
    }

    // Initialize the map
    try {
        map = new atlas.Map(mapConfig.containerId, {
            authOptions: authOptions,
            center: mapConfig.center,
            zoom: mapConfig.zoom,
            language: 'en-US',
            style: 'road',
            showLogo: false,
            showFeedbackLink: false
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
 * @param {Array} locations - Array of location objects
 */
export function updateMarkers(locations) {
    if (!dataSource) {
        console.warn('[NebaMap] Data source not initialized');
        return;
    }

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

    // Adjust map bounds to show all markers
    fitBounds();
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
    const coordinates = feature.getCoordinates();
    const properties = feature.getProperties();

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
