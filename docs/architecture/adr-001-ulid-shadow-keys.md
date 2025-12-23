---
layout: default
title: ADR-001 - ULID and Shadow Key Pattern
---

# ADR-001: ULID and Shadow Key Pattern

**Status**: Accepted

**Date**: 2025-12-20

**Context**: Modular Monolith Refactoring

---

## Context and Problem Statement

The NEBA Management application needs an identity strategy for domain entities that balances several competing concerns:

1. **Domain Identity**: Entities need stable, globally unique identifiers that are meaningful in the domain model
2. **Database Performance**: Foreign key relationships benefit from small, efficient integer keys
3. **Distributed Systems**: Future scalability may require identifiers that can be generated independently without coordination
4. **Ordering**: The ability to sort entities by creation time is valuable for many use cases
5. **Developer Experience**: Identifiers should be easy to work with in both domain code and database queries

Traditional approaches have limitations:

- **Auto-increment integers**: Fast and efficient, but not globally unique; can't be generated before database insert
- **GUIDs/UUIDs**: Globally unique but large (16 bytes), not sortable, poor database performance as clustered keys
- **Single identifier type**: Forces a compromise between domain clarity and database efficiency

## Decision

We will use a **hybrid identity strategy** combining ULIDs for domain identity with shadow integer keys for database relationships:

### Primary Strategy: ULID for Domain Identity

Each entity has a strongly-typed ULID identifier (e.g., `BowlerId`, `TitleId`, `SeasonAwardId`):

- **Domain model** uses ULID identifiers exclusively
- **Database column**: `domain_id` (char(26), unique, indexed)
- **ULID format**: 26-character base32-encoded string (e.g., `01ARZ3NDEKTSV4RRFFQ69G5FAV`)

### Secondary Strategy: Shadow Integer Keys for Database Relationships

Each entity has a hidden integer primary key managed by EF Core:

- **Database column**: `id` (int, primary key, identity always)
- **Not exposed** in domain model
- **Used for**: Foreign key relationships, database performance

### Implementation Pattern

```csharp
// Domain Entity
public class Bowler : Aggregate<BowlerId>
{
    public BowlerId Id { get; private set; } // ULID domain identifier
    public Name Name { get; private set; }
    // ... other domain properties
}

// EF Core Configuration
public class BowlerConfiguration : IEntityTypeConfiguration<Bowler>
{
    public void Configure(EntityTypeBuilder<Bowler> builder)
    {
        // Configure ULID domain identifier
        builder.Property(bowler => bowler.Id)
            .IsUlid<BowlerId, BowlerId.EfCoreValueConverter>();

        // Configure shadow integer primary key
        builder.Property<int>("db_id")
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.HasKey("db_id");

        // Create unique index on domain_id
        builder.HasIndex(bowler => bowler.Id)
            .IsUnique();
    }
}

// Helper for consistent configuration
public static class UlidConfiguration
{
    public static void IsUlid<TProperty, TConverter>(
        this PropertyBuilder<TProperty> builder)
        where TConverter : ValueConverter, new()
    {
        builder
            .HasColumnName("domain_id")
            .HasColumnType("char(26)")
            .HasConversion<TConverter>()
            .IsRequired();
    }
}
```

### Database Schema

```sql
CREATE TABLE website.bowlers (
    id              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    domain_id       CHAR(26) NOT NULL,
    first_name      VARCHAR(100) NOT NULL,
    last_name       VARCHAR(100) NOT NULL,
    -- ... other columns
    CONSTRAINT uq_bowlers_domain_id UNIQUE (domain_id)
);

CREATE TABLE website.titles (
    id              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    domain_id       CHAR(26) NOT NULL,
    bowler_id       INT NOT NULL,  -- FK to bowlers.id (not bowlers.domain_id)
    tournament_type INT NOT NULL,
    month           INT NOT NULL,
    year            INT NOT NULL,
    CONSTRAINT uq_titles_domain_id UNIQUE (domain_id),
    CONSTRAINT fk_titles_bowler FOREIGN KEY (bowler_id)
        REFERENCES website.bowlers(id) ON DELETE CASCADE
);
```

## Rationale

### Why ULID over GUID/UUID?

**Advantages of ULID**:

1. **Sortable by creation time**: First 48 bits encode timestamp (millisecond precision)
2. **Smaller string representation**: 26 characters vs 36 for UUID (with hyphens)
3. **Lexicographically sortable**: String comparison matches temporal order
4. **Monotonically increasing**: Within same millisecond, random bits ensure uniqueness
5. **Case-insensitive**: Base32 encoding (Crockford alphabet)

**Comparison**:

```text
ULID:  01ARZ3NDEKTSV4RRFFQ69G5FAV  (26 chars)
UUID:  123e4567-e89b-12d3-a456-426614174000  (36 chars)
```

### Why Shadow Integer Keys?

**Performance Benefits**:

1. **Foreign Key Efficiency**: 4-byte int vs 26-byte char in FK columns
2. **Index Performance**: Integer indexes are smaller and faster
3. **Join Performance**: Integer joins are more efficient than string joins
4. **Storage Savings**: Significant space savings with many relationships

**Example Savings** (Title table with 1000 records):

```text
With CHAR(26) FK:  bowler_id column = 1000 × 26 = 26 KB
With INT FK:       bowler_id column = 1000 × 4 = 4 KB
Savings: 84.6% per foreign key column
```

### Why Hybrid Approach?

**Benefits**:

1. **Best of Both Worlds**: Domain clarity + database performance
2. **Future-Proof**: ULIDs support distributed ID generation if needed
3. **Clean Domain Model**: Domain code works with meaningful identifiers
4. **Optimized Queries**: Database uses efficient integer relationships
5. **Flexibility**: Can expose ULID in APIs while using ints internally

**Trade-offs Accepted**:

1. **Complexity**: Two identifiers per entity (mitigated by EF Core shadow properties)
2. **Storage**: Additional unique index on `domain_id` (acceptable overhead)
3. **Migration Effort**: Requires careful EF Core configuration (one-time cost)

## Consequences

### Positive

- **Domain Purity**: Domain model uses meaningful, strongly-typed identifiers
- **Performance**: Database operations benefit from integer keys
- **Scalability**: ULIDs support future distributed scenarios
- **Ordering**: Entities can be sorted by creation time via ULID
- **Developer Experience**: Clear separation between domain and persistence concerns

### Negative

- **Increased Complexity**: Two identifiers to manage (mitigated by `UlidConfiguration` helper)
- **Learning Curve**: Team must understand the pattern
- **Migration Complexity**: Converting existing data requires careful scripting

### Neutral

- **API Design**: Must decide which identifier to expose (recommendation: ULID for public APIs)
- **Query Patterns**: Must query by `domain_id` when using ULID from domain

## Implementation Notes

### Consistency via UlidConfiguration Helper

To ensure consistent configuration across all entities:

```csharp
// Neba.Infrastructure/Database/Configurations/UlidConfiguration.cs
public static class UlidConfiguration
{
    public static void IsUlid<TProperty, TConverter>(
        this PropertyBuilder<TProperty> builder)
        where TConverter : ValueConverter, new()
    {
        builder
            .HasColumnName("domain_id")
            .HasColumnType("char(26)")
            .HasConversion<TConverter>()
            .IsRequired();
    }
}
```

Usage in entity configurations:

```csharp
builder.Property(entity => entity.Id)
    .IsUlid<BowlerId, BowlerId.EfCoreValueConverter>();
```

### Migration from Legacy IDs

For entities with legacy integer IDs (e.g., `website_id`, `application_id`):

1. Add `domain_id` column with ULID
2. Keep legacy ID columns for reference (nullable, unique)
3. Generate ULIDs for existing records during migration
4. Update foreign keys to use shadow `id` column
5. Maintain legacy IDs for historical data correlation

### Foreign Key Relationships

Foreign keys reference the shadow `id` column:

```csharp
// Title entity referencing Bowler
builder.HasOne<Bowler>()
    .WithMany(bowler => bowler.Titles)
    .HasForeignKey("bowler_id")  // References bowlers.id (int)
    .OnDelete(DeleteBehavior.Cascade);
```

### Querying Patterns

**By ULID from domain code**:

```csharp
var bowler = await context.Bowlers
    .FirstOrDefaultAsync(b => b.Id == bowlerId);  // bowlerId is BowlerId (ULID)
```

**EF Core translates to**:

```sql
SELECT * FROM website.bowlers WHERE domain_id = '01ARZ3NDEKTSV4RRFFQ69G5FAV';
```

## Alternatives Considered

### Alternative 1: GUID/UUID Only

**Rejected because**:

- Not sortable by creation time
- Larger string representation (36 chars)
- Poor database performance as clustered key
- Less human-readable

### Alternative 2: Integer Only

**Rejected because**:

- Not globally unique
- Requires database round-trip for generation
- Doesn't support distributed ID generation
- Less meaningful in domain model

### Alternative 3: Composite Natural Keys

**Rejected because**:

- Complex foreign key relationships
- Mutable natural keys cause update cascades
- Poor performance for large composite keys

### Alternative 4: ULID as Primary Key (No Shadow Key)

**Rejected because**:

- Foreign key columns would be char(26), reducing performance
- Larger indexes and slower joins
- Storage overhead for relationship-heavy models

## Related Decisions

- [Bounded Contexts](bounded-contexts.md) - Context separation and schema-per-context pattern
- [Strongly-Typed IDs](https://github.com/andrewlock/StronglyTypedId) - Pattern for type-safe identifiers

## References

- [ULID Specification](https://github.com/ulid/spec)
- [EF Core Shadow Properties](https://learn.microsoft.com/en-us/ef/core/modeling/shadow-properties)
- [PostgreSQL Identity Columns](https://www.postgresql.org/docs/current/ddl-identity-columns.html)
- [Strongly Typed IDs in C#](https://andrewlock.net/using-strongly-typed-entity-ids-to-avoid-primitive-obsession-part-1/)

## Implementation Commits

- `0794cd1` - Refactor to use domain_id and ULID config for entities
- `bc5a2f2` - Refactor to use shadow int keys for bowler relations
- `5983dd0` - Refactor migration scripts to use BowlerDbId and update mappings
