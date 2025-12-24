using Neba.Application.Documents;
using Neba.Application.Storage;
using Neba.Tests.Documents;
using Neba.Website.Application.Tournaments.TournamentRules;

namespace Neba.UnitTests.Website.Tournaments.TournamentRules;

public sealed class GetTournamentRulesQueryHandlerTests
{
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly GetTournamentRulesQueryHandler _handler;

    public GetTournamentRulesQueryHandlerTests()
    {
        _storageServiceMock = new Mock<IStorageService>(MockBehavior.Strict);
        _handler = new GetTournamentRulesQueryHandler(_storageServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnTournamentRulesDocument()
    {
        // Arrange
        DocumentDto expectedDocument = DocumentDtoFactory.CreateTournamentRules();
        _storageServiceMock
            .Setup(ds => ds.GetContentWithMetadataAsync("tournaments", "tournament-rules", TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedDocument);

        var query = new GetTournamentRulesQuery();

        // Act
        DocumentDto result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedDocument);
        _storageServiceMock.Verify(ds => ds.GetContentWithMetadataAsync("tournaments", "tournament-rules", TestContext.Current.CancellationToken), Times.Once);
    }
}
