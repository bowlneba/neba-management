---
layout: default
title: Technical Building Blocks
---

# Technical Building Blocks

This document defines the technical infrastructure and cross-cutting domain concepts used throughout the NEBA Management application. These building blocks provide the foundation for implementing Domain-Driven Design (DDD) patterns and ensure consistency across all bounded contexts.

> **Note:** This document covers technical patterns and infrastructure. For business domain concepts, see the domain-specific ubiquitous language documents (Awards & Titles, Hall of Fame, etc.).

---

## DDD Base Types

### Entity\<TId>

**Definition:** An abstract base class representing a domain entity with a unique identity, as defined by Domain-Driven Design (DDD) principles. Entities are objects that are defined by their identity rather than their attributes, and their identity remains constant throughout their lifecycle.

**Purpose:**

- Provides a consistent foundation for all domain entities
- Implements identity-based equality (two entities are equal if their IDs are equal, regardless of other property values)
- Ensures all entities have a strongly-typed unique identifier
- Enforces DDD entity semantics across the domain model

**Type Parameters:**

- `TId` - The type of the entity's unique identifier (must be a struct that implements IEquatable<TId>)

**Properties:**

- `Id` - The unique identifier for the entity (read-only after initialization)

**Key Behaviors:**

- **Identity-based equality:** Two entities are considered equal if and only if their IDs are equal
- **Hash code based on ID:** The hash code is derived from the ID, ensuring consistent behavior in collections
- **Immutable identity:** Once set, the ID cannot be changed (enforced through `internal init` accessor)

**Examples of Entities:**

- **Title** - Identified by TitleId
- **SeasonAward** - Identified by SeasonAwardId
- **HallOfFameInduction** - Identified by HallOfFameId

**Contrast with Value Objects:**

Unlike entities, value objects (like Name or StoredFile) are defined by their attributes, not by identity. Two Name objects with the same FirstName and LastName are considered equal, even if they are separate instances.

**Code Reference:** `src/backend/Neba.Domain/Entity.cs`

---

### Aggregate\<TId>

**Definition:** An abstract base class representing a domain aggregate root, which serves as the entry point for a cluster of related objects in Domain-Driven Design (DDD). Aggregates define transactional boundaries and enforce business invariants across a group of related entities and value objects.

**Purpose:**

- Marks specific entities as aggregate roots (entry points for modifying related objects)
- Defines transactional consistency boundaries
- Enforces business rules and invariants across the aggregate
- Controls access to child entities (prevents direct external modification of children)

**Type Parameters:**

- `TId` - The type of the aggregate's unique identifier (must be a struct that implements IEquatable<TId>)

**Relationship to Entity:**

Aggregate<TId> inherits from Entity<TId>, meaning all aggregates are entities, but not all entities are aggregates.

**Examples of Aggregate Roots:**

- **Bowler** (aggregate root)
  - Contains Title entities (child)
  - Contains SeasonAward entities (child)
  - Contains HallOfFameInduction entities (child)
  - All modifications to titles, awards, and inductions must go through the Bowler aggregate

**Aggregate Boundaries:**

An aggregate boundary defines which objects can be modified together in a single transaction:

- **Inside the boundary:** Objects are part of the same aggregate and can be modified together
- **Outside the boundary:** Objects are in different aggregates and are accessed through their own aggregate roots
- **Invariants:** Business rules that must always be true for the entire aggregate

**Why Aggregate Roots Matter:**

- **Consistency:** Ensures related objects are always in a valid state
- **Transactional boundaries:** Changes to the aggregate are committed or rolled back as a unit
- **Encapsulation:** Hides internal complexity and protects business rules

**Code Reference:** `src/backend/Neba.Domain/Aggregate.cs`

---

## Documents vs Files

**Critical Distinction:** The NEBA Management system distinguishes between **Documents** and **Files** as two different concepts:

### Documents

**Definition:** Documents are content stored in external document management systems (Google Docs, Google Sheets, Microsoft Office, etc.) that are retrieved, converted to HTML, and cached for display in the application.

**Characteristics:**
- Sourced from Google Docs API, Microsoft Office 365, or similar document platforms
- Converted to HTML for display in the web application
- Examples: Bylaws, Tournament Rules, Meeting Minutes
- Cached in blob storage after conversion for performance
- Refreshed on-demand through background jobs

**Key Terms:**
- **Document Type:** The category of document (e.g., "bylaws", "tournament-rules")
- **Document Refresh:** The process of fetching a document from its source, converting it to HTML, and updating the cache
- **Document Service:** Application service that retrieves documents and manages their lifecycle

**Usage in Code:**
- `IDocumentsService` - Retrieves documents as HTML
- `SyncHtmlDocumentToStorageJob` - Background job that refreshes documents
- `DocumentRefreshStatus` - Tracks the status of document refresh operations

### Files

**Definition:** Files are binary or text assets stored in the system's blob storage (Azure Blob Storage, AWS S3, etc.) that are uploaded by users or generated by the system.

**Characteristics:**
- Stored directly in Azure Blob Storage (or AWS S3 in future)
- Represented by the `StoredFile` value object
- Examples: Photos, logos, PDFs, images, certificates
- Permanent storage with metadata (content type, size, location)
- Accessed directly via URLs or download endpoints

**Key Terms:**
- **StoredFile:** Value object representing a file's metadata and location
- **File Upload:** The process of accepting user-uploaded files and storing them
- **File Storage:** The infrastructure layer service that manages file persistence

**Usage in Code:**
- `StoredFile` value object - Represents file metadata
- `IStorageService` - Infrastructure service for file operations
- `TournamentFile` entity - Associates files with tournaments
- `HallOfFameInduction.Photo` - Photo file for inductees

**Why This Distinction Matters:**

1. **Different Lifecycles:** Documents are refreshed from external sources; files are uploaded once and rarely change
2. **Different Operations:** Documents are converted and cached; files are stored as-is
3. **Different Domains:** Documents are content management; files are asset storage
4. **Code Clarity:** Using precise terminology prevents confusion in code and discussions

**Examples:**

- ✅ "Refresh the bylaws **document** from Google Docs"
- ✅ "Upload the tournament logo **file** to Azure Storage"
- ❌ "Download the bylaws **file**" (should be "document")
- ❌ "Refresh the logo **document**" (should be "file")

---

## Value Objects

### StoredFile

**Definition:** A value object representing a **file** stored in blob storage (Azure Blob Storage, AWS S3, etc.), including its location, metadata, and content information. This represents uploaded or generated assets, **not** documents from Google Docs or Office.

**Purpose:**

- Provides a consistent representation of stored files across the domain
- Encapsulates file metadata (location, content type, size)
- Used for Hall of Fame photos, tournament logos, and other uploaded/generated files
- **Not used for documents** (bylaws, tournament rules) which are managed separately

**Properties:**

- `Container` - The storage location or path (e.g., blob container name, filesystem directory)
- `Path` - The original or stored file name including extension (e.g., "bowler-123.jpg")
- `ContentType` - The MIME content type of the file (e.g., "image/png", "image/jpeg", "application/pdf")
- `SizeInBytes` - The size of the file in bytes

**Value Object Semantics:**

- **Immutability:** StoredFile is a C# record, making it immutable by default
- **Value-based equality:** Two StoredFile instances are equal if all properties match
- **No identity:** Unlike entities, StoredFile has no unique identifier; it's defined entirely by its properties

**Usage Examples:**

- **Hall of Fame photos:** HallOfFameInduction.Photo property stores an optional photo file of the inductee
- **Tournament logos:** TournamentFile.File property stores tournament-related files like logos
- **Future use cases:** Bowler profile pictures, generated certificates, uploaded PDFs

**Not Used For:**

- Documents from Google Docs/Office (these are handled by the Documents subsystem)
- HTML content or text content (use appropriate domain primitives instead)

**Storage Implementation:**

The StoredFile value object describes the file's metadata but does not contain the actual file content. The actual storage mechanism (Azure Blob Storage, filesystem, etc.) is an infrastructure concern handled separately.

**Business Rules:**

- All properties are required (non-nullable strings) except when StoredFile itself is optional (nullable)
- Content type should be a valid MIME type
- Size in bytes should be a positive value
- Container and Path together define the unique storage location

**Related Terms:** HallOfFameInduction

**Code Reference:** `src/backend/Neba.Domain/StoredFile.cs`

---

## Strongly-Typed Identifiers

### Overview

**Definition:** Strongly-typed identifiers are dedicated types for entity IDs, providing type safety and preventing accidental misuse of identifiers. The NEBA application uses ULID (Universally Unique Lexicographically Sortable Identifier) for all entity identifiers.

**Why Strongly-Typed IDs?**

- **Type safety:** Prevents accidentally passing a BowlerId where a TitleId is expected
- **Compile-time checking:** Errors are caught at compile time rather than runtime
- **Self-documenting:** Code is more readable when method signatures use BowlerId instead of Guid or string
- **Domain clarity:** Makes the domain model more explicit and easier to understand

**ULID Benefits:**

- **Sortable:** ULIDs are lexicographically sortable, making them useful for ordering by creation time
- **URL-safe:** Can be used in URLs without encoding
- **Compact:** More compact than traditional GUIDs in string representation
- **Time-based:** Contains a timestamp component, providing creation order information

**Implementation:**

All strongly-typed identifiers use the StronglyTypedIds library with "ulid-full" format:

```csharp
[StronglyTypedId("ulid-full")]
public readonly partial struct BowlerId;
```

**Available Identifiers:**

1. **BowlerId** - Unique identifier for Bowler aggregate
   - Code reference: `src/backend/Neba.Domain/Identifiers/BowlerId.cs`

2. **TitleId** - Unique identifier for Title entity
   - Code reference: `src/backend/Neba.Domain/Identifiers/TitleId.cs`

3. **SeasonAwardId** - Unique identifier for SeasonAward entity
   - Code reference: `src/backend/Neba.Domain/Identifiers/SeasonAwardId.cs`

4. **HallOfFameId** - Unique identifier for HallOfFameInduction entity
   - Code reference: `src/backend/Neba.Domain/Identifiers/HallOfFameId.cs`

**Usage Example:**

```csharp
// Type-safe: Compiler prevents mixing up different ID types
BowlerId bowlerId = BowlerId.New();
TitleId titleId = TitleId.New();

// This would cause a compile error:
// ProcessBowler(titleId); // Error: Cannot convert TitleId to BowlerId
```

**Generated Methods:**

The StronglyTypedIds library generates:
- `New()` - Creates a new ULID
- `ToString()` - Converts to string representation
- `Parse(string)` - Parses from string
- Equality operators
- JSON serialization support

**Code References:** `src/backend/Neba.Domain/Identifiers/` directory

---

## Domain Errors Pattern

### Overview

**Definition:** The domain errors pattern provides a functional approach to error handling using the ErrorOr library. Instead of throwing exceptions, domain operations return `ErrorOr<T>` results that can contain either a successful value or one or more errors.

**Why ErrorOr Pattern?**

- **Explicit error handling:** Forces consumers to handle errors explicitly
- **Functional approach:** Avoids exceptions for expected failure cases (not found, validation failures, etc.)
- **Rich error information:** Errors contain codes, descriptions, and optional metadata
- **Composable:** Errors can be chained and transformed through operations
- **Performance:** Avoids the overhead of exception throwing for expected failures

**Error Types:**

ErrorOr supports multiple error types:
- **Validation:** Invalid input or business rule violations
- **NotFound:** Requested entity doesn't exist
- **Conflict:** Operation conflicts with current state
- **Unauthorized:** Permission denied
- **Failure:** General failure

**Domain Error Classes:**

Domain errors are organized into static classes by domain concept:

1. **NameErrors** - Errors related to Name value object
   - `FirstNameRequired` - First name is required (Validation error)
   - `LastNameRequired` - Last name is required (Validation error)
   - Code reference: `src/backend/Neba.Domain/NameErrors.cs`

2. **BowlerErrors** - Errors related to Bowler aggregate
   - `BowlerNotFound(BowlerId id)` - Bowler with specified ID was not found (NotFound error)
   - Includes metadata with the bowler ID for debugging
   - Code reference: `src/backend/Neba.Website.Domain/Bowlers/BowlerErrors.cs`

**Error Structure:**

Each error contains:
- **Code:** Unique error identifier (e.g., "Name.FirstName.Required", "Bowler.NotFound")
- **Description:** Human-readable error message
- **Type:** Error category (Validation, NotFound, etc.)
- **Metadata:** Optional additional information (e.g., which ID was not found)

**Usage Example:**

```csharp
// Domain method returns ErrorOr<T>
public static ErrorOr<Name> Create(string firstName, string lastName)
{
    if (string.IsNullOrWhiteSpace(firstName))
        return NameErrors.FirstNameRequired;

    if (string.IsNullOrWhiteSpace(lastName))
        return NameErrors.LastNameRequired;

    return new Name(firstName, lastName);
}

// Consumer handles the result
var result = Name.Create(firstName, lastName);
if (result.IsError)
{
    // Handle error case
    return result.Errors;
}

// Use successful value
Name name = result.Value;
```

**Benefits:**

- **Compile-time safety:** Consumers must handle errors; can't accidentally ignore them
- **No exception overhead:** Better performance for expected failures
- **Consistent error handling:** Same pattern across all domain operations
- **Rich error context:** Errors carry detailed information for debugging and user feedback

**Code References:**
- `src/backend/Neba.Domain/NameErrors.cs`
- `src/backend/Neba.Website.Domain/Bowlers/BowlerErrors.cs`

---

## Smart Enumerations

### Overview

**Definition:** Smart enumerations (using the Ardalis.SmartEnum library) provide type-safe enumerations with additional behavior beyond standard C# enums. They combine the benefits of enums (predefined set of values) with the power of classes (methods, properties, rich behavior).

**Why SmartEnum Instead of Regular Enums?**

- **Behavior:** Can include methods and properties
- **String representation:** Built-in name property (not just numeric values)
- **Type safety:** Strongly typed with compile-time checking
- **Extensibility:** Can add domain logic to enumeration values
- **Persistence:** Stores numeric values in database (efficient) while providing rich object model
- **Immutability:** Enumeration instances are predefined and immutable

**SmartEnum Types in NEBA:**

1. **Month** - Calendar months with display formatting
   - Values: January (1) through December (12)
   - Methods: `ToShortString()` returns 3-letter abbreviation
   - Code reference: `src/backend/Neba.Domain/Month.cs`

2. **TournamentType** - Tournament format classifications
   - Values: Singles, Doubles, Trios, Masters, etc.
   - Properties: `TeamSize`, `ActiveFormat`
   - Code reference: `src/backend/Neba.Website.Domain/Tournaments/TournamentType.cs`

3. **SeasonAwardType** - Types of season awards
   - Values: BowlerOfTheYear, HighAverage, High5GameBlock
   - Code reference: `src/backend/Neba.Domain/Awards/SeasonAwardType.cs`

4. **BowlerOfTheYearCategory** - BOTY award categories
   - Values: Open, Woman, Senior, SuperSenior, Rookie, Youth
   - Code reference: `src/backend/Neba.Domain/Awards/BowlerOfTheYearCategory.cs`

5. **HallOfFameCategory** - Hall of Fame categories (Flag SmartEnum)
   - Values: SuperiorPerformance, MeritoriousService, FriendOfNeba
   - Special: Uses `SmartFlagEnum<T>` for bitwise flag operations
   - Code reference: `src/backend/Neba.Domain/Awards/HallOfFameCategory.cs`

**Usage Example:**

```csharp
// Type-safe assignment
Month month = Month.January;

// Comparison
if (title.Month == Month.December)
{
    // Special handling for December titles
}

// Display formatting
string shortMonth = month.ToShortString(); // "Jan"
string fullMonth = month.Name; // "January"
int numericMonth = month.Value; // 1

// Iteration
foreach (var month in Month.List)
{
    Console.WriteLine($"{month.Name}: {month.Value}");
}
```

**Flag SmartEnum (HallOfFameCategory):**

HallOfFameCategory uses `SmartFlagEnum<T>` for bitwise operations:

```csharp
// Combine multiple categories
var categories = new List<HallOfFameCategory>
{
    HallOfFameCategory.SuperiorPerformance,
    HallOfFameCategory.MeritoriousService
};

// Check if category is included
if (categories.Contains(HallOfFameCategory.SuperiorPerformance))
{
    // Handle superior performance recognition
}
```

**Benefits:**

- **Domain-rich:** Enumerations can carry domain behavior and rules
- **Database-friendly:** Stores as numeric values for efficiency
- **Type-safe:** Compile-time checking prevents invalid values
- **Discoverable:** IntelliSense shows all available values and their behaviors

---

## Design Patterns Summary

### Patterns Used in NEBA Domain Model

1. **Aggregate Pattern**
   - Defines transactional boundaries
   - Enforces invariants across related entities
   - Example: Bowler aggregate contains Titles, SeasonAwards, HallOfFameInductions

2. **Value Object Pattern**
   - Objects defined by attributes, not identity
   - Immutable
   - Examples: Name, StoredFile, Month

3. **Entity Pattern**
   - Objects defined by identity
   - Identity persists throughout lifecycle
   - Examples: Title, SeasonAward, HallOfFameInduction

4. **Strongly-Typed IDs**
   - Type-safe identifiers
   - Prevents mixing up different entity types
   - ULID-based for sortability and compactness

5. **Result Pattern (ErrorOr)**
   - Functional error handling
   - Explicit success/failure handling
   - Rich error information without exceptions

6. **Smart Enumeration Pattern**
   - Type-safe enumerations with behavior
   - Domain-rich representation of discrete values
   - Examples: Month, TournamentType, SeasonAwardType

7. **Flag Enumeration Pattern**
   - Bitwise flag operations for multiple values
   - Type-safe flag combinations
   - Example: HallOfFameCategory (multiple categories per induction)

---

## Related Documentation

- [Awards & Titles]({{ '/ubiquitous-language/awards-and-titles' | relative_url }}) - Business domain concepts for awards and titles
- [Hall of Fame]({{ '/ubiquitous-language/hall-of-fame' | relative_url }}) - Business domain concepts for Hall of Fame
- [Bounded Contexts]({{ '/architecture/bounded-contexts' | relative_url }}) - Architectural boundaries and context mapping
- [Domain Models]({{ '/domain-models' | relative_url }}) (Coming Soon)
- [Clean Architecture]({{ '/architecture/clean-architecture' | relative_url }}) (Coming Soon)
