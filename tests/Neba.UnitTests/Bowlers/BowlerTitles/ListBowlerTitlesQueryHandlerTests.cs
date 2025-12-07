using ErrorOr;
using Neba.Application.Bowlers;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Tests;

namespace Neba.UnitTests.Bowlers.BowlerTitles;

public sealed class ListBowlerTitlesQueryHandlerTests
{
    private readonly Mock<IWebsiteBowlerQueryRepository> _mockWebsiteBowlerQueryRepository;

    private readonly ListBowlerTitlesQueryHandler _queryHandler;

    public ListBowlerTitlesQueryHandlerTests()
    {
        _mockWebsiteBowlerQueryRepository = new Mock<IWebsiteBowlerQueryRepository>(MockBehavior.Strict);

        _queryHandler = new ListBowlerTitlesQueryHandler(
            _mockWebsiteBowlerQueryRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllTitles()
    {
        // Arrange
        IReadOnlyCollection<BowlerTitleDto> seedTitles = BowlerTitleDtoFactory.Bogus(100);

        _mockWebsiteBowlerQueryRepository
            .Setup(repository => repository.ListBowlerTitlesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(seedTitles);

        ListBowlerTitlesQuery query = new();

        // Act
        ErrorOr<IReadOnlyCollection<BowlerTitleDto>> result = await _queryHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();

        IReadOnlyCollection<BowlerTitleDto> titles = result.Value;
        titles.ShouldBeEquivalentTo(seedTitles);
    }
}
