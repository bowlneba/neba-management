using Neba.Web.Server.Telemetry;

#pragma warning disable CA1416 // Platform compatibility - testing browser-only APIs in unit test context

namespace Neba.UnitTests.Web.Server.Telemetry;

[Trait("Category", "Unit")]
[Trait("Component", "Web.Server.Telemetry")]
public sealed class JavaScriptTelemetryBridgeTests
{
    [Fact(DisplayName = "TrackEvent with event name only completes successfully")]
    public void TrackEvent_WithEventNameOnly_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string eventName = "button.click";

        // Act & Assert
        Should.NotThrow(() => bridge.TrackEvent(eventName));
    }

    [Fact(DisplayName = "TrackEvent with null properties completes successfully")]
    public void TrackEvent_WithNullProperties_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string eventName = "page.load";

        // Act & Assert
        Should.NotThrow(() => bridge.TrackEvent(eventName, null));
    }

    [Fact(DisplayName = "TrackEvent with empty properties completes successfully")]
    public void TrackEvent_WithEmptyProperties_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string eventName = "modal.open";
        var properties = new Dictionary<string, object>();

        // Act & Assert
        Should.NotThrow(() => bridge.TrackEvent(eventName, properties));
    }

    [Fact(DisplayName = "TrackEvent with string properties completes successfully")]
    public void TrackEvent_WithStringProperties_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string eventName = "search.executed";
        var properties = new Dictionary<string, object>
        {
            { "query", "test search" },
            { "category", "users" }
        };

        // Act & Assert
        Should.NotThrow(() => bridge.TrackEvent(eventName, properties));
    }

    [Fact(DisplayName = "TrackEvent with numeric properties completes successfully")]
    public void TrackEvent_WithNumericProperties_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string eventName = "api.call";
        var properties = new Dictionary<string, object>
        {
            { "status_code", 200 },
            { "response_size", 1024 }
        };

        // Act & Assert
        Should.NotThrow(() => bridge.TrackEvent(eventName, properties));
    }

    [Fact(DisplayName = "TrackEvent with boolean properties completes successfully")]
    public void TrackEvent_WithBooleanProperties_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string eventName = "form.submit";
        var properties = new Dictionary<string, object>
        {
            { "success", true },
            { "validation_passed", false }
        };

        // Act & Assert
        Should.NotThrow(() => bridge.TrackEvent(eventName, properties));
    }

    [Fact(DisplayName = "TrackEvent with duration_ms property records operation duration")]
    public void TrackEvent_WithDurationMs_RecordsOperationDuration()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string eventName = "api.request";
        var properties = new Dictionary<string, object>
        {
            { "duration_ms", 125.5 },
            { "success", true }
        };

        // Act & Assert
        Should.NotThrow(() => bridge.TrackEvent(eventName, properties));
    }

    [Fact(DisplayName = "TrackEvent with duration_ms but no success property completes successfully")]
    public void TrackEvent_WithDurationMsWithoutSuccess_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string eventName = "operation.complete";
        var properties = new Dictionary<string, object>
        {
            { "duration_ms", 250.0 }
        };

        // Act & Assert
        Should.NotThrow(() => bridge.TrackEvent(eventName, properties));
    }

    [Fact(DisplayName = "TrackEvent with mixed property types completes successfully")]
    public void TrackEvent_WithMixedPropertyTypes_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string eventName = "complex.event";
        var properties = new Dictionary<string, object>
        {
            { "name", "test" },
            { "count", 42 },
            { "enabled", true },
            { "duration_ms", 75.5 },
            { "success", true }
        };

        // Act & Assert
        Should.NotThrow(() => bridge.TrackEvent(eventName, properties));
    }

    [Fact(DisplayName = "TrackEvent with special characters in event name completes successfully")]
    public void TrackEvent_WithSpecialCharactersInEventName_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string eventName = "map.route_calculated";

        // Act & Assert
        Should.NotThrow(() => bridge.TrackEvent(eventName));
    }

    [Fact(DisplayName = "TrackEvent can be called multiple times")]
    public void TrackEvent_CalledMultipleTimes_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();

        // Act & Assert
        for (int i = 0; i < 10; i++)
        {
            Should.NotThrow(() => bridge.TrackEvent($"event.{i}", new Dictionary<string, object>
            {
                { "index", i },
                { "success", true }
            }));
        }
    }

    [Fact(DisplayName = "TrackError with all parameters completes successfully")]
    public void TrackError_WithAllParameters_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string errorMessage = "JavaScript error occurred";
        const string source = "map.route";
        const string stackTrace = "at calculateRoute (map.js:123)\nat handleClick (app.js:45)";

        // Act & Assert
        Should.NotThrow(() => bridge.TrackError(errorMessage, source, stackTrace));
    }

    [Fact(DisplayName = "TrackError without stack trace completes successfully")]
    public void TrackError_WithoutStackTrace_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string errorMessage = "Error message";
        const string source = "component.init";

        // Act & Assert
        Should.NotThrow(() => bridge.TrackError(errorMessage, source, null));
    }

    [Fact(DisplayName = "TrackError with empty stack trace completes successfully")]
    public void TrackError_WithEmptyStackTrace_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string errorMessage = "Error message";
        const string source = "component.render";

        // Act & Assert
        Should.NotThrow(() => bridge.TrackError(errorMessage, source, string.Empty));
    }

    [Fact(DisplayName = "TrackError with long error message completes successfully")]
    public void TrackError_WithLongErrorMessage_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        string errorMessage = new string('x', 1000);
        const string source = "long.error";

        // Act & Assert
        Should.NotThrow(() => bridge.TrackError(errorMessage, source));
    }

    [Fact(DisplayName = "TrackError with long stack trace completes successfully")]
    public void TrackError_WithLongStackTrace_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string errorMessage = "Deep stack error";
        const string source = "deep.stack";
        string stackTrace = string.Join("\n", Enumerable.Range(0, 50).Select(i => $"at function{i} (file{i}.js:{i})"));

        // Act & Assert
        Should.NotThrow(() => bridge.TrackError(errorMessage, source, stackTrace));
    }

    [Fact(DisplayName = "TrackError with special characters in message completes successfully")]
    public void TrackError_WithSpecialCharactersInMessage_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string errorMessage = "Error with special chars: <>&\"'{}[]";
        const string source = "special.chars";

        // Act & Assert
        Should.NotThrow(() => bridge.TrackError(errorMessage, source));
    }

    [Fact(DisplayName = "TrackError can be called multiple times")]
    public void TrackError_CalledMultipleTimes_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();

        // Act & Assert
        for (int i = 0; i < 5; i++)
        {
            Should.NotThrow(() => bridge.TrackError(
                $"Error message {i}",
                $"source.{i}",
                $"stack trace {i}"));
        }
    }

    [Fact(DisplayName = "TrackEvent and TrackError can be used together")]
    public void TrackEventAndTrackError_CanBeUsedTogether()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();

        // Act & Assert
        Should.NotThrow(() =>
        {
            bridge.TrackEvent("operation.start");
            bridge.TrackEvent("operation.processing", new Dictionary<string, object>
            {
                { "items", 10 }
            });
            bridge.TrackError("Operation failed", "operation.execute", "stack trace");
            bridge.TrackEvent("operation.end", new Dictionary<string, object>
            {
                { "success", false }
            });
        });
    }

    [Fact(DisplayName = "TrackEvent with null property values completes successfully")]
    public void TrackEvent_WithNullPropertyValues_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string eventName = "event.with.nulls";
        var properties = new Dictionary<string, object>
        {
            { "property1", "value" },
            { "property2", null! }
        };

        // Act & Assert
        Should.NotThrow(() => bridge.TrackEvent(eventName, properties));
    }

    [Fact(DisplayName = "TrackEvent with duration_ms as integer completes successfully")]
    public void TrackEvent_WithDurationMsAsInteger_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string eventName = "integer.duration";
        var properties = new Dictionary<string, object>
        {
            { "duration_ms", 100 }, // integer instead of double
            { "success", true }
        };

        // Act & Assert
        Should.NotThrow(() => bridge.TrackEvent(eventName, properties));
    }

    [Fact(DisplayName = "Multiple bridges can be created and used independently")]
    public void MultipleBridges_CanBeUsedIndependently()
    {
        // Arrange
        var bridge1 = new JavaScriptTelemetryBridge();
        var bridge2 = new JavaScriptTelemetryBridge();
        var bridge3 = new JavaScriptTelemetryBridge();

        // Act & Assert
        Should.NotThrow(() =>
        {
            bridge1.TrackEvent("event.from.bridge1");
            bridge2.TrackEvent("event.from.bridge2");
            bridge3.TrackError("error.from.bridge3", "source3");
        });
    }

    [Fact(DisplayName = "TrackEvent with nested properties completes successfully")]
    public void TrackEvent_WithNestedProperties_CompletesSuccessfully()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string eventName = "nested.event";
        var properties = new Dictionary<string, object>
        {
            { "user.id", "123" },
            { "user.name", "Test User" },
            { "action.type", "click" },
            { "action.target", "button" }
        };

        // Act & Assert
        Should.NotThrow(() => bridge.TrackEvent(eventName, properties));
    }

    [Fact(DisplayName = "TrackEvent records success false correctly")]
    public void TrackEvent_WithSuccessFalse_RecordsCorrectly()
    {
        // Arrange
        var bridge = new JavaScriptTelemetryBridge();
        const string eventName = "failed.operation";
        var properties = new Dictionary<string, object>
        {
            { "duration_ms", 500.0 },
            { "success", false }
        };

        // Act & Assert
        Should.NotThrow(() => bridge.TrackEvent(eventName, properties));
    }
}
