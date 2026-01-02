using Neba.Application.Documents;
using Neba.Application.Messaging;
using Neba.Website.Application.Tournaments.TournamentRules;

namespace Neba.UnitTests.Website.Tournaments.TournamentRules;

public sealed class GetTournamentRulesQueryTests
{
    private static readonly string[] ExpectedDocumentTags = ["website", "website:documents", "website:document:tournament-rules"];

    [Fact(DisplayName = "Query implements ICachedQuery interface")]
    public void Query_ShouldImplementICachedQuery()
    {
        // Arrange & Act
        var query = new GetTournamentRulesQuery();

        // Assert
        query.ShouldBeAssignableTo<ICachedQuery<DocumentDto>>();
    }

    [Fact(DisplayName = "Query cache key follows naming convention")]
    public void Query_CacheKey_ShouldFollowConvention()
    {
        // Arrange
        var query = new GetTournamentRulesQuery();

        // Act
        string key = query.Key;

        // Assert
        key.ShouldBe("website:doc:tournament-rules:content");
        key.ShouldSatisfyAllConditions(
            k => k.ShouldNotBeNullOrWhiteSpace(),
            k => k.Length.ShouldBeLessThanOrEqualTo(512),
            k => k.Split(':').Length.ShouldBeGreaterThanOrEqualTo(3),
            k => k.Split(':').ShouldAllBe(p => !string.IsNullOrWhiteSpace(p))
        );
        key.Split(':')[0].ShouldBe("website");
        key.Split(':')[1].ShouldBe("doc");
        key.Split(':')[2].ShouldBe("tournament-rules");
        key.Split(':')[3].ShouldBe("content");
    }

    [Fact(DisplayName = "Query cache expiry is 30 days")]
    public void Query_CacheExpiry_ShouldBe30Days()
    {
        // Arrange
        var query = new GetTournamentRulesQuery();

        // Act
        TimeSpan expiry = query.Expiry;

        // Assert
        expiry.ShouldBe(TimeSpan.FromDays(30));
    }

    [Fact(DisplayName = "Query cache tags include document hierarchy")]
    public void Query_CacheTags_ShouldIncludeDocumentHierarchy()
    {
        // Arrange
        var query = new GetTournamentRulesQuery();

        // Act
        IReadOnlyCollection<string> tags = query.Tags;

        // Assert
        tags.ShouldBeEquivalentTo(ExpectedDocumentTags);
    }
}
