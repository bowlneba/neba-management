using Neba.Application.Messaging;
using Neba.Tests.Website;
using Neba.Website.Application.Awards;
using Neba.Website.Application.Awards.BowlerOfTheYear;

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

    [Fact]
    public void Query_ShouldImplementICachedQuery()
    {
        // Arrange & Act
        var query = new ListBowlerOfTheYearAwardsQuery();

        // Assert
        query.ShouldBeAssignableTo<ICachedQuery<IReadOnlyCollection<BowlerOfTheYearAwardDto>>>();
    }
}
