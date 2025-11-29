using ErrorOr;
using Moq;
using Neba.Application.Bowlers;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Tests;

namespace Neba.UnitTests.Bowlers.BowlerTitles;

public sealed class GetTitlesQueryHandlerTests
{
    private readonly Mock<IWebsiteBowlerQueryRepository> _mockWebsiteBowlerQueryRepository;

    private readonly GetBowlersTitlesQueryHandler _queryHandler;

    public GetTitlesQueryHandlerTests()
    {
        _mockWebsiteBowlerQueryRepository = new Mock<IWebsiteBowlerQueryRepository>(MockBehavior.Strict);

        _queryHandler = new GetBowlersTitlesQueryHandler(
            _mockWebsiteBowlerQueryRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllTitles()
    {
        // Arrange
        IReadOnlyCollection<BowlerTitleDto> seedTitles = BowlerTitleDtoFactory.Bogus(100);

        _mockWebsiteBowlerQueryRepository
            .Setup(repository => repository.GetBowlerTitlesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(seedTitles);

        GetBowlersTitlesQuery query = new();

        // Act
        ErrorOr<IReadOnlyCollection<BowlerTitleDto>> result = await _queryHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();

        IReadOnlyCollection<BowlerTitleDto> titles = result.Value;
        titles.ShouldBeEquivalentTo(seedTitles);
    }
}
