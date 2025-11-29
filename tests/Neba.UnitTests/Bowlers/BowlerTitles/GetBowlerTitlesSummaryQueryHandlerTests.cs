using ErrorOr;
using Neba.Application.Bowlers;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Tests;

namespace Neba.UnitTests.Bowlers.BowlerTitles;

public sealed class GetBowlerTitlesSummaryQueryHandlerTests
{
    private readonly Mock<IWebsiteBowlerQueryRepository> _mockWebsiteBowlerQueryRepository;

    private readonly GetBowlersTitlesSummaryQueryHandler _queryHandler;

    public GetBowlerTitlesSummaryQueryHandlerTests()
    {
        _mockWebsiteBowlerQueryRepository = new Mock<IWebsiteBowlerQueryRepository>(MockBehavior.Strict);

        _queryHandler = new GetBowlersTitlesSummaryQueryHandler(
            _mockWebsiteBowlerQueryRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllBowlersTitlesSummary()
    {
        // Arrange
        IReadOnlyCollection<BowlerTitlesSummaryDto> seedSummaries = BowlerTitlesSummaryDtoFactory.Bogus(50);

        _mockWebsiteBowlerQueryRepository
            .Setup(repository => repository.GetAllBowlerTitlesSummaryAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(seedSummaries);

        GetBowlersTitlesSummaryQuery query = new();

        // Act
        ErrorOr<IReadOnlyCollection<BowlerTitlesSummaryDto>> result = await _queryHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();

        IReadOnlyCollection<BowlerTitlesSummaryDto> summaries = result.Value;
        summaries.ShouldBeEquivalentTo(seedSummaries);
    }
}
