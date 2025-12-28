using Neba.Application.Messaging;
using Neba.Tests.Website;
using Neba.Website.Application.Bowlers.BowlerTitles;

namespace Neba.UnitTests.Website.Bowlers.BowlerTitles;

public sealed class ListBowlerTitleSummariesQueryHandlerTests
{
    private static readonly string[] ExpectedAllBowlersTags = ["website", "website:bowlers"];

    private readonly Mock<IWebsiteTitleQueryRepository> _mockWebsiteTitleQueryRepository;

    private readonly ListBowlerTitleSummariesQueryHandler _queryHandler;

    public ListBowlerTitleSummariesQueryHandlerTests()
    {
        _mockWebsiteTitleQueryRepository = new Mock<IWebsiteTitleQueryRepository>(MockBehavior.Strict);

        _queryHandler = new ListBowlerTitleSummariesQueryHandler(
            _mockWebsiteTitleQueryRepository.Object);
    }

    [Fact(DisplayName = "Returns all bowlers titles summary")]
    public async Task HandleAsync_ShouldReturnAllBowlersTitlesSummary()
    {
        // Arrange
        IReadOnlyCollection<BowlerTitleSummaryDto> seedSummaries = BowlerTitleSummaryDtoFactory.Bogus(50);

        _mockWebsiteTitleQueryRepository
            .Setup(repository => repository.ListTitleSummariesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(seedSummaries);

        ListBowlerTitleSummariesQuery query = new();

        // Act
        IReadOnlyCollection<BowlerTitleSummaryDto> summaries = await _queryHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        summaries.ShouldBeEquivalentTo(seedSummaries);
    }

    [Fact(DisplayName = "Query implements ICachedQuery interface")]
    public void Query_ShouldImplementICachedQuery()
    {
        // Arrange & Act
        var query = new ListBowlerTitleSummariesQuery();

        // Assert
        query.ShouldBeAssignableTo<ICachedQuery<IReadOnlyCollection<BowlerTitleSummaryDto>>>();
    }

    [Fact(DisplayName = "Query cache key follows naming convention")]
    public void Query_CacheKey_ShouldFollowConvention()
    {
        // Arrange
        var query = new ListBowlerTitleSummariesQuery();

        // Act
        string key = query.Key;

        // Assert
        key.ShouldBe("website:query:ListBowlerTitleSummariesQuery");
        key.ShouldSatisfyAllConditions(
            k => k.ShouldNotBeNullOrWhiteSpace(),
            k => k.Length.ShouldBeLessThanOrEqualTo(512),
            k => k.Split(':').Length.ShouldBeGreaterThanOrEqualTo(3),
            k => k.Split(':').ShouldAllBe(p => !string.IsNullOrWhiteSpace(p))
        );
        key.Split(':')[0].ShouldBe("website");
        key.Split(':')[1].ShouldBe("query");
        key.Split(':')[2].ShouldBe("ListBowlerTitleSummariesQuery");
        key.Split(':').Length.ShouldBe(3);
    }

    [Fact(DisplayName = "Query cache expiry is 7 days")]
    public void Query_CacheExpiry_ShouldBeDefault7Days()
    {
        // Arrange
        var query = new ListBowlerTitleSummariesQuery();

        // Act & Assert
        ICachedQuery<IReadOnlyCollection<BowlerTitleSummaryDto>> cachedQuery = query;
        TimeSpan expiry = cachedQuery.Expiry;

        // Assert
        expiry.ShouldBe(TimeSpan.FromDays(7));
    }

    [Fact(DisplayName = "Query cache tags include all bowlers hierarchy")]
    public void Query_CacheTags_ShouldIncludeAllBowlersHierarchy()
    {
        // Arrange
        var query = new ListBowlerTitleSummariesQuery();

        // Act
        IReadOnlyCollection<string> tags = query.Tags;

        // Assert
        tags.ShouldBeEquivalentTo(ExpectedAllBowlersTags);
    }
}
