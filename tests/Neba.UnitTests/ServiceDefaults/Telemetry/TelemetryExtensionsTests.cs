using System.Diagnostics;
using Neba.ServiceDefaults.Telemetry;

#pragma warning disable S3871, S3376 // Exception type naming conventions - test helper class

namespace Neba.UnitTests.ServiceDefaults.Telemetry;

[Trait("Category", "Unit")]
[Trait("Component", "ServiceDefaults.Telemetry")]
public sealed class TelemetryExtensionsTests
{
    [Fact(DisplayName = "GetErrorType returns fully qualified type name for exception")]
    public void GetErrorType_ReturnsFullyQualifiedTypeName()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        string errorType = exception.GetErrorType();

        // Assert
        errorType.ShouldBe("System.InvalidOperationException");
    }

    [Fact(DisplayName = "GetErrorType returns fully qualified name for custom exception")]
    public void GetErrorType_ForCustomException_ReturnsFullyQualifiedName()
    {
        // Arrange
        var exception = new CustomExceptionWithoutFullName();

        // Act
        string errorType = exception.GetErrorType();

        // Assert
        errorType.ShouldNotBeNullOrEmpty();
        // Custom exceptions have FullName, so the full qualified name is returned
        errorType.ShouldBe(exception.GetType().FullName);
    }

    [Fact(DisplayName = "GetErrorType throws ArgumentNullException when exception is null")]
    public void GetErrorType_WhenExceptionIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        Exception? exception = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => exception!.GetErrorType());
    }

    [Fact(DisplayName = "SetExceptionTags sets error.type tag")]
    public void SetExceptionTags_SetsErrorTypeTag()
    {
        // Arrange
        using var activity = new Activity("test");
        activity.Start();
        var exception = new InvalidOperationException("Test exception");

        // Act
        activity.SetExceptionTags(exception);

        // Assert
        activity.GetTagItem("error.type").ShouldBe("System.InvalidOperationException");
    }

    [Fact(DisplayName = "SetExceptionTags sets exception.message tag")]
    public void SetExceptionTags_SetsExceptionMessageTag()
    {
        // Arrange
        using var activity = new Activity("test");
        activity.Start();
        var exception = new InvalidOperationException("Test error message");

        // Act
        activity.SetExceptionTags(exception);

        // Assert
        activity.GetTagItem("exception.message").ShouldBe("Test error message");
    }

    [Fact(DisplayName = "SetExceptionTags sets exception.stacktrace tag")]
    public void SetExceptionTags_SetsExceptionStackTraceTag()
    {
        // Arrange
        using var activity = new Activity("test");
        activity.Start();
        Exception? exception = null;

        try
        {
            throw new InvalidOperationException("Test exception");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Act
        activity.SetExceptionTags(exception!);

        // Assert
        activity.GetTagItem("exception.stacktrace").ShouldNotBeNull();
        activity.GetTagItem("exception.stacktrace")!.ToString()!.ShouldContain("TelemetryExtensionsTests");
    }

    [Fact(DisplayName = "SetExceptionTags sets activity status to Error")]
    public void SetExceptionTags_SetsActivityStatusToError()
    {
        // Arrange
        using var activity = new Activity("test");
        activity.Start();
        var exception = new InvalidOperationException("Test exception");

        // Act
        activity.SetExceptionTags(exception);

        // Assert
        activity.Status.ShouldBe(ActivityStatusCode.Error);
        activity.StatusDescription.ShouldBe("Test exception");
    }

    [Fact(DisplayName = "SetExceptionTags returns activity for method chaining")]
    public void SetExceptionTags_ReturnsActivityForChaining()
    {
        // Arrange
        using var activity = new Activity("test");
        activity.Start();
        var exception = new InvalidOperationException("Test exception");

        // Act
        Activity? result = activity.SetExceptionTags(exception);

        // Assert
        result.ShouldBe(activity);
    }

    [Fact(DisplayName = "SetExceptionTags returns null when activity is null")]
    public void SetExceptionTags_WhenActivityIsNull_ReturnsNull()
    {
        // Arrange
        Activity? activity = null;
        var exception = new InvalidOperationException("Test exception");

        // Act
        Activity? result = activity.SetExceptionTags(exception);

        // Assert
        result.ShouldBeNull();
    }

    [Fact(DisplayName = "SetExceptionTags throws ArgumentNullException when exception is null")]
    public void SetExceptionTags_WhenExceptionIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        using var activity = new Activity("test");
        activity.Start();
        Exception? exception = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => activity.SetExceptionTags(exception!));
    }

    [Fact(DisplayName = "SetCodeAttributes sets code.function tag")]
    public void SetCodeAttributes_SetsCodeFunctionTag()
    {
        // Arrange
        using var activity = new Activity("test");
        activity.Start();

        // Act
        activity.SetCodeAttributes("MyFunction");

        // Assert
        activity.GetTagItem("code.function").ShouldBe("MyFunction");
    }

    [Fact(DisplayName = "SetCodeAttributes sets code.namespace tag when provided")]
    public void SetCodeAttributes_WhenNamespaceProvided_SetsCodeNamespaceTag()
    {
        // Arrange
        using var activity = new Activity("test");
        activity.Start();

        // Act
        activity.SetCodeAttributes("MyFunction", "MyNamespace");

        // Assert
        activity.GetTagItem("code.function").ShouldBe("MyFunction");
        activity.GetTagItem("code.namespace").ShouldBe("MyNamespace");
    }

    [Fact(DisplayName = "SetCodeAttributes does not set code.namespace when null")]
    public void SetCodeAttributes_WhenNamespaceIsNull_DoesNotSetNamespaceTag()
    {
        // Arrange
        using var activity = new Activity("test");
        activity.Start();

        // Act
        activity.SetCodeAttributes("MyFunction", null);

        // Assert
        activity.GetTagItem("code.function").ShouldBe("MyFunction");
        activity.GetTagItem("code.namespace").ShouldBeNull();
    }

    [Fact(DisplayName = "SetCodeAttributes does not set code.namespace when empty")]
    public void SetCodeAttributes_WhenNamespaceIsEmpty_DoesNotSetNamespaceTag()
    {
        // Arrange
        using var activity = new Activity("test");
        activity.Start();

        // Act
        activity.SetCodeAttributes("MyFunction", string.Empty);

        // Assert
        activity.GetTagItem("code.function").ShouldBe("MyFunction");
        activity.GetTagItem("code.namespace").ShouldBeNull();
    }

    [Fact(DisplayName = "SetCodeAttributes returns activity for method chaining")]
    public void SetCodeAttributes_ReturnsActivityForChaining()
    {
        // Arrange
        using var activity = new Activity("test");
        activity.Start();

        // Act
        Activity? result = activity.SetCodeAttributes("MyFunction", "MyNamespace");

        // Assert
        result.ShouldBe(activity);
    }

    [Fact(DisplayName = "SetCodeAttributes returns null when activity is null")]
    public void SetCodeAttributes_WhenActivityIsNull_ReturnsNull()
    {
        // Arrange
        Activity? activity = null;

        // Act
        Activity? result = activity.SetCodeAttributes("MyFunction", "MyNamespace");

        // Assert
        result.ShouldBeNull();
    }

    [Fact(DisplayName = "SetExceptionTags and SetCodeAttributes can be chained together")]
    public void ChainedCalls_WorkCorrectly()
    {
        // Arrange
        using var activity = new Activity("test");
        activity.Start();
        var exception = new InvalidOperationException("Test exception");

        // Act
        activity
            .SetCodeAttributes("HandleRequest", "Neba.Api.Controllers")
            .SetExceptionTags(exception);

        // Assert
        activity.GetTagItem("code.function").ShouldBe("HandleRequest");
        activity.GetTagItem("code.namespace").ShouldBe("Neba.Api.Controllers");
        activity.GetTagItem("error.type").ShouldBe("System.InvalidOperationException");
        activity.GetTagItem("exception.message").ShouldBe("Test exception");
        activity.Status.ShouldBe(ActivityStatusCode.Error);
    }

    // Helper class for testing exception without FullName
    private sealed class CustomExceptionWithoutFullName : Exception
    {
        public CustomExceptionWithoutFullName() : base("Custom exception")
        {
        }

        public CustomExceptionWithoutFullName(string message) : base(message)
        {
        }

        public CustomExceptionWithoutFullName(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
