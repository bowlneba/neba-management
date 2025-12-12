using Neba.Application.Awards;
using Neba.Tests;

namespace Neba.UnitTests.Awards;

public sealed class ListHighBlockAwardsQueryHandlerTests
{
    private readonly Mock<IWebsiteAwardQueryRepository> _mockRepository;

    private readonly ListHigh5GameBlockAwardsQueryHandler _handler;

    public ListHighBlockAwardsQueryHandlerTests()
    {
        _mockRepository = new Mock<IWebsiteAwardQueryRepository>(MockBehavior.Strict);

        _handler = new ListHigh5GameBlockAwardsQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnExpectedResult()
    {
        // Arrange
        IReadOnlyCollection<HighBlockAwardDto> highBlockAwardDto = HighBlockAwardDtoFactory.Bogus(30);

        _mockRepository
            .Setup(x => x.ListHigh5GameBlockAwardsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(highBlockAwardDto);

        ListHigh5GameBlockAwardsQuery query = new();

        // Act
        IReadOnlyCollection<HighBlockAwardDto> result = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeEquivalentTo(highBlockAwardDto);
    }
}
