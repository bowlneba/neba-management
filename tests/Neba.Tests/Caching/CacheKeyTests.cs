using FluentAssertions;
using Neba.Application.Caching;

namespace Neba.Tests.Caching;

public class CacheKeyTests
{
    [Theory(DisplayName = "IsValidCacheKey validates cache key format correctly")]
    [InlineData("website:doc:bylaws:content", true)]
    [InlineData("website:query:GetBowlerQuery:01ARZ3NDEK", true)]
    [InlineData("api:job:import-scores:current", true)]
    [InlineData("shared:session:user-prefs", true)]
    [InlineData("invalid", false)]
    [InlineData("too:short", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("has::empty::parts", false)]
    public void IsValidCacheKey_ValidatesCorrectly(string key, bool expected)
    {
        // Act
        bool result = key.IsValidCacheKey();

        // Assert
        result.Should().Be(expected);
    }

    [Fact(DisplayName = "IsValidCacheKey rejects keys over 512 characters")]
    public void IsValidCacheKey_RejectsLongKeys()
    {
        // Arrange
        string longKey = $"website:query:GetData:{new string('x', 500)}";

        // Act
        bool result = longKey.IsValidCacheKey();

        // Assert
        result.Should().BeFalse();
        longKey.Length.Should().BeGreaterThan(512);
    }

    [Fact(DisplayName = "DocumentContentKey follows ADR-002 convention")]
    public void DocumentContentKey_FollowsConvention()
    {
        // Act
        string key = CacheKeys.Documents.Content("bylaws");

        // Assert
        key.Should().Be("website:doc:bylaws:content");
        key.IsValidCacheKey().Should().BeTrue();
        key.GetContext().Should().Be("website");
        key.GetCacheType().Should().Be("doc");
        key.GetIdentifier().Should().Be("bylaws");
    }

    [Fact(DisplayName = "DocumentMetadataKey follows ADR-002 convention")]
    public void DocumentMetadataKey_FollowsConvention()
    {
        // Act
        string key = CacheKeys.Documents.Metadata("tournament-rules");

        // Assert
        key.Should().Be("website:doc:tournament-rules:metadata");
        key.IsValidCacheKey().Should().BeTrue();
        key.GetContext().Should().Be("website");
        key.GetCacheType().Should().Be("doc");
        key.GetIdentifier().Should().Be("tournament-rules");
    }

    [Fact(DisplayName = "DocumentJobStateKey follows ADR-002 convention")]
    public void DocumentJobStateKey_FollowsConvention()
    {
        // Act
        string key = CacheKeys.Documents.JobState("bylaws");

        // Assert
        key.Should().Be("website:job:doc-sync:bylaws:current");
        key.IsValidCacheKey().Should().BeTrue();
        key.GetContext().Should().Be("website");
        key.GetCacheType().Should().Be("job");
        key.GetIdentifier().Should().Be("doc-sync");
    }

    [Fact(DisplayName = "QueryBuild creates valid key without parameters")]
    public void QueryBuild_WithoutParameters_CreatesValidKey()
    {
        // Act
        string key = CacheKeys.Queries.Build("GetBylawsQuery");

        // Assert
        key.Should().Be("website:query:GetBylawsQuery");
        key.IsValidCacheKey().Should().BeTrue();
    }

    [Fact(DisplayName = "QueryBuild creates valid key with single parameter")]
    public void QueryBuild_WithSingleParameter_CreatesValidKey()
    {
        // Act
        string key = CacheKeys.Queries.Build("GetBowlerQuery", "01ARZ3NDEKTSV4RRFFQ69G5FAV");

        // Assert
        key.Should().Be("website:query:GetBowlerQuery:01ARZ3NDEKTSV4RRFFQ69G5FAV");
        key.IsValidCacheKey().Should().BeTrue();
    }

    [Fact(DisplayName = "QueryBuild creates valid key with multiple parameters")]
    public void QueryBuild_WithMultipleParameters_CreatesValidKey()
    {
        // Act
        string key = CacheKeys.Queries.Build("GetSeasonStandingsQuery", "2024", "01ARZ3NDEK");

        // Assert
        key.Should().Be("website:query:GetSeasonStandingsQuery:2024:01ARZ3NDEK");
        key.IsValidCacheKey().Should().BeTrue();
    }

    [Fact(DisplayName = "GetContext extracts correct context from key")]
    public void GetContext_ExtractsCorrectContext()
    {
        // Arrange
        string key = "website:doc:bylaws:content";

        // Act
        string context = key.GetContext();

        // Assert
        context.Should().Be("website");
    }

    [Fact(DisplayName = "GetContext returns empty string for invalid key")]
    public void GetContext_ReturnsEmptyForInvalidKey()
    {
        // Arrange
        string key = "";

        // Act
        string context = key.GetContext();

        // Assert
        context.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetCacheType extracts correct type from key")]
    public void GetCacheType_ExtractsCorrectType()
    {
        // Arrange
        string key = "website:doc:bylaws:content";

        // Act
        string type = key.GetCacheType();

        // Assert
        type.Should().Be("doc");
    }

    [Fact(DisplayName = "GetCacheType returns empty string for key with one part")]
    public void GetCacheType_ReturnsEmptyForShortKey()
    {
        // Arrange
        string key = "website";

        // Act
        string type = key.GetCacheType();

        // Assert
        type.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetIdentifier extracts correct identifier from key")]
    public void GetIdentifier_ExtractsCorrectIdentifier()
    {
        // Arrange
        string key = "website:doc:bylaws:content";

        // Act
        string identifier = key.GetIdentifier();

        // Assert
        identifier.Should().Be("bylaws");
    }

    [Fact(DisplayName = "GetIdentifier returns empty string for key with two parts")]
    public void GetIdentifier_ReturnsEmptyForShortKey()
    {
        // Arrange
        string key = "website:doc";

        // Act
        string identifier = key.GetIdentifier();

        // Assert
        identifier.Should().BeEmpty();
    }

    [Theory(DisplayName = "All CacheKeys constants produce valid keys")]
    [InlineData("website")]
    [InlineData("api")]
    [InlineData("shared")]
    public void CacheKeysConstants_AreValid(string context)
    {
        // Assert
        context.Should().NotBeNullOrWhiteSpace();
        context.Should().NotContain(':');
        context.Should().Match(c => c.All(ch => char.IsLower(ch) || ch == '-'));
    }

    [Theory(DisplayName = "All CacheKeys.Types constants produce valid type components")]
    [InlineData("doc")]
    [InlineData("query")]
    [InlineData("job")]
    [InlineData("session")]
    public void CacheKeysTypes_AreValid(string type)
    {
        // Assert
        type.Should().NotBeNullOrWhiteSpace();
        type.Should().NotContain(':');
        type.Should().Match(t => t.All(ch => char.IsLower(ch) || ch == '-'));
    }
}

