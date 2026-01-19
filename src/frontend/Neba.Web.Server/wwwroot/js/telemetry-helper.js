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
            return duration;
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

/**
 * Tracks resource loading performance using the Resource Timing API
 * @param {string} resourceType - Type of resource (script, stylesheet, image, etc.)
 */
export function trackResourcePerformance(resourceType = null) {
    if (!globalThis.performance?.getEntriesByType) {
        console.warn('[Telemetry] Performance API not available');
        return;
    }

    const resources = performance.getEntriesByType('resource');

    resources.forEach(resource => {
        const url = new URL(resource.name);
        const resourceName = url.pathname.split('/').pop();

        // Filter by resource type if specified
        if (resourceType && resource.initiatorType !== resourceType) {
            return;
        }

        // Calculate timing metrics
        const metrics = {
            resource_name: resourceName,
            resource_type: resource.initiatorType,
            duration_ms: resource.duration,
            transfer_size: resource.transferSize || 0,
            dns_time: resource.domainLookupEnd - resource.domainLookupStart,
            tcp_time: resource.connectEnd - resource.connectStart,
            ttfb: resource.responseStart - resource.requestStart,
            download_time: resource.responseEnd - resource.responseStart
        };

        trackEvent('resource.loaded', metrics);
    });

    // Clear resource timing buffer to avoid re-reporting
    if (performance.clearResourceTimings) {
        performance.clearResourceTimings();
    }
}

/**
 * Tracks page navigation performance using Navigation Timing API
 */
export function trackNavigationPerformance() {
    if (!globalThis.performance?.getEntriesByType) {
        console.warn('[Telemetry] Navigation Timing API not available');
        return;
    }

    // Wait for page to fully load
    if (document.readyState !== 'complete') {
        window.addEventListener('load', () => {
            setTimeout(trackNavigationPerformance, 0);
        });
        return;
    }

    const navigationEntries = performance.getEntriesByType('navigation')[0];
    if (!navigationEntries) {
        console.warn('[Telemetry] Navigation timing data not available');
        return;
    }

    const metrics = {
        navigation_type: navigationEntries.type,
        redirect_count: navigationEntries.redirectCount,
        dns_time: navigationEntries.domainLookupEnd - navigationEntries.domainLookupStart,
        tcp_time: navigationEntries.connectEnd - navigationEntries.connectStart,
        request_time: navigationEntries.responseStart - navigationEntries.requestStart,
        response_time: navigationEntries.responseEnd - navigationEntries.responseStart,
        dom_processing_time: navigationEntries.domComplete - navigationEntries.domLoading,
        dom_interactive_time: navigationEntries.domInteractive - navigationEntries.domLoading,
        dom_content_loaded_time: navigationEntries.domContentLoadedEventEnd - navigationEntries.domContentLoadedEventStart,
        load_event_time: navigationEntries.loadEventEnd - navigationEntries.loadEventStart,
        total_load_time: navigationEntries.loadEventEnd - navigationEntries.navigationStart
    };

    trackEvent('page.performance', metrics);
}

/**
 * Tracks Core Web Vitals (LCP, FID, CLS) using the web-vitals library pattern
 * Note: This is a simplified version. Consider using the actual web-vitals library for production.
 */
export function trackWebVitals() {
    // Largest Contentful Paint (LCP)
    if ('PerformanceObserver' in globalThis) {
        try {
            const lcpObserver = new PerformanceObserver((list) => {
                const entries = list.getEntries();
                const lastEntry = entries.at(-1);
                trackEvent('web_vitals.lcp', {
                    value: lastEntry.renderTime || lastEntry.loadTime,
                    element: lastEntry.element?.tagName || 'unknown'
                });
            });
            lcpObserver.observe({ entryTypes: ['largest-contentful-paint'] });

            // First Input Delay (FID)
            const fidObserver = new PerformanceObserver((list) => {
                const entries = list.getEntries();
                entries.forEach(entry => {
                    trackEvent('web_vitals.fid', {
                        value: entry.processingStart - entry.startTime,
                        event_type: entry.name
                    });
                });
            });
            fidObserver.observe({ entryTypes: ['first-input'] });

            // Cumulative Layout Shift (CLS)
            let clsValue = 0;
            const clsObserver = new PerformanceObserver((list) => {
                for (const entry of list.getEntries()) {
                    if (!entry.hadRecentInput) {
                        clsValue += entry.value;
                    }
                }
            });
            clsObserver.observe({ entryTypes: ['layout-shift'] });

            // Report CLS on page unload
            globalThis.addEventListener('visibilitychange', () => {
                if (document.visibilityState === 'hidden') {
                    trackEvent('web_vitals.cls', { value: clsValue });
                }
            });
        } catch (error) {
            console.warn('[Telemetry] Failed to track Web Vitals:', error);
        }
    }
}

/**
 * Initialize all performance tracking
 */
export function initializePerformanceTracking() {
    trackNavigationPerformance();
    trackWebVitals();

    // Track resources after a short delay to capture initial resources
    setTimeout(() => trackResourcePerformance(), 1000);
}

// Expose telemetry functions globally for use in non-module contexts (e.g., E2E tests)
if (typeof globalThis !== 'undefined') {
    globalThis.telemetry = {
        initializeTelemetry,
        trackEvent,
        trackError,
        createTimer,
        withTelemetry,
        trackResourcePerformance,
        trackNavigationPerformance,
        trackWebVitals,
        initializePerformanceTracking
    };
}
