using Microsoft.Extensions.Logging.Abstractions;
using Neba.Application.Documents;
using Neba.Application.Storage;
using Neba.Tests.Documents;
using Neba.Website.Application.Documents.Bylaws;

namespace Neba.UnitTests.Website.Documents.Bylaws;

public sealed class GetBylawsQueryHandlerTests
{
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IDocumentsService> _documentsServiceMock;
    private readonly Mock<IBylawsSyncBackgroundJob> _bylawsSyncJobMock;
    private readonly GetBylawsQueryHandler _handler;

    public GetBylawsQueryHandlerTests()
    {
        _storageServiceMock = new Mock<IStorageService>(MockBehavior.Strict);
        _documentsServiceMock = new Mock<IDocumentsService>(MockBehavior.Strict);
        _bylawsSyncJobMock = new Mock<IBylawsSyncBackgroundJob>(MockBehavior.Loose);
        _handler = new GetBylawsQueryHandler(
            _storageServiceMock.Object,
            _documentsServiceMock.Object,
            _bylawsSyncJobMock.Object,
            NullLogger<GetBylawsQueryHandler>.Instance);
    }

    [Fact]
    public async Task HandleAsync_WhenDocumentExistsInStorage_ShouldReturnFromStorage()
    {
        // Arrange
        DocumentDto expectedDocument = DocumentDtoFactory.CreateBylaws();

        _storageServiceMock
            .Setup(ss => ss.ExistsAsync("documents", "bylaws.html", TestContext.Current.CancellationToken))
            .ReturnsAsync(true);

        _storageServiceMock
            .Setup(ss => ss.GetContentWithMetadataAsync("documents", "bylaws.html", TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedDocument);

        var query = new GetBylawsQuery();

        // Act
        DocumentDto result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedDocument);
        _storageServiceMock.Verify(ss => ss.ExistsAsync("documents", "bylaws.html", TestContext.Current.CancellationToken), Times.Once);
        _storageServiceMock.Verify(ss => ss.GetContentWithMetadataAsync("documents", "bylaws.html", TestContext.Current.CancellationToken), Times.Once);
        _documentsServiceMock.Verify(ds => ds.GetDocumentAsHtmlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _bylawsSyncJobMock.Verify(bj => bj.TriggerImmediateSync(), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenDocumentNotInStorage_ShouldRetrieveFromSourceAndTriggerBackgroundSync()
    {
        // Arrange
        const string bylawsHtml = "<h1>Bylaws</h1><p>These are the bylaws...</p>";

        _storageServiceMock
            .Setup(ss => ss.ExistsAsync("documents", "bylaws.html", TestContext.Current.CancellationToken))
            .ReturnsAsync(false);

        _documentsServiceMock
            .Setup(ds => ds.GetDocumentAsHtmlAsync("bylaws", TestContext.Current.CancellationToken))
            .ReturnsAsync(bylawsHtml);

        _bylawsSyncJobMock
            .Setup(bj => bj.TriggerImmediateSync())
            .Returns("mock-job-id");

        var query = new GetBylawsQuery();

        // Act
        DocumentDto result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.Content.ShouldBe(bylawsHtml);
        result.Metadata.ShouldContainKey("LastUpdatedUtc");
        result.Metadata.ShouldContainKey("LastUpdatedBy");
        _storageServiceMock.Verify(ss => ss.ExistsAsync("documents", "bylaws.html", TestContext.Current.CancellationToken), Times.Once);
        _documentsServiceMock.Verify(ds => ds.GetDocumentAsHtmlAsync("bylaws", TestContext.Current.CancellationToken), Times.Once);
        _bylawsSyncJobMock.Verify(bj => bj.TriggerImmediateSync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenBackgroundSyncFails_ShouldStillReturnDocument()
    {
        // Arrange
        const string bylawsHtml = "<h1>Bylaws</h1><p>These are the bylaws...</p>";

        _storageServiceMock
            .Setup(ss => ss.ExistsAsync("documents", "bylaws.html", TestContext.Current.CancellationToken))
            .ReturnsAsync(false);

        _documentsServiceMock
            .Setup(ds => ds.GetDocumentAsHtmlAsync("bylaws", TestContext.Current.CancellationToken))
            .ReturnsAsync(bylawsHtml);

        _bylawsSyncJobMock
            .Setup(bj => bj.TriggerImmediateSync())
            .Throws(new InvalidOperationException("Background job unavailable"));

        var query = new GetBylawsQuery();

        // Act
        DocumentDto result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.Content.ShouldBe(bylawsHtml);
        result.Metadata.ShouldContainKey("LastUpdatedUtc");
        result.Metadata.ShouldContainKey("LastUpdatedBy");
        _documentsServiceMock.Verify(ds => ds.GetDocumentAsHtmlAsync("bylaws", TestContext.Current.CancellationToken), Times.Once);
    }
}
