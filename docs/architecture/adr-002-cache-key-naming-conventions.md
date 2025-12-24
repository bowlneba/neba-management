---
layout: default
title: ADR-002 - Cache Key Naming Conventions
---

# ADR-002: Cache Key Naming Conventions

**Status**: Proposed

**Date**: 2024-12-24

**Context**: Caching Strategy Standardization

---

## Context and Problem Statement

The NEBA Management application uses Microsoft's HybridCache for distributed caching across several use cases:

1. **Document Content**: Caching HTML documents (bylaws, tournament rules)
2. **Background Job State**: Tracking async job progress for real-time status updates
3. **Query Results**: Future implementation via `ICachedQuery<TResponse>` interface
4. **Hub Groups**: SignalR/SSE group identifiers for broadcasting updates

Currently, cache keys are created inconsistently:
- Document content: `"bylaws"`, `"tournament-rules"`
- Job state: `"bylaws:refresh:current"`, `"tournament-rules:refresh:current"`
- Hub groups: `"bylaws-refresh"`, `"tournament-rules-refresh"`

This inconsistency creates several problems:

1. **Collision Risk**: Different cache types could accidentally use the same key
2. **Poor Organization**: No clear hierarchy makes invalidation difficult
3. **Limited Scalability**: No namespace for bounded contexts or multi-tenancy
4. **Unclear Intent**: Keys don't indicate what type of data they contain
5. **Difficult Debugging**: Can't easily identify cache keys by type or purpose

### Technical Constraints

- HybridCache configured with `MaximumKeyLength = 512` characters
- Current modular monolith architecture with bounded contexts (Website, Core API)
- Potential future multi-tenancy requirements
- Need to support tag-based cache invalidation
- Must maintain backward compatibility during migration

## Decision

We will adopt a **Hierarchical Colon-Delimited Cache Key Pattern** with the following structure:

### Primary Pattern

```
{context}:{type}:{identifier}[:{subtype}][:{qualifier}]
```

**Components:**

1. **context** (required): Bounded context or application area
   - Examples: `website`, `api`, `shared`
   - Indicates which part of the system owns this cache entry

2. **type** (required): Cache entry type
   - Examples: `doc`, `query`, `job`, `session`
   - Indicates what kind of data is cached

3. **identifier** (required): Unique identifier for the cached item
   - Examples: `bylaws`, `tournament-rules`, `GetBowlerQuery`
   - The primary key or name of the cached entity

4. **subtype** (optional): Additional categorization
   - Examples: `state`, `content`, `metadata`
   - Used when multiple cache entries exist for the same identifier

5. **qualifier** (optional): Temporal or contextual qualifier
   - Examples: `current`, `v2`, `2024-Q1`
   - Used for versioning or time-based segmentation

### Standard Key Formats by Use Case

#### Document Content Caching
```csharp
// Document HTML content
"website:doc:bylaws:content"
"website:doc:tournament-rules:content"

// Document metadata
"website:doc:bylaws:metadata"
```

#### Background Job State Tracking
```csharp
// Job state for a specific document refresh
"website:job:doc-sync:bylaws:current"
"website:job:doc-sync:tournament-rules:current"

// Generic job state pattern
"{context}:job:{job-type}:{target}:current"
```

#### Query Result Caching
```csharp
// Query results (ICachedQuery implementations)
"website:query:GetBowlerQuery:01JGQM9BXYZ123456789ABCDEF"
"website:query:GetSeasonStandingsQuery:2024:01JGQM9C"

// Pattern for queries with parameters
"{context}:query:{QueryClassName}:{param1}:{param2}"
```

#### SignalR/SSE Hub Groups
```csharp
// Hub group identifiers (not technically cache keys, but related)
"hub:doc-refresh:bylaws"
"hub:doc-refresh:tournament-rules"

// Pattern for hub groups
"hub:{event-type}:{target}"
```

#### Multi-Tenant Keys (Future)
```csharp
// When multi-tenancy is added
"tenant:{tenantId}:website:doc:bylaws:content"
"tenant:{tenantId}:api:query:GetLeagueQuery:{leagueId}"

// Pattern
"tenant:{tenantId}:{context}:{type}:{identifier}..."
```

### Tag Strategy

Tags should follow a flat, simple taxonomy for bulk invalidation:

```csharp
// Tag examples
["website", "documents", "bylaws"]
["website", "queries", "bowlers"]
["api", "tournaments", "standings"]

// Tag pattern: one tag per hierarchical level
["{context}", "{type-category}", "{specific-entity}"]
```

**Usage:**
```csharp
public class GetBylawsQuery : ICachedQuery<DocumentDto>
{
    public string Key => "website:query:GetBylawsQuery";

    public IReadOnlyCollection<string> Tags =>
        new[] { "website", "documents", "bylaws" };
}
```

### Implementation Guidelines

#### 1. Use Constants for Key Components

```csharp
// Good: Centralized constants
public static class CacheKeys
{
    public const string WebsiteContext = "website";
    public const string ApiContext = "api";

    public static class Types
    {
        public const string Document = "doc";
        public const string Query = "query";
        public const string Job = "job";
    }

    public static class Documents
    {
        public const string Bylaws = "bylaws";
        public const string TournamentRules = "tournament-rules";

        public static string Content(string documentKey)
            => $"{WebsiteContext}:{Types.Document}:{documentKey}:content";

        public static string JobState(string documentKey)
            => $"{WebsiteContext}:{Types.Job}:doc-sync:{documentKey}:current";
    }
}

// Usage
string key = CacheKeys.Documents.Content("bylaws");
// Result: "website:doc:bylaws:content"
```

#### 2. ICachedQuery Implementation

```csharp
public sealed record GetBylawsQuery : ICachedQuery<DocumentDto>
{
    public string Key => CacheKeys.Documents.Content(BylawsConstants.DocumentKey);

    public TimeSpan Expiry => TimeSpan.FromDays(7);

    public IReadOnlyCollection<string> Tags =>
        new[] { CacheKeys.WebsiteContext, "documents", "bylaws" };
}

// For parameterized queries
public sealed record GetBowlerQuery(BowlerId BowlerId) : ICachedQuery<BowlerDto>
{
    public string Key => $"{CacheKeys.WebsiteContext}:{CacheKeys.Types.Query}:GetBowlerQuery:{BowlerId}";

    public IReadOnlyCollection<string> Tags =>
        new[] { CacheKeys.WebsiteContext, "bowlers", BowlerId.ToString() };
}
```

#### 3. Helper Extension Methods

```csharp
public static class CacheKeyExtensions
{
    /// <summary>
    /// Validates that a cache key follows the required naming convention.
    /// </summary>
    public static bool IsValidCacheKey(this string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 512)
            return false;

        // Must have at least 3 parts: context:type:identifier
        string[] parts = key.Split(':');
        return parts.Length >= 3 && parts.All(p => !string.IsNullOrWhiteSpace(p));
    }

    /// <summary>
    /// Extracts the context from a cache key.
    /// </summary>
    public static string GetContext(this string key)
    {
        string[] parts = key.Split(':');
        return parts.Length > 0 ? parts[0] : string.Empty;
    }

    /// <summary>
    /// Extracts the type from a cache key.
    /// </summary>
    public static string GetCacheType(this string key)
    {
        string[] parts = key.Split(':');
        return parts.Length > 1 ? parts[1] : string.Empty;
    }
}
```

## Rationale

### Why Hierarchical Colon-Delimited Pattern?

1. **Industry Standard**: Widely used in Redis, Memcached, and other caching systems
2. **Human Readable**: Easy to understand and debug in cache inspection tools
3. **Hierarchical**: Natural tree structure for invalidation and organization
4. **Tool Support**: Many cache visualization tools parse colon-delimited keys
5. **Efficient**: More compact than URLs (`/`) or dots (`.`) which can be percent-encoded

### Why Not Alternatives?

**URL-Style Paths** (`website/doc/bylaws/content`):
- ❌ Slashes may require escaping in some contexts
- ❌ Less common in caching systems
- ❌ Longer than colons

**Dot-Delimited** (`website.doc.bylaws.content`):
- ❌ Dots have special meaning in some systems (e.g., DNS)
- ❌ Can be confused with class/namespace notation
- ❌ Less visually distinct than colons

**Underscore/Dash Only** (`website_doc_bylaws_content`):
- ❌ No clear hierarchy without delimiters
- ❌ Difficult to parse programmatically
- ❌ Harder to read and debug

**Prefixed Flat Keys** (`neba-doc-bylaws-content`):
- ❌ No hierarchical structure for selective invalidation
- ❌ Application prefix wastes space in single-app deployment
- ❌ Doesn't scale to multi-tenancy

### Performance Considerations

- **Key Length**: Average key length ~40-60 characters, well under 512 limit
- **Parsing Cost**: String splits are negligible compared to cache I/O
- **Tag Efficiency**: Flat tag arrays enable efficient bulk operations
- **Memory**: Colon delimiters add minimal overhead

### Security Considerations

- **No Sensitive Data**: Keys should never contain PII or sensitive information
- **Predictable Structure**: Consistent format prevents injection attacks
- **Validation**: Helper methods enforce format compliance
- **Tenant Isolation**: Future multi-tenant keys prevent cross-tenant access

## Consequences

### Positive

✅ **Consistency**: All cache keys follow the same predictable pattern
✅ **Debuggability**: Easy to identify cache entry purpose from the key
✅ **Scalability**: Built-in namespace for contexts and future multi-tenancy
✅ **Invalidation**: Hierarchical structure supports targeted cache clearing
✅ **Tooling**: Standard format enables automated validation and testing
✅ **Documentation**: Self-documenting keys reduce cognitive load
✅ **Migration**: Clear mapping from old keys to new format

### Negative

⚠️ **Migration Effort**: Existing cache keys must be updated
⚠️ **Breaking Change**: Old keys will not be found after migration
⚠️ **Learning Curve**: Team must adopt new conventions
⚠️ **Verbosity**: Keys are longer than current flat format

### Neutral

➖ **Tag Strategy**: Requires discipline to maintain consistent tagging
➖ **Helper Classes**: Need to maintain CacheKeys constants
➖ **No Versioning**: Explicit version numbers not included (can be added as qualifier if needed)

## Migration Plan

### Phase 1: Add New Keys (Backward Compatible)

```csharp
// Update BylawsConstants to support both formats
public static class BylawsConstants
{
    // Legacy keys (deprecated)
    [Obsolete("Use CacheKeys.Documents.Content instead")]
    public const string DocumentKey = "bylaws";

    // New format
    public static string ContentCacheKey =>
        CacheKeys.Documents.Content("bylaws");

    public static string JobStateCacheKey =>
        CacheKeys.Documents.JobState("bylaws");
}
```

### Phase 2: Dual-Write Pattern

```csharp
// Write to both old and new keys temporarily
await cache.SetAsync(legacyKey, content, options, cancellationToken);
await cache.SetAsync(newKey, content, options, cancellationToken);
```

### Phase 3: Read from New Keys

```csharp
// Switch all reads to new key format
var content = await cache.GetOrCreateAsync(
    newKey,
    factory,
    options,
    tags,
    cancellationToken);
```

### Phase 4: Remove Legacy Keys

```csharp
// Stop writing to old keys, remove legacy constants
// Clean up deprecated code after full migration
```

## Implementation Notes

### 1. CacheKeys Constants Class

Create `src/backend/Neba.Application/Caching/CacheKeys.cs`:

```csharp
namespace Neba.Application.Caching;

/// <summary>
/// Centralized cache key definitions following ADR-002 naming conventions.
/// </summary>
/// <remarks>
/// Cache keys follow the pattern: {context}:{type}:{identifier}[:{subtype}][:{qualifier}]
/// See: docs/architecture/adr-002-cache-key-naming-conventions.md
/// </remarks>
public static class CacheKeys
{
    public const string WebsiteContext = "website";
    public const string ApiContext = "api";
    public const string SharedContext = "shared";

    public static class Types
    {
        public const string Document = "doc";
        public const string Query = "query";
        public const string Job = "job";
        public const string Session = "session";
    }

    public static class Documents
    {
        public static string Content(string documentKey)
            => $"{WebsiteContext}:{Types.Document}:{documentKey}:content";

        public static string Metadata(string documentKey)
            => $"{WebsiteContext}:{Types.Document}:{documentKey}:metadata";

        public static string JobState(string documentKey)
            => $"{WebsiteContext}:{Types.Job}:doc-sync:{documentKey}:current";
    }

    public static class Queries
    {
        public static string Build(string queryName, params object[] parameters)
        {
            string baseKey = $"{WebsiteContext}:{Types.Query}:{queryName}";
            return parameters.Length > 0
                ? $"{baseKey}:{string.Join(':', parameters)}"
                : baseKey;
        }
    }
}
```

### 2. Validation Unit Tests

Create `tests/Neba.UnitTests/Caching/CacheKeyTests.cs`:

```csharp
namespace Neba.UnitTests.Caching;

public class CacheKeyTests
{
    [Theory]
    [InlineData("website:doc:bylaws:content", true)]
    [InlineData("website:query:GetBowlerQuery:01ARZ3NDEK", true)]
    [InlineData("invalid", false)]
    [InlineData("too:short", false)]
    [InlineData("", false)]
    public void IsValidCacheKey_ValidatesCorrectly(string key, bool expected)
    {
        // Act
        bool result = key.IsValidCacheKey();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void DocumentContentKey_FollowsConvention()
    {
        // Act
        string key = CacheKeys.Documents.Content("bylaws");

        // Assert
        key.Should().Be("website:doc:bylaws:content");
        key.IsValidCacheKey().Should().BeTrue();
        key.GetContext().Should().Be("website");
        key.GetCacheType().Should().Be("doc");
    }
}
```

### 3. PR Review Checklist

Add to code review guidelines or create `.github/PULL_REQUEST_TEMPLATE.md` section:

```markdown
### Cache Key Checklist

For PRs that add or modify cache keys, verify:

- [ ] Keys follow the `{context}:{type}:{identifier}[:{subtype}][:{qualifier}]` pattern
- [ ] Keys use `CacheKeys` constants, not hardcoded strings
- [ ] Keys are under 512 characters
- [ ] Keys have at least 3 parts (context:type:identifier)
- [ ] Tags follow flat taxonomy: `["{context}", "{category}", "{entity}"]`
- [ ] No PII or sensitive data in keys
- [ ] Cache expiry is explicitly set (not relying on defaults)
- [ ] Unit tests validate key format
- [ ] Documentation updated if new key type added
```

## PR Reviewer Enforcement Guide

### ✅ Good Examples

```csharp
// Good: Uses CacheKeys constants
string key = CacheKeys.Documents.Content("bylaws");

// Good: Follows pattern with helper method
string key = CacheKeys.Queries.Build("GetBowlerQuery", bowlerId);

// Good: Parameterized query key
public string Key => $"{CacheKeys.WebsiteContext}:{CacheKeys.Types.Query}:GetBowlerQuery:{BowlerId}";

// Good: Proper tag structure
public IReadOnlyCollection<string> Tags => new[] { "website", "documents", "bylaws" };
```

### ❌ Bad Examples - Reject These

```csharp
// Bad: Hardcoded string, no structure
string key = "bylaws";

// Bad: Doesn't follow pattern
string key = "bylaws-content";

// Bad: Missing context
string key = "doc:bylaws:content";

// Bad: Using dots instead of colons
string key = "website.doc.bylaws.content";

// Bad: Key too long (over 512 chars)
string key = $"website:query:GetVeryLongQuery:{string.Join(':', Enumerable.Range(1, 100))}";

// Bad: Contains PII
string key = $"website:user:{email}";  // Never put email in keys!

// Bad: No constants
await cache.SetAsync("some-cache-key", value, options);
```

### Review Questions to Ask

1. **Is the cache key structure documented?**
   - Can you tell from the key what type of data it contains?
   - Is it clear which bounded context owns this cache entry?

2. **Is the key format consistent?**
   - Does it follow the colon-delimited pattern?
   - Are all required components present?

3. **Is the key properly scoped?**
   - Does it include the bounded context?
   - Will it collide with other cache entries?

4. **Is the key testable?**
   - Are there unit tests validating the key format?
   - Does the test use `IsValidCacheKey()` helper?

5. **Are tags appropriate?**
   - Do tags enable useful bulk invalidation?
   - Are tags flat (not hierarchical)?

## Alternatives Considered

### Alternative 1: URL-Style Paths

```
Pattern: {context}/{type}/{identifier}/{subtype}/{qualifier}
Example: website/doc/bylaws/content
```

**Pros:**
- Familiar to web developers
- Natural hierarchy
- Readable

**Cons:**
- Slashes may need escaping
- Longer than colons
- Less common in caching systems
- Doesn't follow Redis conventions

**Decision:** ❌ Rejected - Less standard, potential escaping issues

### Alternative 2: Dot-Delimited Notation

```
Pattern: {context}.{type}.{identifier}.{subtype}.{qualifier}
Example: website.doc.bylaws.content
```

**Pros:**
- Similar to namespace notation
- Compact

**Cons:**
- Dots have special meaning in some systems
- Can be confused with class names
- Less visually distinct
- Not standard in caching systems

**Decision:** ❌ Rejected - Confusing notation, not industry standard

### Alternative 3: Prefixed Flat Keys

```
Pattern: {app-prefix}_{type}_{identifier}_{detail}
Example: neba_doc_bylaws_content
```

**Pros:**
- Simple to implement
- No parsing needed
- Very compact

**Cons:**
- No hierarchical structure
- Difficult to invalidate by pattern
- Doesn't scale to multi-tenancy
- Application prefix unnecessary in single-app deployment
- Hard to parse programmatically

**Decision:** ❌ Rejected - No hierarchy, poor scalability

### Alternative 4: Hybrid Dash-Colon Pattern

```
Pattern: {context}-{type}:{identifier}:{subtype}
Example: website-doc:bylaws:content
```

**Pros:**
- Visual separation between namespace and hierarchy
- Compact

**Cons:**
- Inconsistent delimiter usage
- More complex parsing rules
- Non-standard
- Team must learn two delimiter semantics

**Decision:** ❌ Rejected - Inconsistent, non-standard

### Alternative 5: Keep Current Inconsistent Pattern

```
Pattern: No standard pattern, case-by-case
Examples: "bylaws", "bylaws:refresh:current", "bylaws-refresh"
```

**Pros:**
- No migration needed
- Flexible

**Cons:**
- Collision risk
- Difficult to debug
- No organization
- Doesn't scale
- Poor developer experience

**Decision:** ❌ Rejected - This ADR exists to solve this problem

## Comparison Matrix

| Pattern | Hierarchy | Readability | Standard | Scalability | Parsing | Length |
|---------|-----------|-------------|----------|-------------|---------|--------|
| **Colon-Delimited** ✅ | ✅ Excellent | ✅ High | ✅ Redis | ✅ High | ✅ Simple | ✅ Short |
| URL-Style | ✅ Excellent | ✅ High | ⚠️ Web | ✅ High | ⚠️ Escaping | ⚠️ Longer |
| Dot-Delimited | ✅ Excellent | ⚠️ Confusing | ❌ None | ✅ High | ✅ Simple | ✅ Short |
| Prefixed Flat | ❌ None | ⚠️ Medium | ❌ None | ❌ Low | ⚠️ Complex | ✅ Short |
| Hybrid Dash-Colon | ⚠️ Complex | ⚠️ Medium | ❌ None | ⚠️ Medium | ⚠️ Complex | ✅ Short |
| Current (No Pattern) | ❌ None | ❌ Low | ❌ None | ❌ Low | ❌ Impossible | ✅ Varies |

## References

### Microsoft Documentation
- [HybridCache Documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid)
- [Distributed Caching in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
- [Response Caching Middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/middleware)

### Redis Best Practices
- [Redis Key Naming Conventions](https://redis.io/docs/manual/keyspace/)
- [Redis Key Design Patterns](https://redis.io/docs/manual/patterns/indexes/)
- [StackExchange.Redis Best Practices](https://stackexchange.github.io/StackExchange.Redis/KeysValues)

### Industry Patterns
- [AWS ElastiCache Best Practices](https://docs.aws.amazon.com/AmazonElastiCache/latest/red-ug/BestPractices.html)
- [Azure Cache for Redis Best Practices](https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-best-practices)
- [Memcached Key Naming](https://github.com/memcached/memcached/wiki/UserInternals#key-format)

### Related ADRs
- [ADR-001: ULID and Shadow Key Pattern](adr-001-ulid-shadow-keys.md) - Establishes identifier patterns used in cache keys

---

**Decision Owner**: Development Team

**Last Updated**: 2024-12-24

**Next Review**: After migration complete or 6 months, whichever comes first

