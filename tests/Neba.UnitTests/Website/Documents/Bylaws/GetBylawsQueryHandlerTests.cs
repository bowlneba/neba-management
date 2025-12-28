using Microsoft.Extensions.Logging.Abstractions;
using Neba.Application.Documents;
using Neba.Application.Messaging;
using Neba.Application.Storage;
using Neba.Tests.Documents;
using Neba.Website.Application.Documents.Bylaws;

namespace Neba.UnitTests.Website.Documents.Bylaws;

public sealed class GetBylawsQueryHandlerTests
{
    private static readonly string[] ExpectedDocumentTags = ["website", "website:documents", "website:document:bylaws"];

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

    [Fact(DisplayName = "Returns document from storage when it exists")]
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

    [Fact(DisplayName = "Retrieves document from source and triggers background sync when not in storage")]
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

    [Fact(DisplayName = "Returns document even when background sync fails")]
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

    [Fact(DisplayName = "Query implements ICachedQuery interface")]
    public void Query_ShouldImplementICachedQuery()
    {
        // Arrange & Act
        var query = new GetBylawsQuery();

        // Assert
        query.ShouldBeAssignableTo<ICachedQuery<DocumentDto>>();
    }

    [Fact(DisplayName = "Query cache key follows naming convention")]
    public void Query_CacheKey_ShouldFollowConvention()
    {
        // Arrange
        var query = new GetBylawsQuery();

        // Act
        string key = query.Key;

        // Assert
        key.ShouldBe("website:doc:bylaws:content");
        key.ShouldSatisfyAllConditions(
            k => k.ShouldNotBeNullOrWhiteSpace(),
            k => k.Length.ShouldBeLessThanOrEqualTo(512),
            k => k.Split(':').Length.ShouldBeGreaterThanOrEqualTo(3),
            k => k.Split(':').ShouldAllBe(p => !string.IsNullOrWhiteSpace(p))
        );
        key.Split(':')[0].ShouldBe("website");
        key.Split(':')[1].ShouldBe("doc");
        key.Split(':')[2].ShouldBe("bylaws");
        key.Split(':')[3].ShouldBe("content");
    }

    [Fact(DisplayName = "Query cache expiry is 30 days")]
    public void Query_CacheExpiry_ShouldBe30Days()
    {
        // Arrange
        var query = new GetBylawsQuery();

        // Act
        TimeSpan expiry = query.Expiry;

        // Assert
        expiry.ShouldBe(TimeSpan.FromDays(30));
    }

    [Fact(DisplayName = "Query cache tags include document hierarchy")]
    public void Query_CacheTags_ShouldIncludeDocumentHierarchy()
    {
        // Arrange
        var query = new GetBylawsQuery();

        // Act
        IReadOnlyCollection<string> tags = query.Tags;

        // Assert
        tags.ShouldBeEquivalentTo(ExpectedDocumentTags);
    }
}
