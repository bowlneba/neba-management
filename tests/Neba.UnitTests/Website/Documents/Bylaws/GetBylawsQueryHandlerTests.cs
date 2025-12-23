using Neba.Application.Documents;
using Neba.Application.Storage;
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
    public async Task HandleAsync_ShouldReturnBylawsHtml()
    {
        // Arrange
        const string expectedHtml = "<h1>Bylaws</h1><p>These are the bylaws...</p>";

        _storageServiceMock
            .Setup(ds => ds.GetContentAsync("documents", "bylaws.html", TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedHtml);

        var query = new GetBylawsQuery();

        // Act
        string result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedHtml);
        _storageServiceMock.Verify(ds => ds.GetContentAsync("documents", "bylaws.html", TestContext.Current.CancellationToken), Times.Once);
    }
}
