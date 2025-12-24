---
layout: default
title: ADR-003 - Cache Tagging Strategy for Invalidation
---

# ADR-003: Cache Tagging Strategy for Invalidation

**Status**: Accepted

**Date**: 2025-12-24

**Context**: Cache Invalidation Strategy

**Related**: [ADR-002: Cache Key Naming Conventions](adr-002-cache-key-naming-conventions.md)

---

## Context and Problem Statement

The NEBA Management application uses Microsoft's HybridCache with query result caching via `ICachedQuery<TResponse>`. While ADR-002 established consistent cache key naming conventions, we need a standardized approach to **cache invalidation** when entities change.

### Invalidation Scenarios

Consider these real-world scenarios:

1. **Tournament Results Update**: A tournament's standings are updated
   - Must invalidate: All bowler title queries, tournament standings, award calculations
   - Challenge: Multiple cached queries depend on tournament data

2. **Bowler Information Change**: A bowler's profile is edited
   - Must invalidate: Specific bowler queries, title lists, award standings
   - Challenge: Need to target specific bowler without clearing all bowler caches

3. **Document Refresh**: Tournament rules document is synced from SharePoint
   - Must invalidate: Document content cache
   - Challenge: Simple case, but must be consistent with other patterns

4. **Award Recalculation**: Season-end awards are processed
   - Must invalidate: All award listing queries (Bowler of Year, High Average, High Block)
   - Challenge: Multiple award types must be cleared atomically

### Current State

The `ICachedQuery<TResponse>` interface provides a `Tags` property:

```csharp
public interface ICachedQuery<TResponse> : IQuery<TResponse>
{
    string Key { get; }
    TimeSpan Expiry => TimeSpan.FromDays(7);
    IReadOnlyCollection<string> Tags => Array.Empty<string>();
}
```

However, queries currently use inconsistent or empty tags:

```csharp
// Current: Generic, inconsistent tags
public IReadOnlyCollection<string> Tags =>
    new[] { "documents", "tournament-rules" };

// Problem: No standardized format, no entity references
```

### Requirements

1. **Bulk Invalidation**: Clear all caches related to an entity (e.g., all bowler queries)
2. **Targeted Invalidation**: Clear specific entity cache (e.g., Bowler ID `01ARZ3NDEK`)
3. **Type-Based Invalidation**: Clear by entity type (e.g., all tournament-related caches)
4. **Context Isolation**: Support bounded context separation
5. **Hierarchical Clearing**: Support partial invalidation (e.g., all awards vs specific award type)
6. **Consistency**: Follow established naming patterns from ADR-002
7. **Performance**: Efficient tag lookups in distributed cache scenarios

### Technical Constraints

- HybridCache does not natively support tag-based invalidation
- Tags must be manually tracked and used for bulk `RemoveAsync()` calls
- Flat tag structure for efficiency (no nested hierarchies)
- Tags must be deterministic and reproducible from entity IDs

## Decision

We will adopt a **Standardized Flat Tag Taxonomy** with three tag levels for all cached queries:

### Tag Pattern

```
{context}:{category}[:{entity-id}]
```

**Tag Levels:**

1. **Context Tag**: Bounded context identifier
   - Format: `{context}` (e.g., `website`, `api`, `shared`)
   - Purpose: Clear all caches for a bounded context
   - Example: `website`

2. **Type Tag**: Entity or resource type
   - Format: `{context}:{category}` (e.g., `website:bowlers`, `website:tournaments`)
   - Purpose: Clear all caches for an entity type
   - Example: `website:bowlers`

3. **Entity Tag**: Specific entity instance
   - Format: `{context}:{category}:{entity-id}` (e.g., `website:bowler:01ARZ3NDEK`)
   - Purpose: Clear all caches related to a specific entity
   - Example: `website:bowler:01ARZ3NDEK123456789ABCDEF`

### Standard Tag Taxonomy

#### Document Tags

```csharp
// Context tag
"website"

// Type tag
"website:documents"

// Entity tag (document key)
"website:document:bylaws"
"website:document:tournament-rules"
```

#### Bowler Tags

```csharp
// Context tag
"website"

// Type tag
"website:bowlers"

// Entity tag (bowler ID)
"website:bowler:01ARZ3NDEKTSV4RRFFQ69G5FAV"
```

#### Tournament Tags

```csharp
// Context tag
"website"

// Type tag
"website:tournaments"

// Entity tag (tournament ID)
"website:tournament:01ARZ3NDEKTSV4RRFFQ69G5FAV"
```

#### Award Tags

```csharp
// Context tag
"website"

// Type tag
"website:awards"

// Entity tag (award type)
"website:award:bowler-of-the-year"
"website:award:high-average"
"website:award:high-block"
```

#### Job Tags

```csharp
// Context tag
"website"

// Type tag
"website:jobs"

// Entity tag (job type + target)
"website:job:doc-sync:bylaws"
```

### Implementation Standard

#### CacheTags Utility Class

```csharp
/// <summary>
/// Centralized cache tag definitions following ADR-003 tagging strategy.
/// </summary>
/// <remarks>
/// <para>
/// Tags follow a hierarchical pattern for bulk invalidation:
///   Level 1: {context}
///   Level 2: {context}:{category}
///   Level 3: {context}:{category}:{entity-id}
/// </para>
/// <para>
/// See: docs/architecture/adr-003-cache-tagging-strategy.md
/// </para>
/// </remarks>
public static class CacheTags
{
    /// <summary>
    /// Creates a complete tag set for a document cache entry.
    /// </summary>
    /// <param name="documentKey">Document identifier (e.g., "bylaws").</param>
    /// <returns>Array of tags: [context, type, entity]</returns>
    public static string[] Documents(string documentKey) =>
        new[]
        {
            CacheKeys.WebsiteContext,                                    // Level 1: website
            $"{CacheKeys.WebsiteContext}:documents",                     // Level 2: website:documents
            $"{CacheKeys.WebsiteContext}:document:{documentKey}"         // Level 3: website:document:bylaws
        };

    /// <summary>
    /// Creates a complete tag set for a bowler cache entry.
    /// </summary>
    public static string[] Bowler(BowlerId bowlerId) =>
        new[]
        {
            CacheKeys.WebsiteContext,                                    // Level 1: website
            $"{CacheKeys.WebsiteContext}:bowlers",                       // Level 2: website:bowlers
            $"{CacheKeys.WebsiteContext}:bowler:{bowlerId}"              // Level 3: website:bowler:01ARZ3NDEK...
        };

    /// <summary>
    /// Creates a complete tag set for all bowlers (list queries).
    /// </summary>
    public static string[] AllBowlers() =>
        new[]
        {
            CacheKeys.WebsiteContext,                                    // Level 1: website
            $"{CacheKeys.WebsiteContext}:bowlers"                        // Level 2: website:bowlers
        };

    /// <summary>
    /// Creates a complete tag set for a tournament cache entry.
    /// </summary>
    public static string[] Tournament(TournamentId tournamentId) =>
        new[]
        {
            CacheKeys.WebsiteContext,
            $"{CacheKeys.WebsiteContext}:tournaments",
            $"{CacheKeys.WebsiteContext}:tournament:{tournamentId}"
        };

    /// <summary>
    /// Creates a complete tag set for award cache entries.
    /// </summary>
    public static string[] Award(string awardType) =>
        new[]
        {
            CacheKeys.WebsiteContext,
            $"{CacheKeys.WebsiteContext}:awards",
            $"{CacheKeys.WebsiteContext}:award:{awardType}"
        };

    /// <summary>
    /// Creates a complete tag set for all awards.
    /// </summary>
    public static string[] AllAwards() =>
        new[]
        {
            CacheKeys.WebsiteContext,
            $"{CacheKeys.WebsiteContext}:awards"
        };

    /// <summary>
    /// Creates a complete tag set for job state cache entries.
    /// </summary>
    public static string[] Job(string jobType, string target) =>
        new[]
        {
            CacheKeys.WebsiteContext,
            $"{CacheKeys.WebsiteContext}:jobs",
            $"{CacheKeys.WebsiteContext}:job:{jobType}:{target}"
        };
}
```

#### Query Implementation Examples

```csharp
// Document query with proper tags
public sealed record GetBylawsQuery : ICachedQuery<DocumentDto>
{
    public string Key => CacheKeys.Documents.Content("bylaws");

    public TimeSpan Expiry => TimeSpan.FromDays(30);

    public IReadOnlyCollection<string> Tags => CacheTags.Documents("bylaws");
}

// Bowler-specific query with entity tag
public sealed record BowlerTitlesQuery : ICachedQuery<ErrorOr<BowlerTitlesDto>>
{
    public required BowlerId BowlerId { get; init; }

    public string Key => $"{CacheKeys.WebsiteContext}:{CacheKeys.Types.Query}:BowlerTitlesQuery:{BowlerId}";

    public TimeSpan Expiry => TimeSpan.FromDays(7);

    public IReadOnlyCollection<string> Tags => CacheTags.Bowler(BowlerId);
}

// List query with type-level tags
public sealed record ListBowlerOfTheYearAwardsQuery : ICachedQuery<IReadOnlyCollection<BowlerOfTheYearAwardDto>>
{
    public string Key => CacheKeys.Awards.BowlerOfTheYear();

    public TimeSpan Expiry => TimeSpan.FromDays(7);

    public IReadOnlyCollection<string> Tags => CacheTags.Award("bowler-of-the-year");
}
```

### Invalidation Patterns

#### Invalidate All Caches for an Entity Type

```csharp
// Clear all bowler-related caches when tournament results update
await cache.RemoveByTagAsync($"{CacheKeys.WebsiteContext}:bowlers", cancellationToken);

// This clears:
// - All BowlerTitlesQuery caches (specific bowlers)
// - All ListBowlerTitlesQuery caches
// - All ListBowlerTitleSummariesQuery caches
```

#### Invalidate Specific Entity Cache

```csharp
// Clear only caches related to a specific bowler
var bowlerId = new BowlerId("01ARZ3NDEKTSV4RRFFQ69G5FAV");
await cache.RemoveByTagAsync(
    $"{CacheKeys.WebsiteContext}:bowler:{bowlerId}",
    cancellationToken);

// This clears:
// - BowlerTitlesQuery for this bowler
// - Any other queries tagged with this bowler ID
```

#### Invalidate All Caches in a Context

```csharp
// Nuclear option: clear entire website context
await cache.RemoveByTagAsync(CacheKeys.WebsiteContext, cancellationToken);

// This clears:
// - All website-related queries
// - Documents, bowlers, tournaments, awards
```

#### Cascade Invalidation (Business Logic)

```csharp
// Example: Tournament results updated
public async Task HandleTournamentResultsUpdated(
    TournamentId tournamentId,
    CancellationToken cancellationToken)
{
    // Clear all tournament-related caches
    await cache.RemoveByTagAsync(
        $"{CacheKeys.WebsiteContext}:tournament:{tournamentId}",
        cancellationToken);

    // Clear all bowler caches (titles may have changed)
    await cache.RemoveByTagAsync(
        $"{CacheKeys.WebsiteContext}:bowlers",
        cancellationToken);

    // Clear all award caches (standings may have changed)
    await cache.RemoveByTagAsync(
        $"{CacheKeys.WebsiteContext}:awards",
        cancellationToken);
}
```

## Rationale

### Why Flat Tag Taxonomy?

**Flat Structure:**
```
✅ Good: ["website", "website:bowlers", "website:bowler:01ARZ3NDEK"]
```

**Hierarchical Structure (Rejected):**
```
❌ Rejected: {
    "context": "website",
    "category": "bowlers",
    "entity": "01ARZ3NDEK"
}
```

**Reasons:**

1. **Performance**: Flat arrays are faster to iterate and match
2. **Simplicity**: No nested object parsing required
3. **Deterministic**: Tags can be reconstructed from entity IDs
4. **Cache-Friendly**: Most caching systems expect flat tag lists
5. **Standard Practice**: Aligns with Redis tagging patterns

### Why Three Tag Levels?

**Level 1 (Context)**: Enables bounded context isolation
- Use case: Clear all website caches vs API caches
- Example: `website`

**Level 2 (Type)**: Enables entity type invalidation
- Use case: Clear all bowler queries when batch import completes
- Example: `website:bowlers`

**Level 3 (Entity)**: Enables targeted invalidation
- Use case: Clear specific bowler after profile edit
- Example: `website:bowler:01ARZ3NDEK123456789ABCDEF`

**More levels would be:**
- Too granular (performance cost)
- Harder to manage (cognitive load)
- Rarely needed (YAGNI)

### Why Colon-Delimited Tags?

Consistent with ADR-002 cache key conventions:
- Same delimiter as cache keys
- Easy to parse and validate
- Industry standard (Redis)
- Human-readable in logs

### Why Singular vs Plural?

**Type Tags** (Level 2): **Plural** for collections
- `website:bowlers` - "all queries about bowlers"
- `website:tournaments` - "all queries about tournaments"
- `website:documents` - "all queries about documents"

**Entity Tags** (Level 3): **Singular** for instances
- `website:bowler:01ARZ3NDEK` - "this specific bowler"
- `website:tournament:01ARZ3NDEK` - "this specific tournament"
- `website:document:bylaws` - "this specific document"

**Reasoning:**
- Matches REST API conventions (collection vs resource)
- Natural language clarity
- Consistent with ubiquitous language

## Consequences

### Positive

✅ **Targeted Invalidation**: Clear only affected caches, not everything
✅ **Cascade Control**: Business logic can orchestrate related invalidations
✅ **Performance**: Avoid unnecessary cache misses from over-invalidation
✅ **Debugging**: Tags in logs clearly show invalidation scope
✅ **Consistency**: Follows ADR-002 naming patterns
✅ **Testability**: Deterministic tags enable unit testing
✅ **Scalability**: Flat structure performs well at scale

### Negative

⚠️ **Manual Tracking**: HybridCache doesn't track tags natively (yet)
⚠️ **Implementation Effort**: All queries must implement proper tags
⚠️ **Tag Proliferation**: Many tags per cache entry (3+ tags)
⚠️ **Breaking Change**: Existing queries need tag updates

### Neutral

➖ **Tag Discipline**: Requires consistent tag application
➖ **Business Logic**: Invalidation logic must be in application layer
➖ **No Wildcards**: Cannot use patterns like `website:bowler:*`

## Alternatives Considered

### Alternative 1: Single Tag Per Query

```csharp
// One tag per query
public IReadOnlyCollection<string> Tags => new[] { "bowlers" };
```

**Pros:**
- Simple
- Less tag storage

**Cons:**
- Cannot target specific entities
- Cannot clear by context
- Loses hierarchical invalidation

**Decision**: ❌ Rejected - Too limited for real-world scenarios

### Alternative 2: Hierarchical Object Tags

```csharp
// Nested tag structure
public IReadOnlyCollection<CacheTag> Tags => new[]
{
    new CacheTag { Context = "website", Category = "bowlers", EntityId = "01ARZ3NDEK" }
};
```

**Pros:**
- Type-safe
- Structured

**Cons:**
- Complex to serialize/deserialize
- Not supported by HybridCache tags property
- Performance overhead
- Over-engineering

**Decision**: ❌ Rejected - Unnecessary complexity

### Alternative 3: Prefix-Based Tags

```csharp
// Prefix-based matching
public IReadOnlyCollection<string> Tags => new[] { "website*", "bowlers*", "bowler:01ARZ3NDEK" };
```

**Pros:**
- Supports wildcard matching

**Cons:**
- HybridCache doesn't support wildcards
- Requires custom implementation
- Less predictable behavior
- Non-standard

**Decision**: ❌ Rejected - Not supported by platform

### Alternative 4: Category-Only Tags

```csharp
// Just category tags
public IReadOnlyCollection<string> Tags => new[] { "bowlers", "titles" };
```

**Pros:**
- Simple
- Less storage

**Cons:**
- Cannot invalidate by context
- Cannot target specific entities
- Loses most invalidation benefits

**Decision**: ❌ Rejected - Insufficient granularity

### Alternative 5: Keep Empty Tags

```csharp
// No tags
public IReadOnlyCollection<string> Tags => Array.Empty<string>();
```

**Pros:**
- No effort required

**Cons:**
- Cannot do bulk invalidation
- Must clear entire cache or individual keys
- This ADR exists to solve this problem

**Decision**: ❌ Rejected - Defeats the purpose

## Migration Plan

### Phase 1: Add CacheTags Utility (Non-Breaking)

1. Create `CacheTags` class in `Neba.Application/Caching/`
2. Add unit tests for tag generation
3. Update documentation

### Phase 2: Update Existing Queries (Breaking)

1. Update all `ICachedQuery` implementations with proper tags
2. Prioritize high-traffic queries first:
   - Document queries (bylaws, tournament rules)
   - Award listing queries
   - Bowler title queries
3. Update related tests

### Phase 3: Implement Invalidation Logic (New Feature)

1. Add invalidation methods to command handlers
2. Implement cascade invalidation for complex scenarios
3. Add integration tests for invalidation

### Phase 4: Monitor and Optimize (Ongoing)

1. Log invalidation operations
2. Monitor cache hit rates
3. Adjust tag strategy based on usage patterns
4. Consider implementing tag index if needed

## Implementation Checklist

- [ ] Create `CacheTags.cs` utility class
- [ ] Add unit tests for tag generation (`CacheTagTests.cs`)
- [ ] Update `GetBylawsQuery` with proper tags
- [ ] Update `GetTournamentRulesQuery` with proper tags
- [ ] Update `ListBowlerOfTheYearAwardsQuery` with proper tags
- [ ] Update `ListHighAverageAwardsQuery` with proper tags
- [ ] Update `ListHigh5GameBlockAwardsQuery` with proper tags
- [ ] Update `BowlerTitlesQuery` with proper tags
- [ ] Update `ListBowlerTitlesQuery` with proper tags
- [ ] Update `ListBowlerTitleSummariesQuery` with proper tags
- [ ] Update ADR-002 to reference ADR-003
- [ ] Add cache invalidation examples to documentation
- [ ] Update PR review checklist with tag requirements

## Validation

### Tag Format Validation

```csharp
public static class CacheTagExtensions
{
    /// <summary>
    /// Validates that a tag follows the required format.
    /// </summary>
    public static bool IsValidCacheTag(this string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return false;

        // Must be: {context} or {context}:{category} or {context}:{category}:{entity-id}
        string[] parts = tag.Split(':');
        return parts.Length is >= 1 and <= 3 && parts.All(p => !string.IsNullOrWhiteSpace(p));
    }

    /// <summary>
    /// Gets the tag level (1=context, 2=type, 3=entity).
    /// </summary>
    public static int GetTagLevel(this string tag)
    {
        return tag.Split(':').Length;
    }
}
```

## References

### Microsoft Documentation
- [HybridCache Documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid)
- [Distributed Cache Tag Helper](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/built-in/distributed-cache-tag-helper)

### Industry Patterns
- [Redis Cache Tags Pattern](https://redis.io/docs/manual/patterns/indexes/)
- [Cache Invalidation Strategies](https://docs.aws.amazon.com/AmazonElastiCache/latest/red-ug/Strategies.html)
- [Tag-Based Cache Invalidation](https://symfony.com/doc/current/cache/cache_invalidation.html)

### Related ADRs
- [ADR-002: Cache Key Naming Conventions](adr-002-cache-key-naming-conventions.md)

---

**Decision Owner**: Development Team

**Last Updated**: 2024-12-24

**Next Review**: After implementation or 6 months, whichever comes first

