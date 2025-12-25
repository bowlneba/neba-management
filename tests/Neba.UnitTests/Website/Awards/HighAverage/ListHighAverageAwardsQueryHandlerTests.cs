using Neba.Application.Messaging;
using Neba.Tests.Website;
using Neba.Website.Application.Awards;
using Neba.Website.Application.Awards.HighAverage;

namespace Neba.UnitTests.Website.Awards.HighAverage;

public sealed class ListHighAverageAwardsQueryHandlerTests
{
    private static readonly string[] ExpectedAwardTags = ["website", "website:awards", "website:award:high-average"];

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

    [Fact]
    public void Query_ShouldImplementICachedQuery()
    {
        // Arrange & Act
        var query = new ListHighAverageAwardsQuery();

        // Assert
        query.ShouldBeAssignableTo<ICachedQuery<IReadOnlyCollection<HighAverageAwardDto>>>();
    }

    [Fact]
    public void Query_CacheKey_ShouldFollowConvention()
    {
        // Arrange
        var query = new ListHighAverageAwardsQuery();

        // Act
        string key = query.Key;

        // Assert
        key.ShouldBe("website:awards:high-average");
        key.ShouldSatisfyAllConditions(
            k => k.ShouldNotBeNullOrWhiteSpace(),
            k => k.Length.ShouldBeLessThanOrEqualTo(512),
            k => k.Split(':').Length.ShouldBeGreaterThanOrEqualTo(3),
            k => k.Split(':').ShouldAllBe(p => !string.IsNullOrWhiteSpace(p))
        );
        key.Split(':')[0].ShouldBe("website");
        key.Split(':')[1].ShouldBe("awards");
        key.Split(':')[2].ShouldBe("high-average");
        key.Split(':').Length.ShouldBe(3);
    }

    [Fact]
    public void Query_CacheExpiry_ShouldBeDefault7Days()
    {
        // Arrange
        var query = new ListHighAverageAwardsQuery();

        // Act & Assert
        ICachedQuery<IReadOnlyCollection<HighAverageAwardDto>> cachedQuery = query;
        TimeSpan expiry = cachedQuery.Expiry;

        // Assert
        expiry.ShouldBe(TimeSpan.FromDays(7));
    }

    [Fact]
    public void Query_CacheTags_ShouldIncludeAwardHierarchy()
    {
        // Arrange
        var query = new ListHighAverageAwardsQuery();

        // Act
        IReadOnlyCollection<string> tags = query.Tags;

        // Assert
        tags.ShouldBeEquivalentTo(ExpectedAwardTags);
    }
}
