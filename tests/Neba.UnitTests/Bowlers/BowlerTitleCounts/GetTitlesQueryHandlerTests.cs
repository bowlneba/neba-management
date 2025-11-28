using ErrorOr;
using Moq;
using Neba.Application.Bowlers;
using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Tests;

namespace Neba.UnitTests.Bowlers.BowlerTitleCounts;

public sealed class GetTitlesQueryHandlerTests
{
    private readonly Mock<IWebsiteBowlerQueryRepository> _mockWebsiteBowlerQueryRepository;

    private readonly GetTitlesQueryHandler _queryHandler;

    public GetTitlesQueryHandlerTests()
    {
        _mockWebsiteBowlerQueryRepository = new Mock<IWebsiteBowlerQueryRepository>(MockBehavior.Strict);

        _queryHandler = new GetTitlesQueryHandler(
            _mockWebsiteBowlerQueryRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllTitles()
    {
        // Arrange
        IReadOnlyCollection<BowlerTitleDto> seedTitles = BowlerTitleDtoFactory.Bogus(100);

        _mockWebsiteBowlerQueryRepository
            .Setup(repository => repository.GetTitlesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(seedTitles);

        GetTitlesQuery query = new();

        // Act
        ErrorOr<IReadOnlyCollection<BowlerTitleDto>> result = await _queryHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();

        IReadOnlyCollection<BowlerTitleDto> titles = result.Value;
        titles.ShouldBeEquivalentTo(seedTitles);
    }
}
