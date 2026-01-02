using Neba.Application.Caching;

namespace Neba.UnitTests.Caching;

[Trait("Category", "Unit")]
[Trait("Component", "Caching")]

public class CacheKeyTests
{
    [Theory(DisplayName = "Validates cache key format correctly")]
    [InlineData("website:doc:bylaws:content", true, TestDisplayName = "Valid key with doc type")]
    [InlineData("website:query:GetBowlerQuery:01ARZ3NDEK", true, TestDisplayName = "Valid key with query type")]
    [InlineData("api:job:import-scores:current", true, TestDisplayName = "Valid key with api context")]
    [InlineData("shared:session:user-prefs", true, TestDisplayName = "Valid key with shared context")]
    [InlineData("invalid", false, TestDisplayName = "Invalid single word")]
    [InlineData("too:short", false, TestDisplayName = "Invalid too short")]
    [InlineData("", false, TestDisplayName = "Invalid empty string")]
    [InlineData("   ", false, TestDisplayName = "Invalid whitespace only")]
    [InlineData("has::empty::parts", false, TestDisplayName = "Invalid empty parts")]
    public void IsValidCacheKey_ValidatesCorrectly(string key, bool expected)
    {
        // Act
        bool result = key.IsValidCacheKey();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact(DisplayName = "IsValidCacheKey rejects keys over 512 characters")]
    public void IsValidCacheKey_RejectsLongKeys()
    {
        // Arrange
        string longKey = $"website:query:GetData:{new string('x', 500)}";

        // Act
        bool result = longKey.IsValidCacheKey();

        // Assert
        result.ShouldBeFalse();
        longKey.Length.ShouldBeGreaterThan(512);
    }

    [Fact(DisplayName = "DocumentContentKey follows ADR-002 convention")]
    public void DocumentContentKey_FollowsConvention()
    {
        // Act
        string key = CacheKeys.Documents.Content("bylaws");

        // Assert
        key.ShouldBe("website:doc:bylaws:content");
        key.IsValidCacheKey().ShouldBeTrue();
        key.GetContext().ShouldBe("website");
        key.GetCacheType().ShouldBe("doc");
        key.GetIdentifier().ShouldBe("bylaws");
    }

    [Fact(DisplayName = "DocumentMetadataKey follows ADR-002 convention")]
    public void DocumentMetadataKey_FollowsConvention()
    {
        // Act
        string key = CacheKeys.Documents.Metadata("tournament-rules");

        // Assert
        key.ShouldBe("website:doc:tournament-rules:metadata");
        key.IsValidCacheKey().ShouldBeTrue();
        key.GetContext().ShouldBe("website");
        key.GetCacheType().ShouldBe("doc");
        key.GetIdentifier().ShouldBe("tournament-rules");
    }

    [Fact(DisplayName = "DocumentJobStateKey follows ADR-002 convention")]
    public void DocumentJobStateKey_FollowsConvention()
    {
        // Act
        string key = CacheKeys.Documents.JobState("bylaws");

        // Assert
        key.ShouldBe("website:job:doc-sync:bylaws:current");
        key.IsValidCacheKey().ShouldBeTrue();
        key.GetContext().ShouldBe("website");
        key.GetCacheType().ShouldBe("job");
        key.GetIdentifier().ShouldBe("doc-sync");
    }

    [Fact(DisplayName = "AwardsListBowlerOfTheYearKey follows ADR-002 convention")]
    public void AwardsListBowlerOfTheYearKey_FollowsConvention()
    {
        // Act
        string key = CacheKeys.Awards.BowlerOfTheYear();

        // Assert
        key.ShouldBe("website:awards:bowler-of-the-year");
        key.IsValidCacheKey().ShouldBeTrue();
        key.GetContext().ShouldBe("website");
        key.GetCacheType().ShouldBe("awards");
        key.GetIdentifier().ShouldBe("bowler-of-the-year");
    }

    [Fact(DisplayName = "QueryBuild creates valid key without parameters")]
    public void QueryBuild_WithoutParameters_CreatesValidKey()
    {
        // Act
        string key = CacheKeys.Queries.Build("GetBylawsQuery");

        // Assert
        key.ShouldBe("website:query:GetBylawsQuery");
        key.IsValidCacheKey().ShouldBeTrue();
    }

    [Fact(DisplayName = "QueryBuild creates valid key with single parameter")]
    public void QueryBuild_WithSingleParameter_CreatesValidKey()
    {
        // Act
        string key = CacheKeys.Queries.Build("GetBowlerQuery", "01ARZ3NDEKTSV4RRFFQ69G5FAV");

        // Assert
        key.ShouldBe("website:query:GetBowlerQuery:01ARZ3NDEKTSV4RRFFQ69G5FAV");
        key.IsValidCacheKey().ShouldBeTrue();
    }

    [Fact(DisplayName = "QueryBuild creates valid key with multiple parameters")]
    public void QueryBuild_WithMultipleParameters_CreatesValidKey()
    {
        // Act
        string key = CacheKeys.Queries.Build("GetSeasonStandingsQuery", "2024", "01ARZ3NDEK");

        // Assert
        key.ShouldBe("website:query:GetSeasonStandingsQuery:2024:01ARZ3NDEK");
        key.IsValidCacheKey().ShouldBeTrue();
    }

    [Fact(DisplayName = "GetContext extracts correct context from key")]
    public void GetContext_ExtractsCorrectContext()
    {
        // Arrange
        const string key = "website:doc:bylaws:content";

        // Act
        string context = key.GetContext();

        // Assert
        context.ShouldBe("website");
    }

    [Fact(DisplayName = "GetContext returns empty string for invalid key")]
    public void GetContext_ReturnsEmptyForInvalidKey()
    {
        // Arrange
        const string key = "";

        // Act
        string context = key.GetContext();

        // Assert
        context.ShouldBeEmpty();
    }

    [Fact(DisplayName = "GetCacheType extracts correct type from key")]
    public void GetCacheType_ExtractsCorrectType()
    {
        // Arrange
        const string key = "website:doc:bylaws:content";

        // Act
        string type = key.GetCacheType();

        // Assert
        type.ShouldBe("doc");
    }

    [Fact(DisplayName = "GetCacheType returns empty string for key with one part")]
    public void GetCacheType_ReturnsEmptyForShortKey()
    {
        // Arrange
        const string key = "website";

        // Act
        string type = key.GetCacheType();

        // Assert
        type.ShouldBeEmpty();
    }

    [Fact(DisplayName = "GetIdentifier extracts correct identifier from key")]
    public void GetIdentifier_ExtractsCorrectIdentifier()
    {
        // Arrange
        const string key = "website:doc:bylaws:content";

        // Act
        string identifier = key.GetIdentifier();

        // Assert
        identifier.ShouldBe("bylaws");
    }

    [Fact(DisplayName = "GetIdentifier returns empty string for key with two parts")]
    public void GetIdentifier_ReturnsEmptyForShortKey()
    {
        // Arrange
        const string key = "website:doc";

        // Act
        string identifier = key.GetIdentifier();

        // Assert
        identifier.ShouldBeEmpty();
    }

    [Theory(DisplayName = "All context constants produce valid keys")]
    [InlineData("website", TestDisplayName = "Website context is valid")]
    [InlineData("api", TestDisplayName = "API context is valid")]
    [InlineData("shared", TestDisplayName = "Shared context is valid")]
    public void CacheKeysConstants_AreValid(string context)
    {
        // Assert
        context.ShouldNotBeNullOrWhiteSpace();
        context.ShouldNotContain(':');
        context.All(ch => char.IsLower(ch) || ch == '-').ShouldBeTrue();
    }

    [Theory(DisplayName = "All type constants produce valid components")]
    [InlineData("doc", TestDisplayName = "Doc type is valid")]
    [InlineData("query", TestDisplayName = "Query type is valid")]
    [InlineData("job", TestDisplayName = "Job type is valid")]
    [InlineData("session", TestDisplayName = "Session type is valid")]
    [InlineData("awards", TestDisplayName = "Awards type is valid")]
    public void CacheKeysTypes_AreValid(string type)
    {
        // Assert
        type.ShouldNotBeNullOrWhiteSpace();
        type.ShouldNotContain(':');
        type.All(ch => char.IsLower(ch) || ch == '-').ShouldBeTrue();
    }
}
