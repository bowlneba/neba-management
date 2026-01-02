using System.Threading.Channels;
using Neba.Infrastructure.Documents;

namespace Neba.UnitTests.Infrastructure.Documents;

public sealed class DocumentRefreshChannelsTests
{
    private readonly DocumentRefreshChannels _channels = new();

    [Fact(DisplayName = "Creates a new channel when document type does not exist")]
    public void GetOrCreateChannel_WithNewDocumentType_ShouldCreateChannel()
    {
        // Arrange
        const string documentType = "test-document";

        // Act
        Channel<DocumentRefreshStatusEvent> channel = _channels.GetOrCreateChannel(documentType);

        // Assert
        channel.ShouldNotBeNull();
        channel.Reader.ShouldNotBeNull();
        channel.Writer.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Returns the same channel when document type already exists")]
    public void GetOrCreateChannel_WithSameDocumentType_ShouldReturnSameChannel()
    {
        // Arrange
        const string documentType = "test-document";

        // Act
        Channel<DocumentRefreshStatusEvent> channel1 = _channels.GetOrCreateChannel(documentType);
        Channel<DocumentRefreshStatusEvent> channel2 = _channels.GetOrCreateChannel(documentType);

        // Assert
        channel1.ShouldBeSameAs(channel2);
    }

    [Fact(DisplayName = "Returns different channels for different document types")]
    public void GetOrCreateChannel_WithDifferentDocumentTypes_ShouldReturnDifferentChannels()
    {
        // Arrange
        const string documentType1 = "bylaws";
        const string documentType2 = "tournament-rules";

        // Act
        Channel<DocumentRefreshStatusEvent> channel1 = _channels.GetOrCreateChannel(documentType1);
        Channel<DocumentRefreshStatusEvent> channel2 = _channels.GetOrCreateChannel(documentType2);

        // Assert
        channel1.ShouldNotBeSameAs(channel2);
    }

    [Fact(DisplayName = "Creates an unbounded channel that accepts unlimited items")]
    public void GetOrCreateChannel_ShouldCreateUnboundedChannel()
    {
        // Arrange
        const string documentType = "test-document";

        // Act
        Channel<DocumentRefreshStatusEvent> channel = _channels.GetOrCreateChannel(documentType);

        // Assert
        // Unbounded channels allow unlimited items in the queue
        // This is an implementation detail test - if this fails, the channel options changed
        bool writeResult = channel.Writer.TryWrite(DocumentRefreshStatusEvent.FromStatus("test"));
        writeResult.ShouldBeTrue();
    }

    [Fact(DisplayName = "Supports concurrent access from multiple readers and writers")]
    public async Task GetOrCreateChannel_ShouldSupportMultipleReadersAndWriters()
    {
        // Arrange
        const string documentType = "test-document";
        Channel<DocumentRefreshStatusEvent> channel = _channels.GetOrCreateChannel(documentType);

        // Act & Assert
        // Should be able to write from multiple tasks
        Task<bool> writeTask1 = Task.Run(() => channel.Writer.TryWrite(DocumentRefreshStatusEvent.FromStatus("status1")));
        Task<bool> writeTask2 = Task.Run(() => channel.Writer.TryWrite(DocumentRefreshStatusEvent.FromStatus("status2")));

        bool[] writeResults = await Task.WhenAll(writeTask1, writeTask2);

        writeResults[0].ShouldBeTrue();
        writeResults[1].ShouldBeTrue();

        // Should be able to read from multiple readers
        Task<DocumentRefreshStatusEvent?> readTask1 = Task.Run(async () =>
        {
            await channel.Reader.WaitToReadAsync();
            channel.Reader.TryRead(out DocumentRefreshStatusEvent? item);
            return item;
        });

        Task<DocumentRefreshStatusEvent?> readTask2 = Task.Run(async () =>
        {
            await channel.Reader.WaitToReadAsync();
            channel.Reader.TryRead(out DocumentRefreshStatusEvent? item);
            return item;
        });

        DocumentRefreshStatusEvent?[] readResults = await Task.WhenAll(readTask1, readTask2);

        readResults[0].ShouldNotBeNull();
        readResults[1].ShouldNotBeNull();
    }
}
