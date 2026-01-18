/**
 * JavaScript Telemetry Helper
 * Provides utilities for tracking user interactions and performance metrics from JavaScript code.
 * Integrates with .NET telemetry via JSInterop.
 */

let telemetryBridgeInstance = null;

/**
 * Initializes the telemetry bridge with a DotNet reference
 * @param {any} dotNetReference - DotNet object reference for JSInterop
 */
export function initializeTelemetry(dotNetReference) {
    telemetryBridgeInstance = dotNetReference;
}

/**
 * Tracks a user interaction or event
 * @param {string} eventName - Name of the event (e.g., "map.route_calculated")
 * @param {Object} properties - Event properties (e.g., { duration_ms: 150, success: true })
 */
export function trackEvent(eventName, properties = {}) {
    if (telemetryBridgeInstance) {
        try {
            telemetryBridgeInstance.invokeMethodAsync('TrackEvent', eventName, properties);
        } catch (error) {
            console.warn('[Telemetry] Failed to track event:', eventName, error);
        }
    }
}

/**
 * Tracks a JavaScript error
 * @param {string} errorMessage - Error message
 * @param {string} source - Source of the error (e.g., "map.route")
 * @param {string} stackTrace - Optional stack trace
 */
export function trackError(errorMessage, source, stackTrace = null) {
    if (telemetryBridgeInstance) {
        try {
            telemetryBridgeInstance.invokeMethodAsync('TrackError', errorMessage, source, stackTrace);
        } catch (error) {
            console.warn('[Telemetry] Failed to track error:', error);
        }
    }
}

/**
 * Creates a performance timer that automatically tracks duration
 * @param {string} eventName - Name of the event to track
 * @returns {Object} Timer object with stop() method
 */
export function createTimer(eventName) {
    const startTime = performance.now();

    return {
        stop: (success = true, additionalProperties = {}) => {
            const duration = performance.now() - startTime;
            trackEvent(eventName, {
                duration_ms: duration,
                success: success,
                ...additionalProperties
            });
        }
    };
}

/**
 * Wraps an async function with automatic telemetry tracking
 * @param {string} eventName - Name of the event
 * @param {Function} fn - Async function to wrap
 * @returns {Function} Wrapped function with telemetry
 */
export function withTelemetry(eventName, fn) {
    return async function(...args) {
        const timer = createTimer(eventName);
        try {
            const result = await fn.apply(this, args);
            timer.stop(true);
            return result;
        } catch (error) {
            timer.stop(false, { error: error.message });
            trackError(error.message, eventName, error.stack);
            throw error;
        }
    };
}
