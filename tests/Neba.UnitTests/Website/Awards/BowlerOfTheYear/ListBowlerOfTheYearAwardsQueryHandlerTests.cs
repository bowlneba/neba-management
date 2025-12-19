using Neba.Application.Awards;
using Neba.Application.Awards.BowlerOfTheYear;
using Neba.Tests.Website;

namespace Neba.UnitTests.Website.Awards.BowlerOfTheYear;

public sealed class ListBowlerOfTheYearAwardsQueryHandlerTests
{
    private readonly Mock<IWebsiteAwardQueryRepository> _websiteAwardQueryRepositoryMock;

    private readonly ListBowlerOfTheYearAwardsQueryHandler _handler;

    public ListBowlerOfTheYearAwardsQueryHandlerTests()
    {
        _websiteAwardQueryRepositoryMock = new Mock<IWebsiteAwardQueryRepository>(MockBehavior.Strict);

        _handler = new ListBowlerOfTheYearAwardsQueryHandler(
            _websiteAwardQueryRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAwardsList()
    {
        // Arrange
        IReadOnlyCollection<BowlerOfTheYearAwardDto> expectedAwards = BowlerOfTheYearAwardDtoFactory.Bogus(50);

        _websiteAwardQueryRepositoryMock
            .Setup(repo => repo.ListBowlerOfTheYearAwardsAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedAwards);

        // Act
        IReadOnlyCollection<BowlerOfTheYearAwardDto> awards = await _handler.HandleAsync(new ListBowlerOfTheYearAwardsQuery(), TestContext.Current.CancellationToken);

        // Assert
        awards.ShouldBeEquivalentTo(expectedAwards);
    }
}
