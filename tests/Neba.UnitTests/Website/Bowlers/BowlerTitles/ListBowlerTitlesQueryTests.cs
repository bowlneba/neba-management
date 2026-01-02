using Neba.Application.Messaging;
using Neba.Website.Application.Bowlers.BowlerTitles;

namespace Neba.UnitTests.Website.Bowlers.BowlerTitles;
[Trait("Category", "Unit")]
[Trait("Component", "Website.Bowlers.BowlerTitles")]

public sealed class ListBowlerTitlesQueryTests
{
    private static readonly string[] ExpectedAllBowlersTags = ["website", "website:bowlers"];

    [Fact(DisplayName = "Query implements ICachedQuery interface")]
    public void Query_ShouldImplementICachedQuery()
    {
        // Arrange & Act
        var query = new ListBowlerTitlesQuery();

        // Assert
        query.ShouldBeAssignableTo<ICachedQuery<IReadOnlyCollection<BowlerTitleDto>>>();
    }

    [Fact(DisplayName = "Query cache key follows naming convention")]
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

    [Fact(DisplayName = "Query cache expiry is 7 days")]
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

    [Fact(DisplayName = "Query cache tags include all bowlers hierarchy")]
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
