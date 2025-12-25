using Neba.Application.Messaging;
using Neba.Tests.Website;
using Neba.Website.Application.Awards;
using Neba.Website.Application.Awards.BowlerOfTheYear;

namespace Neba.UnitTests.Website.Awards.BowlerOfTheYear;

public sealed class ListBowlerOfTheYearAwardsQueryHandlerTests
{
    private static readonly string[] ExpectedAwardTags = ["website", "website:awards", "website:award:bowler-of-the-year"];

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

    [Fact]
    public void Query_CacheKey_ShouldFollowConvention()
    {
        // Arrange
        var query = new ListBowlerOfTheYearAwardsQuery();

        // Act
        string key = query.Key;

        // Assert
        key.ShouldBe("website:awards:bowler-of-the-year");
        key.ShouldSatisfyAllConditions(
            k => k.ShouldNotBeNullOrWhiteSpace(),
            k => k.Length.ShouldBeLessThanOrEqualTo(512),
            k => k.Split(':').Length.ShouldBeGreaterThanOrEqualTo(3),
            k => k.Split(':').ShouldAllBe(p => !string.IsNullOrWhiteSpace(p))
        );
        key.Split(':')[0].ShouldBe("website");
        key.Split(':')[1].ShouldBe("awards");
        key.Split(':')[2].ShouldBe("bowler-of-the-year");
        key.Split(':').Length.ShouldBe(3);
    }

    [Fact]
    public void Query_CacheExpiry_ShouldBeDefault7Days()
    {
        // Arrange
        var query = new ListBowlerOfTheYearAwardsQuery();

        // Act & Assert
        ICachedQuery<IReadOnlyCollection<BowlerOfTheYearAwardDto>> cachedQuery = query;
        TimeSpan expiry = cachedQuery.Expiry;

        // Assert
        expiry.ShouldBe(TimeSpan.FromDays(7));
    }

    [Fact]
    public void Query_CacheTags_ShouldIncludeAwardHierarchy()
    {
        // Arrange
        var query = new ListBowlerOfTheYearAwardsQuery();

        // Act
        IReadOnlyCollection<string> tags = query.Tags;

        // Assert
        tags.ShouldBeEquivalentTo(ExpectedAwardTags);
    }
}
