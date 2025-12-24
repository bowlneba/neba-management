using Microsoft.Extensions.Logging.Abstractions;
using Neba.Application.Documents;
using Neba.Application.Storage;
using Neba.Tests.Documents;
using Neba.Website.Application.Tournaments.TournamentRules;

namespace Neba.UnitTests.Website.Tournaments.TournamentRules;

public sealed class GetTournamentRulesQueryHandlerTests
{
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IDocumentsService> _tournamentsServiceMock;
    private readonly Mock<ITournamentRulesSyncBackgroundJob> _tournamentRulesSyncJobMock;
    private readonly GetTournamentRulesQueryHandler _handler;

    public GetTournamentRulesQueryHandlerTests()
    {
        _storageServiceMock = new Mock<IStorageService>(MockBehavior.Strict);
        _tournamentsServiceMock = new Mock<IDocumentsService>(MockBehavior.Strict);
        _tournamentRulesSyncJobMock = new Mock<ITournamentRulesSyncBackgroundJob>(MockBehavior.Loose);
        _handler = new GetTournamentRulesQueryHandler(
            _storageServiceMock.Object,
            _tournamentsServiceMock.Object,
            _tournamentRulesSyncJobMock.Object,
            NullLogger<GetTournamentRulesQueryHandler>.Instance);
    }

    [Fact]
    public async Task HandleAsync_WhenDocumentExistsInStorage_ShouldReturnFromStorage()
    {
        // Arrange
        DocumentDto expectedDocument = DocumentDtoFactory.CreateTournamentRules();

        _storageServiceMock
            .Setup(ss => ss.ExistsAsync("tournaments", "tournament-rules.html", TestContext.Current.CancellationToken))
            .ReturnsAsync(true);

        _storageServiceMock
            .Setup(ss => ss.GetContentWithMetadataAsync("tournaments", "tournament-rules.html", TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedDocument);

        var query = new GetTournamentRulesQuery();

        // Act
        DocumentDto result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedDocument);
        _storageServiceMock.Verify(ss => ss.ExistsAsync("tournaments", "tournament-rules.html", TestContext.Current.CancellationToken), Times.Once);
        _storageServiceMock.Verify(ss => ss.GetContentWithMetadataAsync("tournaments", "tournament-rules.html", TestContext.Current.CancellationToken), Times.Once);
        _tournamentsServiceMock.Verify(ds => ds.GetDocumentAsHtmlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _tournamentRulesSyncJobMock.Verify(bj => bj.TriggerImmediateSync(), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenDocumentNotInStorage_ShouldRetrieveFromSourceAndTriggerBackgroundSync()
    {
        // Arrange
        const string tournamentRulesHtml = "<h1>Tournament Rules</h1><p>These are the rules...</p>";

        _storageServiceMock
            .Setup(ss => ss.ExistsAsync("tournaments", "tournament-rules.html", TestContext.Current.CancellationToken))
            .ReturnsAsync(false);

        _tournamentsServiceMock
            .Setup(ds => ds.GetDocumentAsHtmlAsync("tournament-rules", TestContext.Current.CancellationToken))
            .ReturnsAsync(tournamentRulesHtml);

        _tournamentRulesSyncJobMock
            .Setup(bj => bj.TriggerImmediateSync())
            .Returns("mock-job-id");

        var query = new GetTournamentRulesQuery();

        // Act
        DocumentDto result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.Content.ShouldBe(tournamentRulesHtml);
        result.Metadata.ShouldContainKey("LastUpdatedUtc");
        result.Metadata.ShouldContainKey("LastUpdatedBy");
        _storageServiceMock.Verify(ss => ss.ExistsAsync("tournaments", "tournament-rules.html", TestContext.Current.CancellationToken), Times.Once);
        _tournamentsServiceMock.Verify(ds => ds.GetDocumentAsHtmlAsync("tournament-rules", TestContext.Current.CancellationToken), Times.Once);
        _tournamentRulesSyncJobMock.Verify(bj => bj.TriggerImmediateSync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenBackgroundSyncFails_ShouldStillReturnDocument()
    {
        // Arrange
        const string tournamentRulesHtml = "<h1>Tournament Rules</h1><p>These are the rules...</p>";

        _storageServiceMock
            .Setup(ss => ss.ExistsAsync("tournaments", "tournament-rules.html", TestContext.Current.CancellationToken))
            .ReturnsAsync(false);

        _tournamentsServiceMock
            .Setup(ds => ds.GetDocumentAsHtmlAsync("tournament-rules", TestContext.Current.CancellationToken))
            .ReturnsAsync(tournamentRulesHtml);

        _tournamentRulesSyncJobMock
            .Setup(bj => bj.TriggerImmediateSync())
            .Throws(new InvalidOperationException("Background job unavailable"));

        var query = new GetTournamentRulesQuery();

        // Act
        DocumentDto result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.Content.ShouldBe(tournamentRulesHtml);
        result.Metadata.ShouldContainKey("LastUpdatedUtc");
        result.Metadata.ShouldContainKey("LastUpdatedBy");
        _tournamentsServiceMock.Verify(ds => ds.GetDocumentAsHtmlAsync("tournament-rules", TestContext.Current.CancellationToken), Times.Once);
    }
}
