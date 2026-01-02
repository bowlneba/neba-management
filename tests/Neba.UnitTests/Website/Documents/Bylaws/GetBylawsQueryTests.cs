using Neba.Application.Documents;
using Neba.Application.Messaging;
using Neba.Website.Application.Documents.Bylaws;

namespace Neba.UnitTests.Website.Documents.Bylaws;
[Trait("Category", "Unit")]
[Trait("Component", "Website.Documents.Bylaws")]

public sealed class GetBylawsQueryTests
{
    private static readonly string[] ExpectedDocumentTags = ["website", "website:documents", "website:document:bylaws"];

    [Fact(DisplayName = "Query implements ICachedQuery interface")]
    public void Query_ShouldImplementICachedQuery()
    {
        // Arrange & Act
        var query = new GetBylawsQuery();

        // Assert
        query.ShouldBeAssignableTo<ICachedQuery<DocumentDto>>();
    }

    [Fact(DisplayName = "Query cache key follows naming convention")]
    public void Query_CacheKey_ShouldFollowConvention()
    {
        // Arrange
        var query = new GetBylawsQuery();

        // Act
        string key = query.Key;

        // Assert
        key.ShouldBe("website:doc:bylaws:content");
        key.ShouldSatisfyAllConditions(
            k => k.ShouldNotBeNullOrWhiteSpace(),
            k => k.Length.ShouldBeLessThanOrEqualTo(512),
            k => k.Split(':').Length.ShouldBeGreaterThanOrEqualTo(3),
            k => k.Split(':').ShouldAllBe(p => !string.IsNullOrWhiteSpace(p))
        );
        key.Split(':')[0].ShouldBe("website");
        key.Split(':')[1].ShouldBe("doc");
        key.Split(':')[2].ShouldBe("bylaws");
        key.Split(':')[3].ShouldBe("content");
    }

    [Fact(DisplayName = "Query cache expiry is 30 days")]
    public void Query_CacheExpiry_ShouldBe30Days()
    {
        // Arrange
        var query = new GetBylawsQuery();

        // Act
        TimeSpan expiry = query.Expiry;

        // Assert
        expiry.ShouldBe(TimeSpan.FromDays(30));
    }

    [Fact(DisplayName = "Query cache tags include document hierarchy")]
    public void Query_CacheTags_ShouldIncludeDocumentHierarchy()
    {
        // Arrange
        var query = new GetBylawsQuery();

        // Act
        IReadOnlyCollection<string> tags = query.Tags;

        // Assert
        tags.ShouldBeEquivalentTo(ExpectedDocumentTags);
    }
}
