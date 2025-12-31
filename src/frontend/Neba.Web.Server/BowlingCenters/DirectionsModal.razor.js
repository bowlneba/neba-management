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
 * Supports both subscription key (local dev) and Azure AD (production) authentication
 * @param {string} query - The address search query
 * @returns {Promise<Array>} Promise that resolves to array of address suggestions
 */
export async function searchAddress(query) {
    console.log('[DirectionsModal] Searching for address:', query);

    // Wait for Azure Maps SDK to be loaded
    if (typeof atlas === 'undefined') {
        console.error('[DirectionsModal] Azure Maps SDK not loaded');
        return [];
    }

    try {
        // Get authentication configuration
        const authConfig = window.azureMapsAuthConfig;

        if (!authConfig) {
            console.error('[DirectionsModal] No Azure Maps auth configuration available');
            return [];
        }

        let url = `https://atlas.microsoft.com/search/address/json?` +
            `api-version=1.0` +
            `&query=${encodeURIComponent(query)}` +
            `&limit=5` +
            `&countrySet=US` +
            `&view=Auto`;

        let headers = {};

        // Determine authentication method
        if (authConfig.subscriptionKey) {
            // Local development: Use subscription key
            console.log('[DirectionsModal] Using subscription key authentication');
            url += `&subscription-key=${authConfig.subscriptionKey}`;
        } else if (authConfig.accountId) {
            // Production: Use Azure AD authentication
            console.log('[DirectionsModal] Using Azure AD authentication');
            const token = await getAzureADToken();
            if (!token) {
                console.error('[DirectionsModal] Failed to get Azure AD token');
                return [];
            }
            headers['Authorization'] = `Bearer ${token}`;
            headers['x-ms-client-id'] = authConfig.accountId;
        } else {
            console.error('[DirectionsModal] No valid authentication method available');
            return [];
        }

        console.log('[DirectionsModal] Fetching from Azure Maps Search API...');
        const response = await fetch(url, { headers });

        if (!response.ok) {
            const errorText = await response.text();
            console.error('[DirectionsModal] Search API error:', response.status, response.statusText, errorText);
            return [];
        }

        const data = await response.json();
        console.log('[DirectionsModal] Search API response:', data);

        if (!data.results || data.results.length === 0) {
            console.log('[DirectionsModal] No results found for query:', query);
            return [];
        }

        // Transform results to our format
        // Note: Using PascalCase to match C# AddressSuggestion class for proper deserialization
        const suggestions = data.results.map(result => ({
            Address: result.address.freeformAddress,
            Locality: result.address.municipality || result.address.countrySubdivision,
            Latitude: result.position.lat,
            Longitude: result.position.lon
        }));

        console.log('[DirectionsModal] Found address suggestions:', suggestions.length, suggestions);
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
 * Gets an Azure AD access token for Azure Maps API calls
 * In production with Azure App Service, this uses the built-in authentication
 * @returns {Promise<string|null>} The access token or null if not available
 */
async function getAzureADToken() {
    try {
        // In Azure App Service with Easy Auth enabled, we can get the token from /.auth/me
        const response = await fetch('/.auth/me');

        if (!response.ok) {
            console.warn('[DirectionsModal] Could not fetch auth info from /.auth/me');
            return null;
        }

        const data = await response.json();

        if (data && data[0] && data[0].access_token) {
            return data[0].access_token;
        }

        console.warn('[DirectionsModal] No access token found in auth response');
        return null;
    } catch (error) {
        console.error('[DirectionsModal] Error getting Azure AD token:', error);
        return null;
    }
}
