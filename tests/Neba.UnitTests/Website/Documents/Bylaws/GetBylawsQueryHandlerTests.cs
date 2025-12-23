using Neba.Application.Documents;
using Neba.Application.Storage;
using Neba.Tests.Documents;
using Neba.Website.Application.Documents.Bylaws;

namespace Neba.UnitTests.Website.Documents.Bylaws;

public sealed class GetBylawsQueryHandlerTests
{
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly GetBylawsQueryHandler _handler;

    public GetBylawsQueryHandlerTests()
    {
        _storageServiceMock = new Mock<IStorageService>(MockBehavior.Strict);
        _handler = new GetBylawsQueryHandler(_storageServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnBylawsDocument()
    {
        // Arrange
        DocumentDto expectedDocument = DocumentDtoFactory.CreateBylaws();

        _storageServiceMock
            .Setup(ds => ds.GetContentWithMetadataAsync("documents", "bylaws.html", TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedDocument);

        var query = new GetBylawsQuery();

        // Act
        DocumentDto result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedDocument);
        _storageServiceMock.Verify(ds => ds.GetContentWithMetadataAsync("documents", "bylaws.html", TestContext.Current.CancellationToken), Times.Once);
    }
}
