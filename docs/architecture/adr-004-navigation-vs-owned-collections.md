---
layout: default
title: "ADR-004: Navigation Properties vs. Owned Collections Pattern"
---

# ADR-004: Navigation Properties vs. Owned Collections Pattern

**Status**: Accepted
**Date**: 2026-01-03
**Context**: Entity Framework Core relationship management in Domain-Driven Design

## Context

When modeling domain entities with Entity Framework Core, collections can serve two distinct purposes:

1. **Navigation properties**: For querying/projection across aggregate boundaries
2. **Owned collections**: Part of the aggregate's state, protected by invariants

Without a clear pattern, it's unclear which collections EF Core can freely mutate versus which should only be modified through domain methods. This led to issues like the "Collection was of a fixed size" exception when EF Core tried to modify collections initialized with `[]` syntax.

## Decision

We distinguish between **navigation properties** and **owned collections** using different patterns:

### Navigation Properties (For Projection Only)

Use when the collection crosses aggregate boundaries and exists solely for querying.

**Pattern**:
```csharp
// Navigation property for querying a bowler's titles. Titles are not owned by the
// Bowler aggregate - they are owned by tournaments in the domain sense. This collection
// exists solely for projection/querying purposes (e.g., "show all championships this
// bowler has won"). Titles are created separately and reference both tournaments and
// bowlers.
//
// Technical note: Uses a mutable backing field because when titles are created that
// reference existing bowlers, EF Core modifies this collection during relationship
// fixup. Using a readonly array ([]) would cause a "Collection was of a fixed size"
// exception. The public property remains IReadOnlyCollection<Title> to maintain the
// immutable API contract.
private List<Title> _titles = [];

/// <summary>
/// Gets the collection of championship titles won by the bowler.
/// This is a navigation property for projection only - titles are owned by tournaments.
/// </summary>
internal IReadOnlyCollection<Title> Titles
{
    get => _titles;
    init => _titles = value?.ToList() ?? [];
}
```

**Characteristics**:
- ✅ Mutable backing field (`List<T>`) for EF Core
- ✅ Read-only public API (`IReadOnlyCollection<T>`)
- ✅ No domain methods to mutate
- ✅ Entities created separately, not as part of aggregate construction
- ✅ EF Core manages the collection during relationship fixup

### Owned Collections (Part of Aggregate State)

Use when the collection is part of the aggregate's state and protected by business rules.

**Pattern**:
```csharp
/// <summary>
/// Gets the collection of season awards earned by the bowler (BOTY, High Average, High Block).
/// This is an owned collection that is part of the Bowler aggregate's state and should be
/// mutated only through domain methods that enforce business rules.
/// </summary>
internal IReadOnlyCollection<SeasonAward> SeasonAwards { get; init; }

// Domain method to mutate owned collection
public void AwardSeasonAward(SeasonAward award)
{
    // Validate invariants
    if (_seasonAwards.Any(a => a.Season == award.Season && a.AwardType == award.AwardType))
        throw new InvalidOperationException("Bowler already has this award for this season");

    _seasonAwards.Add(award);
}
```

**Characteristics**:
- ✅ Private mutable backing field (when domain methods needed)
- ✅ Read-only public API (`IReadOnlyCollection<T>`)
- ✅ Domain methods enforce invariants
- ✅ Entities created as part of aggregate construction
- ✅ Protected aggregate boundaries

## Examples

### Tournament → Titles (Navigation)

```csharp
public sealed class Tournament : Aggregate<TournamentId>
{
    // Navigation property - titles are owned by tournament in domain sense
    // but collection is for projection only
    private readonly List<Title> _champions = [];

    public IReadOnlyCollection<Title> Champions
    {
        get => _champions;
        init => _champions = value?.ToList() ?? [];
    }
}
```

**Why navigation**: Titles are semantically owned by tournaments (a title can't exist without a tournament), but the `Champions` collection is not part of the Tournament aggregate's invariants. Titles are created separately and reference both tournaments and bowlers.

### Bowler → Titles (Navigation)

```csharp
public sealed class Bowler : Aggregate<BowlerId>
{
    // Navigation property - titles owned by tournaments, not bowler
    private List<Title> _titles = [];

    internal IReadOnlyCollection<Title> Titles
    {
        get => _titles;
        init => _titles = value?.ToList() ?? [];
    }
}
```

**Why navigation**: Titles belong to tournaments in the domain. This collection exists for queries like "show all championships this bowler has won."

### Bowler → Season Awards (Owned)

```csharp
public sealed class Bowler : Aggregate<BowlerId>
{
    /// <summary>
    /// Gets the collection of season awards earned by the bowler.
    /// This is an owned collection that is part of the Bowler aggregate's state.
    /// </summary>
    internal IReadOnlyCollection<SeasonAward> SeasonAwards { get; init; }
}
```

**Why owned**: Season awards are part of the Bowler aggregate's state. They should be mutated through domain methods that enforce business rules (e.g., preventing duplicate awards for the same season).

## Technical Implementation

### The "Collection was of a fixed size" Problem

When using collection expressions (`[]`) for navigation properties:

```csharp
// ❌ This causes EF Core errors
public IReadOnlyCollection<Title> Champions { get; init; } = [];
```

The `[]` syntax creates a **fixed-size array**. When EF Core tracks entities and tries to add related entities during relationship fixup, it fails.

**Solution**: Use a mutable backing field:

```csharp
// ✅ EF Core can modify the backing field
private readonly List<Title> _champions = [];

public IReadOnlyCollection<Title> Champions
{
    get => _champions;
    init => _champions = value?.ToList() ?? [];
}
```

### Test Data Seeding Pattern

For navigation properties, create entities separately in test data:

```csharp
// 1. Create and save bowling centers
var bowlingCenters = BowlingCenterFactory.Bogus(10);
await context.BowlingCenters.AddRangeAsync(bowlingCenters);
await context.SaveChangesAsync();

// 2. Create and save tournaments (referencing bowling centers)
var tournaments = TournamentFactory.Bogus(50, bowlingCenters);
await context.Tournaments.AddRangeAsync(tournaments);
await context.SaveChangesAsync();

// 3. Create and save bowlers (without titles)
var bowlers = BowlerFactory.Bogus(50);
await context.Bowlers.AddRangeAsync(bowlers);
await context.SaveChangesAsync();

// 4. Create titles that reference both tournaments and bowlers
var titles = TitleFactory.Bogus(100, tournaments, bowlers);
await context.Titles.AddRangeAsync(titles);
await context.SaveChangesAsync();
```

This matches the domain flow: tournaments award titles to bowlers.

## Consequences

### Positive

- ✅ **Clear domain boundaries**: Immediately obvious which collections are owned vs. navigation
- ✅ **EF Core compatibility**: Mutable backing fields prevent "fixed size collection" errors
- ✅ **Immutable public API**: `IReadOnlyCollection<T>` enforces read-only access
- ✅ **Domain method enforcement**: Owned collections can only be mutated through methods that validate invariants
- ✅ **Better test data**: Separating entity creation matches real-world domain flow

### Negative

- ⚠️ **More boilerplate**: Navigation properties require backing field + property
- ⚠️ **Not obvious**: Developers need to understand the pattern
- ⚠️ **Testing complexity**: Test data creation requires careful ordering

### Neutral

- ℹ️ **Documentation critical**: Comments must explain the distinction
- ℹ️ **Consistent application**: Pattern must be applied consistently across the codebase

## Related Patterns

- **ADR-001**: [ULID and Shadow Key Pattern](adr-001-ulid-shadow-keys.md) - How entities are identified
- **Bounded Contexts**: [Bounded Contexts](bounded-contexts.md) - How aggregates are organized

## References

- Entity Framework Core: [Backing Fields](https://learn.microsoft.com/en-us/ef/core/modeling/backing-field)
- Domain-Driven Design: Eric Evans, "Domain-Driven Design: Tackling Complexity in the Heart of Software"
- Aggregate Pattern: Vaughn Vernon, "Implementing Domain-Driven Design"
