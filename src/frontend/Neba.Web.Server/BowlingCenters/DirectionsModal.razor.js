// DirectionsModal - Handles geolocation and address search for directions feature

/**
 * Gets the user's current location using the browser's Geolocation API
 * @returns {Promise<number[]>} Promise that resolves to [longitude, latitude]
 */
export async function getCurrentLocation() {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            reject(new Error('Geolocation is not supported by your browser'));
            return;
        }

        navigator.geolocation.getCurrentPosition(
            (position) => {
                const longitude = position.coords.longitude;
                const latitude = position.coords.latitude;
                console.log('[DirectionsModal] Got current location:', { latitude, longitude });
                resolve([longitude, latitude]);
            },
            (error) => {
                let errorMessage = 'Unable to retrieve your location';

                switch (error.code) {
                    case error.PERMISSION_DENIED:
                        errorMessage = 'Location access denied. Please enable location services.';
                        break;
                    case error.POSITION_UNAVAILABLE:
                        errorMessage = 'Location information unavailable.';
                        break;
                    case error.TIMEOUT:
                        errorMessage = 'Location request timed out.';
                        break;
                }

                console.error('[DirectionsModal] Geolocation error:', errorMessage, error);
                reject(new Error(errorMessage));
            },
            {
                enableHighAccuracy: false, // Don't require GPS, WiFi/cell tower is fine
                timeout: 10000, // 10 second timeout
                maximumAge: 300000 // Accept cached location up to 5 minutes old
            }
        );
    });
}

/**
 * Searches for address suggestions using Azure Maps Search API
 * Note: This uses the global 'atlas' object from Azure Maps SDK
 * @param {string} query - The address search query
 * @returns {Promise<Array>} Promise that resolves to array of address suggestions
 */
export async function searchAddress(query) {
    // Wait for Azure Maps SDK to be loaded
    if (typeof atlas === 'undefined') {
        console.error('[DirectionsModal] Azure Maps SDK not loaded');
        return [];
    }

    try {
        // Get the search service URL from the map instance
        // For now, we'll use the Search API directly
        // This requires the subscription key to be available

        // Get subscription key from the existing map authentication
        // We'll construct the API request manually
        const subscriptionKey = getAzureMapsSubscriptionKey();

        if (!subscriptionKey) {
            console.error('[DirectionsModal] No Azure Maps subscription key available');
            return [];
        }

        // Use Azure Maps Search API to get address suggestions
        // Limit search to New England region for better results
        const url = `https://atlas.microsoft.com/search/address/json?` +
            `subscription-key=${subscriptionKey}` +
            `&api-version=1.0` +
            `&query=${encodeURIComponent(query)}` +
            `&limit=5` +
            `&countrySet=US` +
            `&view=Auto`;

        const response = await fetch(url);

        if (!response.ok) {
            console.error('[DirectionsModal] Search API error:', response.status, response.statusText);
            return [];
        }

        const data = await response.json();

        if (!data.results || data.results.length === 0) {
            return [];
        }

        // Transform results to our format
        const suggestions = data.results.map(result => ({
            address: result.address.freeformAddress,
            locality: result.address.municipality || result.address.countrySubdivision,
            latitude: result.position.lat,
            longitude: result.position.lon
        }));

        console.log('[DirectionsModal] Found address suggestions:', suggestions.length);
        return suggestions;
    } catch (error) {
        console.error('[DirectionsModal] Error searching address:', error);
        return [];
    }
}

/**
 * Opens a URL in a new browser tab
 * @param {string} url - The URL to open
 */
export function openInNewTab(url) {
    window.open(url, '_blank', 'noopener,noreferrer');
}

/**
 * Helper function to get the Azure Maps subscription key
 * This extracts it from the existing map instance's authentication
 * @returns {string|null} The subscription key or null if not found
 */
function getAzureMapsSubscriptionKey() {
    // Try to get it from the window object where it might be stored
    // This is a bit of a hack, but works for now
    // In production, we might want to pass this more explicitly

    // Check if there's a map instance we can access
    // The NebaMap component should have set this up
    try {
        // Access the atlas authentication from the global scope
        // When the map is initialized, the subscription key is in the auth config
        // We'll need to store this in a global variable during map initialization
        if (window.azureMapsSubscriptionKey) {
            return window.azureMapsSubscriptionKey;
        }

        console.warn('[DirectionsModal] Subscription key not found in window.azureMapsSubscriptionKey');
        return null;
    } catch (error) {
        console.error('[DirectionsModal] Error getting subscription key:', error);
        return null;
    }
}
