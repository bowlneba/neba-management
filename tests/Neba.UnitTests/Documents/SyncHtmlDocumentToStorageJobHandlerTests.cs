using Microsoft.Extensions.Logging.Abstractions;
using Neba.Application.Documents;
using Neba.Application.Storage;
using Neba.Website.Application.Documents.Bylaws;

namespace Neba.UnitTests.Documents;

public sealed class SyncHtmlDocumentToStorageJobHandlerTests
{
    private readonly Mock<IDocumentsService> _mockDocumentsService;
    private readonly Mock<IStorageService> _mockStorageService;

    private readonly SyncHtmlDocumentToStorageJobHandler _job;

    public SyncHtmlDocumentToStorageJobHandlerTests()
    {
        _mockDocumentsService = new Mock<IDocumentsService>(MockBehavior.Strict);
        _mockStorageService = new Mock<IStorageService>(MockBehavior.Strict);

        _job = new SyncHtmlDocumentToStorageJobHandler(
            _mockDocumentsService.Object,
            _mockStorageService.Object,
            NullLogger<SyncHtmlDocumentToStorageJobHandler>.Instance);
    }

    [Fact]
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
                It.Is<Dictionary<string, string>>(md => md.ContainsKey("syncedAt")),
                TestContext.Current.CancellationToken))
            .ReturnsAsync("mock/document/location");

        var job = new SyncHtmlDocumentToStorageJob
        {
            DocumentKey = "bylaws",
            ContainerName = "documents",
            DocumentName = "bylaws.html"
        };

        // Act
        await _job.ExecuteAsync(job, TestContext.Current.CancellationToken);

        // Assert
        _mockDocumentsService.VerifyAll();
        _mockStorageService.VerifyAll();
    }
}
