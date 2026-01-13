using Neba.Application;
using Neba.Tests.Website;
using Neba.Website.Application.Tournaments;
using Neba.Website.Application.Tournaments.ListTournaments;

namespace Neba.UnitTests.Website.Tournaments.ListTournaments;

[Trait("Category", "Unit")]
[Trait("Component", "Website.Tournaments")]
public sealed class ListFutureTournamentsQueryHandlerTests
{
    private readonly Mock<IWebsiteTournamentQueryRepository> _mockTournamentQueryRepository;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;

    private readonly ListFutureTournamentsQueryHandler _handler;

    public ListFutureTournamentsQueryHandlerTests()
    {
        _mockTournamentQueryRepository = new Mock<IWebsiteTournamentQueryRepository>(MockBehavior.Strict);
        _mockDateTimeProvider = new Mock<IDateTimeProvider>(MockBehavior.Strict);

        _handler = new ListFutureTournamentsQueryHandler(
            _mockTournamentQueryRepository.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact(DisplayName = "Returns tournaments from repository for valid query")]
    public async Task HandleAsync_ValidQuery_ReturnsTournamentsFromRepository()
    {
        // Arrange
        var today = new DateOnly(2026, 1, 13);
        IReadOnlyCollection<TournamentSummaryDto> tournaments = TournamentSummaryDtoFactory.Bogus(count: 3, seed: 42);

        _mockDateTimeProvider
            .Setup(x => x.Today)
            .Returns(today);

        _mockTournamentQueryRepository
            .Setup(x => x.ListTournamentsAfterDateAsync(today, TestContext.Current.CancellationToken))
            .ReturnsAsync(tournaments);

        var query = new ListFutureTournamentsQuery();

        // Act
        IReadOnlyCollection<TournamentSummaryDto> result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(tournaments);
    }

    [Fact(DisplayName = "Calls repository with today's date from date time provider")]
    public async Task HandleAsync_ValidQuery_CallsRepositoryWithTodayDate()
    {
        // Arrange
        var today = new DateOnly(2026, 1, 13);
        IReadOnlyCollection<TournamentSummaryDto> tournaments = TournamentSummaryDtoFactory.Bogus(count: 2, seed: 123);

        _mockDateTimeProvider
            .Setup(x => x.Today)
            .Returns(today);

        _mockTournamentQueryRepository
            .Setup(x => x.ListTournamentsAfterDateAsync(today, TestContext.Current.CancellationToken))
            .ReturnsAsync(tournaments);

        var query = new ListFutureTournamentsQuery();

        // Act
        await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        _mockTournamentQueryRepository.Verify(
            x => x.ListTournamentsAfterDateAsync(today, TestContext.Current.CancellationToken),
            Times.Once);
    }

    [Fact(DisplayName = "Returns empty collection when no tournaments found")]
    public async Task HandleAsync_NoTournamentsFound_ReturnsEmptyCollection()
    {
        // Arrange
        var today = new DateOnly(2026, 1, 13);
        var emptyTournaments = new List<TournamentSummaryDto>();

        _mockDateTimeProvider
            .Setup(x => x.Today)
            .Returns(today);

        _mockTournamentQueryRepository
            .Setup(x => x.ListTournamentsAfterDateAsync(today, TestContext.Current.CancellationToken))
            .ReturnsAsync(emptyTournaments);

        var query = new ListFutureTournamentsQuery();

        // Act
        IReadOnlyCollection<TournamentSummaryDto> result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeEmpty();
    }
}
