using Neba.Tests.Website;
using Neba.Website.Application.BowlingCenters;

namespace Neba.UnitTests.Website.BowlingCenters;
[Trait("Category", "Unit")]
[Trait("Component", "Website.BowlingCenters")]

public sealed class ListBowlingCentersQueryHandlerTests
{
    private readonly Mock<IWebsiteBowlingCenterQueryRepository> _repositoryMock;

    private readonly ListBowlingCentersQueryHandler _handler;

    public ListBowlingCentersQueryHandlerTests()
    {
        _repositoryMock = new Mock<IWebsiteBowlingCenterQueryRepository>(MockBehavior.Strict);

        _handler = new ListBowlingCentersQueryHandler(
            _repositoryMock.Object);
    }

    [Fact(DisplayName = "Should return bowling centers from repository")]
    public async Task HandleAsync_ShouldReturnBowlingCentersFromRepository()
    {
        // Arrange
        IReadOnlyCollection<BowlingCenterDto> expectedCenters = BowlingCenterDtoFactory.Bogus(count: 5);
        _repositoryMock
            .Setup(r => r.ListBowlingCentersAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedCenters);

        var query = new ListBowlingCentersQuery();

        // Act
        IReadOnlyCollection<BowlingCenterDto> result = await _handler.HandleAsync(
            query,
            TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeSameAs(expectedCenters);
    }
}
