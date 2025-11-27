using ErrorOr;
using Moq;
using Neba.Application.Bowlers;
using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Tests;

namespace Neba.UnitTests.Bowlers.BowlerTitleCounts;

public sealed class GetBowlerTitleCountsQueryHandlerTests
{
    private readonly Mock<IWebsiteBowlerQueryRepository> _bowlerQueryRepositoryMock;

    private readonly GetBowlerTitleCountsQueryHandler _queryHandler;

    public GetBowlerTitleCountsQueryHandlerTests()
    {
        _bowlerQueryRepositoryMock = new Mock<IWebsiteBowlerQueryRepository>(MockBehavior.Strict);

        _queryHandler = new GetBowlerTitleCountsQueryHandler(
            _bowlerQueryRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnBowlerTitleCounts()
    {
        // Arrange
        IReadOnlyCollection<BowlerTitleCountDto> bowlerTitleCounts = BowlerTitleCountDtoFactory.Bogus(100);
        _bowlerQueryRepositoryMock
            .Setup(repo => repo.GetBowlerTitleCountsAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(bowlerTitleCounts);

        // Act
        ErrorOr<IReadOnlyCollection<BowlerTitleCountDto>> result = await _queryHandler.HandleAsync(
            new GetBowlerTitleCountsQuery(),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();

        IReadOnlyCollection<BowlerTitleCountDto> returnedBowlerTitleCounts = result.Value;
        returnedBowlerTitleCounts.ShouldNotBeNull();
        returnedBowlerTitleCounts.ShouldBe(bowlerTitleCounts);
    }
}
