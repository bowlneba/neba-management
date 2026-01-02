using Neba.Tests.Website;
using Neba.Website.Application.Bowlers.BowlerTitles;

namespace Neba.UnitTests.Website.Bowlers.BowlerTitles;

public sealed class ListBowlerTitleSummariesQueryHandlerTests
{
    private readonly Mock<IWebsiteTitleQueryRepository> _mockWebsiteTitleQueryRepository;

    private readonly ListBowlerTitleSummariesQueryHandler _queryHandler;

    public ListBowlerTitleSummariesQueryHandlerTests()
    {
        _mockWebsiteTitleQueryRepository = new Mock<IWebsiteTitleQueryRepository>(MockBehavior.Strict);

        _queryHandler = new ListBowlerTitleSummariesQueryHandler(
            _mockWebsiteTitleQueryRepository.Object);
    }

    [Fact(DisplayName = "Returns all bowlers titles summary")]
    public async Task HandleAsync_ShouldReturnAllBowlersTitlesSummary()
    {
        // Arrange
        IReadOnlyCollection<BowlerTitleSummaryDto> seedSummaries = BowlerTitleSummaryDtoFactory.Bogus(50);

        _mockWebsiteTitleQueryRepository
            .Setup(repository => repository.ListTitleSummariesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(seedSummaries);

        ListBowlerTitleSummariesQuery query = new();

        // Act
        IReadOnlyCollection<BowlerTitleSummaryDto> summaries = await _queryHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        summaries.ShouldBeEquivalentTo(seedSummaries);
    }
}
