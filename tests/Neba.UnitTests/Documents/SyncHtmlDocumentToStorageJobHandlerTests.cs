using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging.Abstractions;
using Neba.Application.Documents;
using Neba.Application.Storage;

namespace Neba.UnitTests.Documents;

public sealed class SyncHtmlDocumentToStorageJobHandlerTests
{
    private readonly Mock<IDocumentsService> _mockDocumentsService;
    private readonly Mock<IStorageService> _mockStorageService;
    private readonly Mock<HybridCache> _mockCache;
    private readonly Mock<IDocumentRefreshNotifier> _mockNotifier;

    private readonly SyncHtmlDocumentToStorageJobHandler _job;

    public SyncHtmlDocumentToStorageJobHandlerTests()
    {
        _mockDocumentsService = new Mock<IDocumentsService>(MockBehavior.Strict);
        _mockStorageService = new Mock<IStorageService>(MockBehavior.Strict);
        _mockCache = new Mock<HybridCache>(MockBehavior.Loose);
        _mockNotifier = new Mock<IDocumentRefreshNotifier>(MockBehavior.Loose);

        _job = new SyncHtmlDocumentToStorageJobHandler(
            _mockDocumentsService.Object,
            _mockStorageService.Object,
            _mockCache.Object,
            _mockNotifier.Object,
            NullLogger<SyncHtmlDocumentToStorageJobHandler>.Instance);
    }

    [Fact(DisplayName = "Gets document and uploads to storage")]
    public async Task ExecuteAsync_ShouldGetDocumentAndUploadToStorage()
    {
        // Arrange
        const string bylawsHtml = "<h1>Bylaws</h1><p>These are the bylaws...</p>";

        _mockDocumentsService
            .Setup(ds => ds.GetDocumentAsHtmlAsync("bylaws", TestContext.Current.CancellationToken))
            .ReturnsAsync(bylawsHtml);

        _mockStorageService
            .Setup(ss => ss.UploadAsync(
                "documents",
                "bylaws.html",
                bylawsHtml,
                "text/html",
                It.Is<Dictionary<string, string>>(md => md.ContainsKey("LastUpdatedUtc")),
                TestContext.Current.CancellationToken))
            .ReturnsAsync("mock/document/location");

        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = "bylaws",
            Container = "documents",
            Path = "bylaws.html"
        };

        // Act
        await _job.ExecuteAsync(job, TestContext.Current.CancellationToken);

        // Assert
        _mockDocumentsService.VerifyAll();
        _mockStorageService.VerifyAll();
    }

    [Fact(DisplayName = "Throws ArgumentNullException when job is null")]
    public async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenJobIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _job.ExecuteAsync(null!, TestContext.Current.CancellationToken));
    }

    [Theory(DisplayName = "Throws ArgumentException when document key is null or whitespace")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenDocumentKeyIsNullOrWhitespace(string? documentKey)
    {
        // Arrange
        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = documentKey!,
            Container = "documents",
            Path = "bylaws.html"
        };

        // Act & Assert
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _job.ExecuteAsync(job, TestContext.Current.CancellationToken));

        Assert.Contains("DocumentKey", exception.Message);
    }

    [Theory(DisplayName = "Throws ArgumentException when container is null or whitespace")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenContainerIsNullOrWhitespace(string? containerName)
    {
        // Arrange
        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = "bylaws",
            Container = containerName!,
            Path = "bylaws.html"
        };

        // Act & Assert
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _job.ExecuteAsync(job, TestContext.Current.CancellationToken));

        Assert.Contains("Container", exception.Message);
    }

    [Theory(DisplayName = "Throws ArgumentException when path is null or whitespace")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenPathIsNullOrWhitespace(string? documentName)
    {
        // Arrange
        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = "bylaws",
            Container = "documents",
            Path = documentName!
        };

        // Act & Assert
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _job.ExecuteAsync(job, TestContext.Current.CancellationToken));

        Assert.Contains("Path", exception.Message);
    }

    [Fact(DisplayName = "Removes document cache key when provided")]
    public async Task ExecuteAsync_ShouldRemoveDocumentCacheKey_WhenProvided()
    {
        // Arrange
        const string bylawsHtml = "<h1>Bylaws</h1><p>These are the bylaws...</p>";

        _mockDocumentsService
            .Setup(ds => ds.GetDocumentAsHtmlAsync("bylaws", TestContext.Current.CancellationToken))
            .ReturnsAsync(bylawsHtml);

        _mockStorageService
            .Setup(ss => ss.UploadAsync(
                "documents",
                "bylaws.html",
                bylawsHtml,
                "text/html",
                It.IsAny<Dictionary<string, string>>(),
                TestContext.Current.CancellationToken))
            .ReturnsAsync("mock/document/location");

        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = "bylaws",
            Container = "documents",
            Path = "bylaws.html",
            DocumentCacheKey = "document-cache-key"
        };

        // Act
        await _job.ExecuteAsync(job, TestContext.Current.CancellationToken);

        // Assert
        _mockCache.Verify(c => c.RemoveAsync("document-cache-key", TestContext.Current.CancellationToken), Times.Once);
    }

    [Fact(DisplayName = "Removes cache key after delay when provided")]
    public async Task ExecuteAsync_ShouldRemoveCacheKey_AfterDelay_WhenProvided()
    {
        // Arrange
        const string bylawsHtml = "<h1>Bylaws</h1><p>These are the bylaws...</p>";

        _mockDocumentsService
            .Setup(ds => ds.GetDocumentAsHtmlAsync("bylaws", TestContext.Current.CancellationToken))
            .ReturnsAsync(bylawsHtml);

        _mockStorageService
            .Setup(ss => ss.UploadAsync(
                "documents",
                "bylaws.html",
                bylawsHtml,
                "text/html",
                It.IsAny<Dictionary<string, string>>(),
                TestContext.Current.CancellationToken))
            .ReturnsAsync("mock/document/location");

        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = "bylaws",
            Container = "documents",
            Path = "bylaws.html",
            CacheKey = "job-cache-key"
        };

        // Act
        await _job.ExecuteAsync(job, TestContext.Current.CancellationToken);

        // Assert
        _mockCache.Verify(c => c.RemoveAsync("job-cache-key", TestContext.Current.CancellationToken), Times.Once);
    }

    [Fact(DisplayName = "Updates metadata with LastUpdatedUtc and TriggeredBy")]
    public async Task ExecuteAsync_ShouldUpdateMetadata_WithLastUpdatedUtcAndTriggeredBy()
    {
        // Arrange
        const string bylawsHtml = "<h1>Bylaws</h1><p>These are the bylaws...</p>";
        const string triggeredBy = "test-user@example.com";

        _mockDocumentsService
            .Setup(ds => ds.GetDocumentAsHtmlAsync("bylaws", TestContext.Current.CancellationToken))
            .ReturnsAsync(bylawsHtml);

        IDictionary<string, string>? capturedMetadata = null;
        _mockStorageService
            .Setup(ss => ss.UploadAsync(
                "documents",
                "bylaws.html",
                bylawsHtml,
                "text/html",
                It.IsAny<Dictionary<string, string>>(),
                TestContext.Current.CancellationToken))
            .Callback<string, string, string, string, IDictionary<string, string>, CancellationToken>(
                (_, _, _, _, metadata, _) => capturedMetadata = metadata)
            .ReturnsAsync("mock/document/location");

        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = "bylaws",
            Container = "documents",
            Path = "bylaws.html",
            TriggeredBy = triggeredBy
        };

        // Act
        await _job.ExecuteAsync(job, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedMetadata);
        Assert.True(capturedMetadata.ContainsKey("LastUpdatedUtc"));
        Assert.Equal(triggeredBy, capturedMetadata["LastUpdatedBy"]);
    }

    [Fact(DisplayName = "Updates status throughout execution")]
    public async Task ExecuteAsync_ShouldUpdateStatusThroughoutExecution()
    {
        // Arrange
        const string bylawsHtml = "<h1>Bylaws</h1><p>These are the bylaws...</p>";

        _mockDocumentsService
            .Setup(ds => ds.GetDocumentAsHtmlAsync("bylaws", TestContext.Current.CancellationToken))
            .ReturnsAsync(bylawsHtml);

        _mockStorageService
            .Setup(ss => ss.UploadAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                TestContext.Current.CancellationToken))
            .ReturnsAsync("mock/document/location");

        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = "bylaws",
            Container = "documents",
            Path = "bylaws.html",
            HubGroupName = "test-group"
        };

        // Act
        await _job.ExecuteAsync(job, TestContext.Current.CancellationToken);

        // Assert - Verify status updates were called
        _mockNotifier.Verify(
            n => n.NotifyStatusAsync("test-group", DocumentRefreshStatus.Retrieving, null, TestContext.Current.CancellationToken),
            Times.Once);
        _mockNotifier.Verify(
            n => n.NotifyStatusAsync("test-group", DocumentRefreshStatus.Uploading, null, TestContext.Current.CancellationToken),
            Times.Once);
        _mockNotifier.Verify(
            n => n.NotifyStatusAsync("test-group", DocumentRefreshStatus.Completed, null, TestContext.Current.CancellationToken),
            Times.Once);
    }

    [Fact(DisplayName = "Handles exceptions and updates status to failed")]
    public async Task ExecuteAsync_ShouldHandleException_AndUpdateStatusToFailed()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Document service failed");

        _mockDocumentsService
            .Setup(ds => ds.GetDocumentAsHtmlAsync("bylaws", TestContext.Current.CancellationToken))
            .ThrowsAsync(expectedException);

        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = "bylaws",
            Container = "documents",
            Path = "bylaws.html",
            HubGroupName = "test-group"
        };

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _job.ExecuteAsync(job, TestContext.Current.CancellationToken));

        Assert.Same(expectedException, exception);

        // Verify Failed status was sent
        _mockNotifier.Verify(
            n => n.NotifyStatusAsync("test-group", DocumentRefreshStatus.Failed, "Document service failed", TestContext.Current.CancellationToken),
            Times.Once);
    }

    [Fact(DisplayName = "Keeps failed state longer when cache key is provided")]
    public async Task ExecuteAsync_ShouldKeepFailedStateLonger_WhenCacheKeyProvided()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Document service failed");

        _mockDocumentsService
            .Setup(ds => ds.GetDocumentAsHtmlAsync("bylaws", TestContext.Current.CancellationToken))
            .ThrowsAsync(expectedException);

        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = "bylaws",
            Container = "documents",
            Path = "bylaws.html",
            CacheKey = "job-cache-key"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _job.ExecuteAsync(job, TestContext.Current.CancellationToken));

        _mockCache.Verify(c => c.RemoveAsync("job-cache-key", TestContext.Current.CancellationToken), Times.Once);
    }

    [Fact(DisplayName = "Stores job state in cache when cache key is provided")]
    public async Task ExecuteAsync_ShouldStoreJobStateInCache_WhenCacheKeyProvided()
    {
        // Arrange
        const string bylawsHtml = "<h1>Bylaws</h1><p>These are the bylaws...</p>";
        const string cacheKey = "job-cache-key";
        const string triggeredBy = "test-user@example.com";

        _mockDocumentsService
            .Setup(ds => ds.GetDocumentAsHtmlAsync("bylaws", TestContext.Current.CancellationToken))
            .ReturnsAsync(bylawsHtml);

        _mockStorageService
            .Setup(ss => ss.UploadAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                TestContext.Current.CancellationToken))
            .ReturnsAsync("mock/document/location");

        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = "bylaws",
            Container = "documents",
            Path = "bylaws.html",
            CacheKey = cacheKey,
            TriggeredBy = triggeredBy
        };

        // Act
        await _job.ExecuteAsync(job, TestContext.Current.CancellationToken);

        // Assert - Verify cache.SetAsync was called with job state
        _mockCache.Verify(
            c => c.SetAsync(
                cacheKey,
                It.Is<DocumentRefreshJobState>(state =>
                    state.DocumentType == "bylaws" &&
                    state.TriggeredBy == triggeredBy &&
                    state.Status == DocumentRefreshStatus.Retrieving.Name),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IEnumerable<string>>(),
                CancellationToken.None),
            Times.AtLeastOnce);
    }
}
