using Neba.Tests.Website;
using Neba.Website.Application.Tournaments;
using Neba.Website.Application.Tournaments.ListTournaments;

namespace Neba.UnitTests.Website.Tournaments.ListTournaments;

[Trait("Category", "Unit")]
[Trait("Component", "Website.Tournaments")]
public sealed class ListTournamentsInAYearQueryHandlerTests
{
    private readonly Mock<IWebsiteTournamentQueryRepository> _mockTournamentQueryRepository;

    private readonly ListTournamentsInAYearQueryHandler _handler;

    public ListTournamentsInAYearQueryHandlerTests()
    {
        _mockTournamentQueryRepository = new Mock<IWebsiteTournamentQueryRepository>(MockBehavior.Strict);

        _handler = new ListTournamentsInAYearQueryHandler(_mockTournamentQueryRepository.Object);
    }

    [Fact(DisplayName = "Returns tournaments for requested year")]
    public async Task HandleAsync_ValidQuery_ReturnsTournamentsForYear()
    {
        // Arrange
        const int year = 2026;
        IReadOnlyCollection<TournamentSummaryDto> tournaments = TournamentSummaryDtoFactory.Bogus(count: 3, seed: 42);

        _mockTournamentQueryRepository
            .Setup(x => x.ListTournamentsInYearAsync(year, TestContext.Current.CancellationToken))
            .ReturnsAsync(tournaments);

        var query = new ListTournamentInAYearQuery
        {
            Year = year
        };

        // Act
        IReadOnlyCollection<TournamentSummaryDto> result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(tournaments);
    }

    [Fact(DisplayName = "Calls repository with requested year")]
    public async Task HandleAsync_ValidQuery_CallsRepositoryWithYear()
    {
        // Arrange
        const int year = 2025;
        IReadOnlyCollection<TournamentSummaryDto> tournaments = TournamentSummaryDtoFactory.Bogus(count: 2, seed: 7);

        _mockTournamentQueryRepository
            .Setup(x => x.ListTournamentsInYearAsync(year, TestContext.Current.CancellationToken))
            .ReturnsAsync(tournaments);

        var query = new ListTournamentInAYearQuery
        {
            Year = year
        };

        // Act
        await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        _mockTournamentQueryRepository.Verify(
            x => x.ListTournamentsInYearAsync(year, TestContext.Current.CancellationToken),
            Times.Once);
    }

    [Fact(DisplayName = "Returns empty collection when no tournaments in year")]
    public async Task HandleAsync_NoTournamentsFound_ReturnsEmptyCollection()
    {
        // Arrange
        const int year = 2024;
        var emptyTournaments = new List<TournamentSummaryDto>();

        _mockTournamentQueryRepository
            .Setup(x => x.ListTournamentsInYearAsync(year, TestContext.Current.CancellationToken))
            .ReturnsAsync(emptyTournaments);

        var query = new ListTournamentInAYearQuery
        {
            Year = year
        };

        // Act
        IReadOnlyCollection<TournamentSummaryDto> result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeEmpty();
    }
}
