using Neba.Tests.Website;
using Neba.Website.Application.Awards;
using Neba.Website.Application.Awards.HighBlock;

namespace Neba.UnitTests.Website.Awards.HighBlock;

[Trait("Category", "Unit")]
[Trait("Component", "Website.Awards.HighBlock")]

public sealed class ListHigh5GameBlockAwardsQueryHandlerTests
{
    private readonly Mock<IWebsiteAwardQueryRepository> _mockRepository;

    private readonly ListHigh5GameBlockAwardsQueryHandler _handler;

    public ListHigh5GameBlockAwardsQueryHandlerTests()
    {
        _mockRepository = new Mock<IWebsiteAwardQueryRepository>(MockBehavior.Strict);

        _handler = new ListHigh5GameBlockAwardsQueryHandler(_mockRepository.Object);
    }

    [Fact(DisplayName = "Returns high 5-game block awards")]
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
