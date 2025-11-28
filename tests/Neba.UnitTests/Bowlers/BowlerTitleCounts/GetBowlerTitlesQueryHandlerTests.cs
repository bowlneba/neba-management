using ErrorOr;
using Moq;
using Neba.Application.Bowlers;
using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Domain.Bowlers;
using Neba.Tests;

namespace Neba.UnitTests.Bowlers.BowlerTitleCounts;

public sealed class GetBowlerTitlesQueryHandlerTests
{
    private readonly Mock<IWebsiteBowlerQueryRepository> _mockWebsiteBowlerQueryRepository;

    private readonly GetBowlerTitlesQueryHandler _queryHandler;

    public GetBowlerTitlesQueryHandlerTests()
    {
        _mockWebsiteBowlerQueryRepository = new Mock<IWebsiteBowlerQueryRepository>(MockBehavior.Strict);

        _queryHandler = new GetBowlerTitlesQueryHandler(
            _mockWebsiteBowlerQueryRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnBowlerNotFoundError_WhenBowlerDoesNotExist()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        GetBowlerTitlesQuery query = new() { BowlerId = bowlerId };

        _mockWebsiteBowlerQueryRepository
            .Setup(repo => repo.GetBowlerTitlesAsync(bowlerId, TestContext.Current.CancellationToken))
            .ReturnsAsync((BowlerTitlesDto?)null);

        // Act
        ErrorOr<BowlerTitlesDto?> result = await _queryHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Bowler.NotFound");
        result.FirstError.Description.ShouldBe("Bowler was not found.");
        result.FirstError.Metadata!["bowlerId"].ShouldBe(bowlerId);

        Console.WriteLine(result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnBowlerTitlesDto_WhenBowlerExists()
    {
        // Arrange
        BowlerTitlesDto bowler = BowlerTitlesDtoFactory.Bogus();
        GetBowlerTitlesQuery query = new() { BowlerId = bowler.BowlerId };

        _mockWebsiteBowlerQueryRepository
            .Setup(repo => repo.GetBowlerTitlesAsync(bowler.BowlerId, TestContext.Current.CancellationToken))
            .ReturnsAsync(bowler);

        // Act
        ErrorOr<BowlerTitlesDto?> result = await _queryHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(bowler);
    }
}
