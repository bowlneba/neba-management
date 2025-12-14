using Neba.Application.Bowlers.BowlerTitles;
using Neba.Tests;

namespace Neba.UnitTests.Bowlers.BowlerTitles;

public sealed class ListBowlerTitlesQueryHandlerTests
{
    private readonly Mock<IWebsiteTitleQueryRepository> _mockWebsiteTitleQueryRepository;

    private readonly ListBowlerTitlesQueryHandler _queryHandler;

    public ListBowlerTitlesQueryHandlerTests()
    {
        _mockWebsiteTitleQueryRepository = new Mock<IWebsiteTitleQueryRepository>(MockBehavior.Strict);

        _queryHandler = new ListBowlerTitlesQueryHandler(
            _mockWebsiteTitleQueryRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllTitles()
    {
        // Arrange
        IReadOnlyCollection<BowlerTitleDto> seedTitles = BowlerTitleDtoFactory.Bogus(100);

        _mockWebsiteTitleQueryRepository
            .Setup(repository => repository.ListTitlesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(seedTitles);

        ListBowlerTitlesQuery query = new();

        // Act
        IReadOnlyCollection<BowlerTitleDto> titles = await _queryHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        titles.ShouldBeEquivalentTo(seedTitles);
    }
}
