using System.Threading.Channels;
using Neba.Infrastructure.Documents;

namespace Neba.UnitTests.Infrastructure.Documents;

public sealed class DocumentRefreshChannelsTests
{
    private readonly DocumentRefreshChannels _channels = new();

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
    public async Task GetOrCreateChannel_ShouldSupportMultipleReadersAndWriters()
    {
        // Arrange
        const string documentType = "test-document";
        Channel<DocumentRefreshStatusEvent> channel = _channels.GetOrCreateChannel(documentType);

        // Act & Assert
        // Should be able to write from multiple tasks
        var writeTask1 = Task.Run(() => channel.Writer.TryWrite(DocumentRefreshStatusEvent.FromStatus("status1")));
        var writeTask2 = Task.Run(() => channel.Writer.TryWrite(DocumentRefreshStatusEvent.FromStatus("status2")));

        var writeResults = await Task.WhenAll(writeTask1, writeTask2);

        writeResults[0].ShouldBeTrue();
        writeResults[1].ShouldBeTrue();

        // Should be able to read from multiple readers
        var readTask1 = Task.Run(async () =>
        {
            await channel.Reader.WaitToReadAsync();
            channel.Reader.TryRead(out var item);
            return item;
        });

        var readTask2 = Task.Run(async () =>
        {
            await channel.Reader.WaitToReadAsync();
            channel.Reader.TryRead(out var item);
            return item;
        });

        var readResults = await Task.WhenAll(readTask1, readTask2);

        readResults[0].ShouldNotBeNull();
        readResults[1].ShouldNotBeNull();
    }
}
