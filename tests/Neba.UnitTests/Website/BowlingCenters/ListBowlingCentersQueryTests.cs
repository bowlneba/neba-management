using Neba.Application.Messaging;
using Neba.Website.Application.BowlingCenters;

namespace Neba.UnitTests.Website.BowlingCenters;
[Trait("Category", "Unit")]
[Trait("Component", "Website.BowlingCenters")]

public sealed class ListBowlingCentersQueryTests
{
    private static readonly string[] ExpectedBowlingCenterTags = ["website", "website:bowling-centers"];

    [Fact(DisplayName = "Query implements ICachedQuery interface")]
    public void Query_ShouldImplementICachedQuery()
    {
        // Arrange & Act
        var query = new ListBowlingCentersQuery();

        // Assert
        query.ShouldBeAssignableTo<ICachedQuery<IReadOnlyCollection<BowlingCenterDto>>>();
    }

    [Fact(DisplayName = "Query cache key follows naming convention")]
    public void Query_CacheKey_ShouldFollowConvention()
    {
        // Arrange
        var query = new ListBowlingCentersQuery();

        // Act
        string key = query.Key;

        // Assert
        key.ShouldBe("website:query:ListBowlingCentersQuery");
        key.ShouldSatisfyAllConditions(
            k => k.ShouldNotBeNullOrWhiteSpace(),
            k => k.Length.ShouldBeLessThanOrEqualTo(512),
            k => k.Split(':').Length.ShouldBeGreaterThanOrEqualTo(3),
            k => k.Split(':').ShouldAllBe(p => !string.IsNullOrWhiteSpace(p))
        );
        key.Split(':')[0].ShouldBe("website");
        key.Split(':')[1].ShouldBe("query");
        key.Split(':')[2].ShouldBe("ListBowlingCentersQuery");
        key.Split(':').Length.ShouldBe(3);
    }

    [Fact(DisplayName = "Query cache expiry is 7 days")]
    public void Query_CacheExpiry_ShouldBeDefault7Days()
    {
        // Arrange
        var query = new ListBowlingCentersQuery();

        // Act & Assert
        ICachedQuery<IReadOnlyCollection<BowlingCenterDto>> cachedQuery = query;
        TimeSpan expiry = cachedQuery.Expiry;

        // Assert
        expiry.ShouldBe(TimeSpan.FromDays(7));
    }

    [Fact(DisplayName = "Query cache tags include bowling centers hierarchy")]
    public void Query_CacheTags_ShouldIncludeBowlingCentersHierarchy()
    {
        // Arrange
        var query = new ListBowlingCentersQuery();

        // Act
        IReadOnlyCollection<string> tags = query.Tags;

        // Assert
        tags.ShouldBeEquivalentTo(ExpectedBowlingCenterTags);
    }
}
