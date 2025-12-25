using Neba.Application.Messaging;
using Neba.Tests.Website;
using Neba.Website.Application.Awards;
using Neba.Website.Application.Awards.HighBlock;

namespace Neba.UnitTests.Website.Awards.HighBlock;

public sealed class ListHigh5GameBlockAwardsQueryHandlerTests
{
    private static readonly string[] ExpectedAwardTags = ["website", "website:awards", "website:award:high-block"];

    private readonly Mock<IWebsiteAwardQueryRepository> _mockRepository;

    private readonly ListHigh5GameBlockAwardsQueryHandler _handler;

    public ListHigh5GameBlockAwardsQueryHandlerTests()
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

    [Fact]
    public void Query_ShouldImplementICachedQuery()
    {
        // Arrange & Act
        var query = new ListHigh5GameBlockAwardsQuery();

        // Assert
        query.ShouldBeAssignableTo<ICachedQuery<IReadOnlyCollection<HighBlockAwardDto>>>();
    }

    [Fact]
    public void Query_CacheKey_ShouldFollowConvention()
    {
        // Arrange
        var query = new ListHigh5GameBlockAwardsQuery();

        // Act
        string key = query.Key;

        // Assert
        key.ShouldBe("website:awards:high-block");
        key.ShouldSatisfyAllConditions(
            k => k.ShouldNotBeNullOrWhiteSpace(),
            k => k.Length.ShouldBeLessThanOrEqualTo(512),
            k => k.Split(':').Length.ShouldBeGreaterThanOrEqualTo(3),
            k => k.Split(':').ShouldAllBe(p => !string.IsNullOrWhiteSpace(p))
        );
        key.Split(':')[0].ShouldBe("website");
        key.Split(':')[1].ShouldBe("awards");
        key.Split(':')[2].ShouldBe("high-block");
        key.Split(':').Length.ShouldBe(3);
    }

    [Fact]
    public void Query_CacheExpiry_ShouldBeDefault7Days()
    {
        // Arrange
        var query = new ListHigh5GameBlockAwardsQuery();

        // Act & Assert
        ICachedQuery<IReadOnlyCollection<HighBlockAwardDto>> cachedQuery = query;
        TimeSpan expiry = cachedQuery.Expiry;

        // Assert
        expiry.ShouldBe(TimeSpan.FromDays(7));
    }

    [Fact]
    public void Query_CacheTags_ShouldIncludeAwardHierarchy()
    {
        // Arrange
        var query = new ListHigh5GameBlockAwardsQuery();

        // Act
        IReadOnlyCollection<string> tags = query.Tags;

        // Assert
        tags.ShouldBeEquivalentTo(ExpectedAwardTags);
    }
}
