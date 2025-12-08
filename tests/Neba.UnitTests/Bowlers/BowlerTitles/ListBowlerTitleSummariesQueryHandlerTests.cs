using ErrorOr;
using Neba.Application.Bowlers;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Tests;

namespace Neba.UnitTests.Bowlers.BowlerTitles;

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

    [Fact]
    public async Task HandleAsync_ShouldReturnAllBowlersTitlesSummary()
    {
        // Arrange
        IReadOnlyCollection<BowlerTitleSummaryDto> seedSummaries = BowlerTitleSummaryDtoFactory.Bogus(50);

        _mockWebsiteTitleQueryRepository
            .Setup(repository => repository.ListTitleSummariesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(seedSummaries);

        ListBowlerTitleSummariesQuery query = new();

        // Act
        ErrorOr<IReadOnlyCollection<BowlerTitleSummaryDto>> result = await _queryHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();

        IReadOnlyCollection<BowlerTitleSummaryDto> summaries = result.Value;
        summaries.ShouldBeEquivalentTo(seedSummaries);
    }
}
