using Neba.Web.Server.Telemetry;

namespace Neba.UnitTests.Web.Server.Telemetry;

[Trait("Category", "Unit")]
[Trait("Component", "Web.Server.Telemetry")]
public sealed class ComponentTelemetryTests
{
    [Fact(DisplayName = "RecordComponentError with valid exception and component name completes successfully")]
    public void RecordComponentError_WithValidParameters_CompletesSuccessfully()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        string componentName = "TestComponent";

        // Act & Assert
        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(exception, componentName));
    }

    [Fact(DisplayName = "RecordComponentError with route path completes successfully")]
    public void RecordComponentError_WithRoutePath_CompletesSuccessfully()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        string componentName = "TestComponent";
        string routePath = "/test/page";

        // Act & Assert
        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(exception, componentName, routePath));
    }

    [Fact(DisplayName = "RecordComponentError without route path completes successfully")]
    public void RecordComponentError_WithoutRoutePath_CompletesSuccessfully()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        string componentName = "TestComponent";

        // Act & Assert
        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(exception, componentName, null));
    }

    [Fact(DisplayName = "RecordComponentError with null route path completes successfully")]
    public void RecordComponentError_WithNullRoutePath_CompletesSuccessfully()
    {
        // Arrange
        var exception = new ArgumentException("Test error");
        string componentName = "MyComponent";

        // Act & Assert
        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(exception, componentName, null));
    }

    [Fact(DisplayName = "RecordComponentError with empty route path completes successfully")]
    public void RecordComponentError_WithEmptyRoutePath_CompletesSuccessfully()
    {
        // Arrange
        var exception = new ArgumentException("Test error");
        string componentName = "MyComponent";

        // Act & Assert
        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(exception, componentName, string.Empty));
    }

    [Fact(DisplayName = "RecordComponentError with different exception types completes successfully")]
    public void RecordComponentError_WithDifferentExceptionTypes_CompletesSuccessfully()
    {
        // Arrange
        string componentName = "TestComponent";
        string routePath = "/test";

        // Act & Assert
        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(
            new InvalidOperationException("Invalid op"), componentName, routePath));

        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(
            new ArgumentNullException("param"), componentName, routePath));

        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(
            new NullReferenceException("null ref"), componentName, routePath));

        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(
            new TimeoutException("timeout"), componentName, routePath));
    }

    [Fact(DisplayName = "RecordComponentError with exception containing stack trace completes successfully")]
    public void RecordComponentError_WithStackTrace_CompletesSuccessfully()
    {
        // Arrange
        string componentName = "ErrorComponent";
        string routePath = "/error";
        Exception? capturedException = null;

        try
        {
            throw new InvalidOperationException("Error with stack trace");
        }
        catch (Exception ex)
        {
            capturedException = ex;
        }

        // Act & Assert
        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(capturedException!, componentName, routePath));
    }

    [Fact(DisplayName = "RecordComponentError with nested exception completes successfully")]
    public void RecordComponentError_WithNestedException_CompletesSuccessfully()
    {
        // Arrange
        string componentName = "NestedErrorComponent";
        string routePath = "/nested";

        var innerException = new ArgumentException("Inner error");
        var outerException = new InvalidOperationException("Outer error", innerException);

        // Act & Assert
        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(outerException, componentName, routePath));
    }

    [Fact(DisplayName = "RecordComponentError with long component name completes successfully")]
    public void RecordComponentError_WithLongComponentName_CompletesSuccessfully()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        string componentName = "VeryLongComponentNameThatExceedsNormalLengthForTestingPurposesComponent";
        string routePath = "/test/very/long/route/path/for/testing";

        // Act & Assert
        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(exception, componentName, routePath));
    }

    [Fact(DisplayName = "RecordComponentError with special characters in route path completes successfully")]
    public void RecordComponentError_WithSpecialCharactersInRoutePath_CompletesSuccessfully()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        string componentName = "TestComponent";
        string routePath = "/test/route?param=value&other=123#section";

        // Act & Assert
        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(exception, componentName, routePath));
    }

    [Fact(DisplayName = "RecordComponentError can be called multiple times for same component")]
    public void RecordComponentError_CalledMultipleTimes_CompletesSuccessfully()
    {
        // Arrange
        string componentName = "RepeatedErrorComponent";
        string routePath = "/repeated";

        // Act & Assert
        for (int i = 0; i < 5; i++)
        {
            var exception = new InvalidOperationException($"Error #{i}");
            Should.NotThrow(() => ComponentTelemetry.RecordComponentError(exception, componentName, routePath));
        }
    }

    [Fact(DisplayName = "RecordComponentError can be called for different components")]
    public void RecordComponentError_ForDifferentComponents_CompletesSuccessfully()
    {
        // Arrange & Act & Assert
        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(
            new InvalidOperationException("Error 1"), "Component1", "/route1"));

        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(
            new ArgumentException("Error 2"), "Component2", "/route2"));

        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(
            new NullReferenceException("Error 3"), "Component3", "/route3"));
    }

    [Fact(DisplayName = "RecordComponentError with exception message containing special characters completes successfully")]
    public void RecordComponentError_WithSpecialCharactersInMessage_CompletesSuccessfully()
    {
        // Arrange
        var exception = new InvalidOperationException("Error with special chars: <>&\"'{}[]");
        string componentName = "SpecialCharsComponent";
        string routePath = "/special";

        // Act & Assert
        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(exception, componentName, routePath));
    }

    [Fact(DisplayName = "RecordComponentError with very long error message completes successfully")]
    public void RecordComponentError_WithLongErrorMessage_CompletesSuccessfully()
    {
        // Arrange
        string longMessage = new string('x', 1000);
        var exception = new InvalidOperationException(longMessage);
        string componentName = "LongMessageComponent";
        string routePath = "/long";

        // Act & Assert
        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(exception, componentName, routePath));
    }

    [Fact(DisplayName = "RecordComponentError with aggregate exception completes successfully")]
    public void RecordComponentError_WithAggregateException_CompletesSuccessfully()
    {
        // Arrange
        var innerExceptions = new[]
        {
            new InvalidOperationException("Error 1"),
            new ArgumentException("Error 2"),
            new NullReferenceException("Error 3")
        };
        var aggregateException = new AggregateException("Multiple errors occurred", innerExceptions);
        string componentName = "AggregateErrorComponent";
        string routePath = "/aggregate";

        // Act & Assert
        Should.NotThrow(() => ComponentTelemetry.RecordComponentError(aggregateException, componentName, routePath));
    }
}
