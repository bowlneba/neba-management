using Neba.Tests.Website;
using Neba.Website.Application.Awards;
using Neba.Website.Application.Awards.HighAverage;

namespace Neba.UnitTests.Website.Awards.HighAverage;

public sealed class ListHighAverageAwardsQueryHandlerTests
{
    private readonly Mock<IWebsiteAwardQueryRepository> _mockRepository;

    private readonly ListHighAverageAwardsQueryHandler _handler;

    public ListHighAverageAwardsQueryHandlerTests()
    {
        _mockRepository = new Mock<IWebsiteAwardQueryRepository>(MockBehavior.Strict);

        _handler = new ListHighAverageAwardsQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnExpectedResult_WhenRepositoryReturnsData()
    {
        // Arrange
        IReadOnlyCollection<HighAverageAwardDto> expected = HighAverageAwardDtoFactory.Bogus(50, 1900);

        _mockRepository
            .Setup(repository => repository.ListHighAverageAwardsAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(expected);

        var query = new ListHighAverageAwardsQuery();

        // Act
        IReadOnlyCollection<HighAverageAwardDto> actual = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }
}
