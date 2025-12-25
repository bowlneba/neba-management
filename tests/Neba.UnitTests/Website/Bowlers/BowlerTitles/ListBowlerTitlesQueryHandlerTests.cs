using Neba.Application.Messaging;
using Neba.Tests.Website;
using Neba.Website.Application.Bowlers.BowlerTitles;

namespace Neba.UnitTests.Website.Bowlers.BowlerTitles;

public sealed class ListBowlerTitlesQueryHandlerTests
{
    private static readonly string[] ExpectedAllBowlersTags = ["website", "website:bowlers"];

    private readonly Mock<IWebsiteTitleQueryRepository> _mockWebsiteTitleQueryRepository;

    private readonly ListBowlerTitlesQueryHandler _queryHandler;

    public ListBowlerTitlesQueryHandlerTests()
    {
        _mockWebsiteTitleQueryRepository = new Mock<IWebsiteTitleQueryRepository>(MockBehavior.Strict);

        _queryHandler = new ListBowlerTitlesQueryHandler(
            _mockWebsiteTitleQueryRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllTitles()
    {
        // Arrange
        IReadOnlyCollection<BowlerTitleDto> seedTitles = BowlerTitleDtoFactory.Bogus(100);

        _mockWebsiteTitleQueryRepository
            .Setup(repository => repository.ListTitlesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(seedTitles);

        ListBowlerTitlesQuery query = new();

        // Act
        IReadOnlyCollection<BowlerTitleDto> titles = await _queryHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        titles.ShouldBeEquivalentTo(seedTitles);
    }

    [Fact]
    public void Query_ShouldImplementICachedQuery()
    {
        // Arrange & Act
        var query = new ListBowlerTitlesQuery();

        // Assert
        query.ShouldBeAssignableTo<ICachedQuery<IReadOnlyCollection<BowlerTitleDto>>>();
    }

    [Fact]
    public void Query_CacheKey_ShouldFollowConvention()
    {
        // Arrange
        var query = new ListBowlerTitlesQuery();

        // Act
        string key = query.Key;

        // Assert
        key.ShouldBe("website:query:ListBowlerTitlesQuery");
        key.ShouldSatisfyAllConditions(
            k => k.ShouldNotBeNullOrWhiteSpace(),
            k => k.Length.ShouldBeLessThanOrEqualTo(512),
            k => k.Split(':').Length.ShouldBeGreaterThanOrEqualTo(3),
            k => k.Split(':').ShouldAllBe(p => !string.IsNullOrWhiteSpace(p))
        );
        key.Split(':')[0].ShouldBe("website");
        key.Split(':')[1].ShouldBe("query");
        key.Split(':')[2].ShouldBe("ListBowlerTitlesQuery");
        key.Split(':').Length.ShouldBe(3);
    }

    [Fact]
    public void Query_CacheExpiry_ShouldBeDefault7Days()
    {
        // Arrange
        var query = new ListBowlerTitlesQuery();

        // Act & Assert
        ICachedQuery<IReadOnlyCollection<BowlerTitleDto>> cachedQuery = query;
        TimeSpan expiry = cachedQuery.Expiry;

        // Assert
        expiry.ShouldBe(TimeSpan.FromDays(7));
    }

    [Fact]
    public void Query_CacheTags_ShouldIncludeAllBowlersHierarchy()
    {
        // Arrange
        var query = new ListBowlerTitlesQuery();

        // Act
        IReadOnlyCollection<string> tags = query.Tags;

        // Assert
        tags.ShouldBeEquivalentTo(ExpectedAllBowlersTags);
    }
}
