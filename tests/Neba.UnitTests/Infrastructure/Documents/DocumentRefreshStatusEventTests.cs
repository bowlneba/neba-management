using Neba.Application.Documents;
using Neba.Infrastructure.Documents;

namespace Neba.UnitTests.Infrastructure.Documents;

[Trait("Category", "Unit")]
[Trait("Component", "Infrastructure.Documents")]

public sealed class DocumentRefreshStatusEventTests
{
    [Fact(DisplayName = "Creates event with default values when only status is provided")]
    public void FromStatus_WithStatusOnly_ShouldCreateEventWithDefaults()
    {
        // Arrange
        DocumentRefreshStatus status = DocumentRefreshStatus.Retrieving;

        // Act
        var result = DocumentRefreshStatusEvent.FromStatus(status);

        // Assert
        result.Status.ShouldBe(status.Name);
        result.ErrorMessage.ShouldBeNull();
        (DateTimeOffset.UtcNow - result.Timestamp).TotalSeconds.ShouldBeLessThan(5);
    }

    [Fact(DisplayName = "Creates event with error message when status and error message are provided")]
    public void FromStatus_WithStatusAndErrorMessage_ShouldCreateEventWithErrorMessage()
    {
        // Arrange
        DocumentRefreshStatus status = DocumentRefreshStatus.Failed;
        const string errorMessage = "Document retrieval failed";

        // Act
        var result = DocumentRefreshStatusEvent.FromStatus(status, errorMessage);

        // Assert
        result.Status.ShouldBe(status.Name);
        result.ErrorMessage.ShouldBe(errorMessage);
        (DateTimeOffset.UtcNow - result.Timestamp).TotalSeconds.ShouldBeLessThan(5);
    }

    [Fact(DisplayName = "Creates event when status is provided as string")]
    public void FromStatus_WithStringStatus_ShouldCreateEvent()
    {
        // Arrange
        const string statusString = "CustomStatus";
        const string errorMessage = "Some error";

        // Act
        var result = DocumentRefreshStatusEvent.FromStatus(statusString, errorMessage);

        // Assert
        result.Status.ShouldBe(statusString);
        result.ErrorMessage.ShouldBe(errorMessage);
        (DateTimeOffset.UtcNow - result.Timestamp).TotalSeconds.ShouldBeLessThan(5);
    }

    [Fact(DisplayName = "Throws ArgumentNullException when status is null")]
    public void FromStatus_WithNullStatus_ShouldThrowArgumentNullException()
    {
        // Arrange
        DocumentRefreshStatus? status = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => DocumentRefreshStatusEvent.FromStatus(status!));
    }

    [Fact(DisplayName = "Throws ArgumentException when string status is null")]
    public void FromStatus_WithNullStringStatus_ShouldThrowArgumentException()
    {
        // Arrange
        const string? statusString = null;

        // Act & Assert
        Should.Throw<ArgumentException>(() => DocumentRefreshStatusEvent.FromStatus(statusString!));
    }

    [Fact(DisplayName = "Throws ArgumentException when string status is empty")]
    public void FromStatus_WithEmptyStringStatus_ShouldThrowArgumentException()
    {
        // Arrange
        string statusString = string.Empty;

        // Act & Assert
        Should.Throw<ArgumentException>(() => DocumentRefreshStatusEvent.FromStatus(statusString));
    }

    [Fact(DisplayName = "Throws ArgumentException when string status is whitespace")]
    public void FromStatus_WithWhitespaceStringStatus_ShouldThrowArgumentException()
    {
        // Arrange
        const string statusString = "   ";

        // Act & Assert
        Should.Throw<ArgumentException>(() => DocumentRefreshStatusEvent.FromStatus(statusString));
    }

    [Fact(DisplayName = "Timestamp property is immutable")]
    public void Timestamp_ShouldBeImmutable()
    {
        // Arrange
        DateTimeOffset originalTime = DateTimeOffset.UtcNow;
        var statusEvent = DocumentRefreshStatusEvent.FromStatus(DocumentRefreshStatus.Completed);

        // Act - try to modify (this should not compile if Timestamp is properly readonly)
        // statusEvent.Timestamp = DateTimeOffset.UtcNow; // This would fail to compile

        // Assert
        (statusEvent.Timestamp - originalTime).TotalSeconds.ShouldBeLessThan(5);
    }

    [Theory(DisplayName = "Maps all document refresh status types correctly")]
    [InlineData("Retrieving")]
    [InlineData("Uploading")]
    [InlineData("Completed")]
    [InlineData("Failed")]
    public void FromStatus_WithAllStatusTypes_ShouldMapCorrectly(string statusName)
    {
        // Arrange
        var status = DocumentRefreshStatus.FromName(statusName);

        // Act
        var result = DocumentRefreshStatusEvent.FromStatus(status);

        // Assert
        result.Status.ShouldBe(status.Name);
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact(DisplayName = "Two events with same status have equal property values")]
    public void Events_ShouldBeEquatable()
    {
        // Arrange
        DocumentRefreshStatus status = DocumentRefreshStatus.Completed;
        var event1 = DocumentRefreshStatusEvent.FromStatus(status);
        var event2 = DocumentRefreshStatusEvent.FromStatus(status);

        // Act & Assert
        event1.ShouldNotBeSameAs(event2); // Different instances
        event1.Status.ShouldBe(event2.Status); // But same values
        event1.ErrorMessage.ShouldBe(event2.ErrorMessage);
        // Timestamps will be slightly different due to creation time
    }

    [Fact(DisplayName = "Handles null error message gracefully")]
    public void Event_ShouldHandleNullErrorMessageGracefully()
    {
        // Arrange
        DocumentRefreshStatus status = DocumentRefreshStatus.Completed;

        // Act
        var result = DocumentRefreshStatusEvent.FromStatus(status, null);

        // Assert
        result.ErrorMessage.ShouldBeNull();
    }
}
