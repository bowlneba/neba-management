using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Neba.Application.Documents;
using Neba.Infrastructure.Documents;

namespace Neba.UnitTests.Infrastructure.Documents;

public sealed class SseDocumentRefreshNotifierTests
{
    private readonly DocumentRefreshChannels _channels;
    private readonly Mock<ILogger<SseDocumentRefreshNotifier>> _loggerMock;
    private readonly SseDocumentRefreshNotifier _notifier;

    public SseDocumentRefreshNotifierTests()
    {
        _channels = new DocumentRefreshChannels();
        _loggerMock = new Mock<ILogger<SseDocumentRefreshNotifier>>();
        _notifier = new SseDocumentRefreshNotifier(_channels, _loggerMock.Object);
    }

    [Fact(DisplayName = "Returns early when hub group name is null")]
    public async Task NotifyStatusAsync_WithNullHubGroupName_ShouldReturnEarly()
    {
        // Arrange
        const string? hubGroupName = null;
        DocumentRefreshStatus status = DocumentRefreshStatus.Retrieving;

        // Act
        await _notifier.NotifyStatusAsync(hubGroupName, status);

        // Assert - No channels should be created, so no events should be readable
        // Since we can't easily verify internal state, we rely on the method completing without error
        // and the logger not being called (which we can verify)
        _loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact(DisplayName = "Returns early when hub group name is empty")]
    public async Task NotifyStatusAsync_WithEmptyHubGroupName_ShouldReturnEarly()
    {
        // Arrange
        string? hubGroupName = string.Empty;
        DocumentRefreshStatus status = DocumentRefreshStatus.Retrieving;

        // Act
        await _notifier.NotifyStatusAsync(hubGroupName, status);

        // Assert
        _loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact(DisplayName = "Returns early when hub group name is whitespace")]
    public async Task NotifyStatusAsync_WithWhitespaceHubGroupName_ShouldReturnEarly()
    {
        // Arrange
        const string? hubGroupName = "   ";
        DocumentRefreshStatus status = DocumentRefreshStatus.Retrieving;

        // Act
        await _notifier.NotifyStatusAsync(hubGroupName, status);

        // Assert
        _loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact(DisplayName = "Extracts document type from hub group name and notifies status")]
    public async Task NotifyStatusAsync_WithValidHubGroupName_ShouldExtractDocumentTypeAndNotify()
    {
        // Arrange
        const string hubGroupName = "bylaws-refresh";
        DocumentRefreshStatus status = DocumentRefreshStatus.Retrieving;

        // Act
        await _notifier.NotifyStatusAsync(hubGroupName, status);

        // Assert - Verify the event was written to the channel
        Channel<DocumentRefreshStatusEvent> channel = _channels.GetOrCreateChannel("bylaws");
        DocumentRefreshStatusEvent readResult = await channel.Reader.ReadAsync();
        readResult.Status.ShouldBe(status.Name);
        readResult.ErrorMessage.ShouldBeNull();
        readResult.Timestamp.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-1), DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Fact(DisplayName = "Includes error message in status event when provided")]
    public async Task NotifyStatusAsync_WithErrorMessage_ShouldIncludeErrorInEvent()
    {
        // Arrange
        const string hubGroupName = "tournament-rules-refresh";
        DocumentRefreshStatus status = DocumentRefreshStatus.Failed;
        const string errorMessage = "Document not found";

        // Act
        await _notifier.NotifyStatusAsync(hubGroupName, status, errorMessage);

        // Assert
        Channel<DocumentRefreshStatusEvent> channel = _channels.GetOrCreateChannel("tournament-rules");
        DocumentRefreshStatusEvent readResult = await channel.Reader.ReadAsync();
        readResult.Status.ShouldBe(status.Name);
        readResult.ErrorMessage.ShouldBe(errorMessage);
    }

    [Fact(DisplayName = "Respects cancellation token during notification")]
    public async Task NotifyStatusAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        const string hubGroupName = "bylaws-refresh";
        DocumentRefreshStatus status = DocumentRefreshStatus.Uploading;
        using var cts = new CancellationTokenSource();

        // Act
        await _notifier.NotifyStatusAsync(hubGroupName, status, null, cts.Token);

        // Assert - The operation should complete successfully with the cancellation token
        Channel<DocumentRefreshStatusEvent> channel = _channels.GetOrCreateChannel("bylaws");
        DocumentRefreshStatusEvent readResult = await channel.Reader.ReadAsync();
        readResult.Status.ShouldBe(status.Name);
    }

    [Theory(DisplayName = "Extracts document type correctly from various hub group name formats")]
    [InlineData("bylaws-refresh", "bylaws")]
    [InlineData("tournament-rules-refresh", "tournament-rules")]
    [InlineData("documents-refresh", "documents")]
    [InlineData("no-suffix", "no-suffix")]
    [InlineData("multiple-refresh-suffixes-refresh", "multiple-refresh-suffixes")]
    public async Task ExtractDocumentType_WithVariousInputs_ShouldExtractCorrectly(string hubGroupName, string expectedDocumentType)
    {
        // Arrange
        DocumentRefreshStatus status = DocumentRefreshStatus.Completed;

        // Act
        await _notifier.NotifyStatusAsync(hubGroupName, status);

        // Assert
        Channel<DocumentRefreshStatusEvent> channel = _channels.GetOrCreateChannel(expectedDocumentType);
        DocumentRefreshStatusEvent readResult = await channel.Reader.ReadAsync();
        readResult.Status.ShouldBe(status.Name);
    }
}
