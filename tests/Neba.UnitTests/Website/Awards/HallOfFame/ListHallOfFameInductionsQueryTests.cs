using Neba.Application.Messaging;
using Neba.Website.Application.Awards.HallOfFame;

namespace Neba.UnitTests.Website.Awards.HallOfFame;
[Trait("Category", "Unit")]
[Trait("Component", "Website.Awards.HallOfFame")]

public sealed class ListHallOfFameInductionsQueryTests
{
    private static readonly string[] ExpectedAwardTags = ["website", "website:awards", "website:award:hall-of-fame"];

    [Fact(DisplayName = "Query implements ICachedQuery interface")]
    public void Query_ShouldImplementICachedQuery()
    {
        // Arrange & Act
        var query = new ListHallOfFameInductionsQuery();

        // Assert
        query.ShouldBeAssignableTo<ICachedQuery<IReadOnlyCollection<HallOfFameInductionDto>>>();
    }

    [Fact(DisplayName = "Query cache key follows naming convention")]
    public void Query_CacheKey_ShouldFollowConvention()
    {
        // Arrange
        var query = new ListHallOfFameInductionsQuery();

        // Act
        string key = query.Key;

        // Assert
        key.ShouldBe("website:awards:hall-of-fame-inductions");
        key.ShouldSatisfyAllConditions(
            k => k.ShouldNotBeNullOrWhiteSpace(),
            k => k.Length.ShouldBeLessThanOrEqualTo(512),
            k => k.Split(':').Length.ShouldBeGreaterThanOrEqualTo(3),
            k => k.Split(':').ShouldAllBe(p => !string.IsNullOrWhiteSpace(p))
        );
        key.Split(':')[0].ShouldBe("website");
        key.Split(':')[1].ShouldBe("awards");
        key.Split(':')[2].ShouldBe("hall-of-fame-inductions");
        key.Split(':').Length.ShouldBe(3);
    }

    [Fact(DisplayName = "Query cache expiry is 65 days")]
    public void Query_CacheExpiry_ShouldBe65Days()
    {
        // Arrange
        var query = new ListHallOfFameInductionsQuery();

        // Act
        TimeSpan expiry = query.Expiry;

        // Assert
        expiry.ShouldBe(TimeSpan.FromDays(65));
    }

    [Fact(DisplayName = "Query cache tags include award hierarchy")]
    public void Query_CacheTags_ShouldIncludeAwardHierarchy()
    {
        // Arrange
        var query = new ListHallOfFameInductionsQuery();

        // Act
        IReadOnlyCollection<string> tags = query.Tags;

        // Assert
        tags.ShouldBeEquivalentTo(ExpectedAwardTags);
    }
}
