using Neba.Application.Messaging;
using Neba.Website.Application.Awards.HighBlock;

namespace Neba.UnitTests.Website.Awards.HighBlock;
[Trait("Category", "Unit")]
[Trait("Component", "Website.Awards.HighBlock")]

public sealed class ListHigh5GameBlockAwardsQueryTests
{
    private static readonly string[] ExpectedAwardTags = ["website", "website:awards", "website:award:high-block"];

    [Fact(DisplayName = "Query implements ICachedQuery interface")]
    public void Query_ShouldImplementICachedQuery()
    {
        // Arrange & Act
        var query = new ListHigh5GameBlockAwardsQuery();

        // Assert
        query.ShouldBeAssignableTo<ICachedQuery<IReadOnlyCollection<HighBlockAwardDto>>>();
    }

    [Fact(DisplayName = "Query cache key follows naming convention")]
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

    [Fact(DisplayName = "Query cache expiry is 7 days")]
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

    [Fact(DisplayName = "Query cache tags include award hierarchy")]
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
