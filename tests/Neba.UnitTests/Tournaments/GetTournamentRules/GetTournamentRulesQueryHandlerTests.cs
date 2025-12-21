using Neba.Application.Documents;
using Neba.Website.Application.Tournaments.GetTournamentRules;

namespace Neba.UnitTests.Tournaments.GetTournamentRules;

public sealed class GetTournamentRulesQueryHandlerTests
{
    private readonly Mock<IDocumentsService> _documentsServiceMock;
    private readonly GetTournamentRulesQueryHandler _handler;

    public GetTournamentRulesQueryHandlerTests()
    {
        _documentsServiceMock = new Mock<IDocumentsService>(MockBehavior.Strict);
        _handler = new GetTournamentRulesQueryHandler(_documentsServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnTournamentRulesHtml()
    {
        // Arrange
        const string expectedHtml = "<h1>Tournament Rules</h1><p>These are the rules...</p>";
        _documentsServiceMock
            .Setup(ds => ds.GetDocumentAsHtmlAsync("tournament-rules", TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedHtml);

        var query = new GetTournamentRulesQuery();

        // Act
        string result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedHtml);
        _documentsServiceMock.Verify(ds => ds.GetDocumentAsHtmlAsync("tournament-rules", TestContext.Current.CancellationToken), Times.Once);
    }
}
