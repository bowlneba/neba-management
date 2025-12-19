using Neba.Application.Documents;
using Neba.Application.Documents.Bylaws;

namespace Neba.UnitTests.Website.Documents;

public sealed class GetBylawsQueryHandlerTests
{
    private readonly Mock<IDocumentsService> _documentsServiceMock;
    private readonly GetBylawsQueryHandler _handler;

    public GetBylawsQueryHandlerTests()
    {
        _documentsServiceMock = new Mock<IDocumentsService>(MockBehavior.Strict);
        _handler = new GetBylawsQueryHandler(_documentsServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnBylawsHtml()
    {
        // Arrange
        const string expectedHtml = "<h1>Bylaws</h1><p>These are the bylaws...</p>";
        _documentsServiceMock
            .Setup(ds => ds.GetDocumentAsHtmlAsync("bylaws", TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedHtml);

        var query = new GetBylawsQuery();

        // Act
        string result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedHtml);
        _documentsServiceMock.Verify(ds => ds.GetDocumentAsHtmlAsync("bylaws", TestContext.Current.CancellationToken), Times.Once);
    }
}
