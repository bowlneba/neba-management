using System.Reflection;
using Neba.Infrastructure.Documents;

namespace Neba.UnitTests.Infrastructure.Documents;

[Trait("Category", "Unit")]
[Trait("Component", "Infrastructure.Documents")]
public sealed class DocumentRefreshSseStreamHandlerTests
{
    [Fact(DisplayName = "CreateStreamHandler returns non-null delegate")]
    public void CreateStreamHandler_WithDocumentType_ReturnsNonNullDelegate()
    {
        // Arrange
        const string documentType = "bylaws";

        // Act
        Delegate handler = DocumentRefreshSseStreamHandler.CreateStreamHandler(documentType);

        // Assert
        handler.ShouldNotBeNull();
    }

    [Fact(DisplayName = "CreateStreamHandler returns delegate with correct parameter count")]
    public void CreateStreamHandler_WithDocumentType_ReturnsDelegateWithCorrectParameters()
    {
        // Arrange
        const string documentType = "tournament-rules";

        // Act
        Delegate handler = DocumentRefreshSseStreamHandler.CreateStreamHandler(documentType);

        // Assert
        handler.Method.GetParameters().Length.ShouldBe(3);
    }

    [Theory(DisplayName = "CreateStreamHandler accepts different document types")]
    [InlineData("bylaws")]
    [InlineData("tournament-rules")]
    [InlineData("other-document")]
    [InlineData("")]
    public void CreateStreamHandler_WithVariousDocumentTypes_ReturnsDelegates(string documentType)
    {
        // Act
        Delegate handler = DocumentRefreshSseStreamHandler.CreateStreamHandler(documentType);

        // Assert
        handler.ShouldNotBeNull();
    }

    [Fact(DisplayName = "CreateStreamHandler returns different delegate instances for each call")]
    public void CreateStreamHandler_CalledMultipleTimes_ReturnsDifferentInstances()
    {
        // Arrange
        const string documentType = "bylaws";

        // Act
        Delegate handler1 = DocumentRefreshSseStreamHandler.CreateStreamHandler(documentType);
        Delegate handler2 = DocumentRefreshSseStreamHandler.CreateStreamHandler(documentType);

        // Assert
        handler1.ShouldNotBeSameAs(handler2);
    }

    [Fact(DisplayName = "CreateStreamHandler delegate has expected parameter types")]
    public void CreateStreamHandler_WithDocumentType_HasExpectedParameterTypes()
    {
        // Arrange
        const string documentType = "bylaws";

        // Act
        Delegate handler = DocumentRefreshSseStreamHandler.CreateStreamHandler(documentType);

        // Assert
        ParameterInfo[] parameters = handler.Method.GetParameters();
        parameters[0].ParameterType.ShouldBe(typeof(DocumentRefreshChannels));
        parameters[2].ParameterType.ShouldBe(typeof(CancellationToken));
    }
}
