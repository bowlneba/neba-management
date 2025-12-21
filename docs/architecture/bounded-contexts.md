---
layout: default
title: Bounded Contexts
---

# Bounded Contexts

This document defines the bounded contexts in the NEBA Management application, following Domain-Driven Design principles. Each bounded context represents a distinct area of the business domain with its own ubiquitous language, models, and boundaries.

---

## Overview

The NEBA Management application is structured as a **modular monolith** where each bounded context is implemented as a separate module within a single deployable unit. This provides the benefits of modular design while avoiding the operational complexity of microservices.

### Current Bounded Contexts

1. **Website Context** - Public-facing website features (history, awards, titles)
2. **Shared Kernel** - Common domain primitives and infrastructure

### Planned Bounded Contexts

- **Tournaments Context** - Tournament operations, scheduling, and management
- **Members Context** - Membership management, registration, and profiles
- **Admin Context** - Administrative operations and governance

---

## Website Bounded Context

**Purpose**: Provides public-facing features for displaying NEBA history, awards, and titles to website visitors.

### Responsibilities

- Display bowler championship titles (Singles, Doubles, Trios, etc.)
- Display season awards (Bowler of the Year, High Average, High Block)
- Provide searchable/filterable views of historical tournament data
- Serve tournament rules and organization bylaws documents
- Maintain read-optimized queries for public website display

### Domain Model

**Aggregate Root**: `Bowler`

**Entities**:

- `Bowler` - NEBA member who participates in tournaments
- `Title` - Championship win in a tournament
- `SeasonAward` - Season-level achievement (BOTY, High Average, High Block)

**Value Objects**:

- `Name` - Bowler's complete name with formatting options
- `Month` - Calendar month with display formats
- `BowlerId`, `TitleId`, `SeasonAwardId` - Strongly-typed identifiers

**Enumerations** (SmartEnum):

- `TournamentType` - Tournament formats (Singles, Doubles, Trios, Masters, etc.)
- `SeasonAwardType` - Award types (BowlerOfTheYear, HighAverage, High5GameBlock)
- `BowlerOfTheYearCategory` - BOTY categories (Open, Woman, Senior, SuperSenior, Rookie, Youth)

### Ubiquitous Language

See [Awards & Titles Ubiquitous Language](../ubiquitous_language/awards-and-titles.md) for detailed term definitions.

**Key Terms**:

- **Title**: A championship win in a NEBA tournament
- **Season Award**: Recognition for exceptional achievement during a season
- **Bowler of the Year (BOTY)**: Points-based award for overall performance
- **High Average**: Award for highest average with minimum games requirement
- **High Block**: Award for highest 5-game qualifying block score
- **Stat-Eligible Tournament**: Tournament where all NEBA members can participate

### Technical Structure

```text
Neba.Website.Domain
├── Bowlers/
│   ├── Bowler.cs (Aggregate Root)
│   ├── BowlerErrors.cs
│   └── (Name.cs moved to Neba.Domain as shared concept)
├── Tournaments/
│   ├── Title.cs
│   └── TournamentType.cs
└── Awards/
    ├── SeasonAward.cs
    ├── SeasonAwardType.cs
    └── BowlerOfTheYearCategory.cs

Neba.Website.Application
├── Bowlers/
│   ├── BowlerTitles/ (queries and handlers)
│   └── IWebsiteBowlerQueryRepository.cs
├── Tournaments/ (queries)
├── Awards/ (queries and handlers)
└── Documents/ (bylaws, tournament rules)

Neba.Website.Infrastructure
├── Database/
│   ├── WebsiteDbContext.cs
│   ├── Configurations/ (EF Core mappings)
│   ├── Migrations/
│   └── Repositories/
└── Health/ (health checks)

Neba.Website.Endpoints
├── Bowlers/
├── Titles/
├── Awards/
└── Documents/

Neba.Website.Contracts
├── Bowlers/ (DTOs)
├── Titles/ (DTOs)
└── Awards/ (DTOs)
```

### Database Schema

**PostgreSQL Schema**: `website`

**Tables**:

- `website.bowlers` - Bowler entities with Name value object columns
- `website.titles` - Title entities with FK to bowlers
- `website.season_awards` - SeasonAward entities with FK to bowlers

**Identity Strategy**: Hybrid ULID + Shadow Keys (see [ADR-001](adr-001-ulid-shadow-keys.md))

- `domain_id` (char(26)) - ULID for domain identity
- `id` (int) - Shadow property for database relationships

**Connection String**: Includes `SearchPath=website` for automatic schema qualification

### Integration Points

**Inbound**:

- Public website (Blazor WebAssembly) via HTTP/REST API
- OpenAPI documentation for API consumers

**Outbound**:

- Google Docs API (for fetching bylaws and tournament rules documents)
- Azure Key Vault (for storing Google Docs API credentials)

### Bounded Context Boundaries

**What's Inside**:

- Read-only views of bowler achievements for public display
- Historical tournament results and awards
- Document retrieval (bylaws, rules)

**What's Outside** (other contexts will handle):

- Tournament operations (scheduling, scoring, registration) → Tournaments Context
- Member management (profiles, memberships, contact info) → Members Context
- Administrative operations (user management, permissions) → Admin Context
- Write operations for tournaments and awards (currently handled externally, will migrate)

### Design Decisions

1. **Read-Only Model**: Website context is primarily read-only; write operations for awards/titles will be handled by other contexts
2. **Denormalized Queries**: Repository queries return denormalized DTOs optimized for display
3. **CQRS Pattern**: All operations are queries (no commands yet)
4. **Aggregate Boundary**: Bowler is the aggregate root containing Titles and SeasonAwards for consistency

---

## Shared Kernel

**Purpose**: Provides common domain primitives, infrastructure, and abstractions shared across all bounded contexts.

### Responsibilities

- Define base domain building blocks (Entity, Aggregate, Value Objects)
- Provide strongly-typed identifier patterns
- Offer CQRS abstractions (IQuery, IQueryHandler, ICommand, ICommandHandler)
- Supply cross-cutting infrastructure (database base context, Key Vault, HTTP utilities)
- Define shared contracts and response types

### Components

**Neba.Domain** (Domain Primitives):

- `Entity<TId>` - Base class for entities
- `Aggregate<TId>` - Base class for aggregate roots
- Strongly-typed ID base classes and converters
- Common value objects (Name, Month)
- Identifier types used across contexts (BowlerId, TitleId, SeasonAwardId)

**Neba.Application** (Application Abstractions):

- `IQuery<TResult>` - Query marker interface
- `IQueryHandler<TQuery, TResult>` - Query handler interface
- `ICommand<TResult>` - Command marker interface (future)
- `ICommandHandler<TCommand, TResult>` - Command handler interface (future)

**Neba.Infrastructure** (Cross-Cutting Infrastructure):

- `NebaDbContext` - Base database context
- `UlidConfiguration` - Helper for ULID entity configuration
- Key Vault integration
- Google Docs integration
- HTTP utilities (ResultExtensions)

**Neba.Contracts** (Shared DTOs):

- Common response types
- Shared data transfer objects

### Design Principles

1. **Minimize Shared Kernel**: Only include truly shared concepts; context-specific items stay in their context
2. **Stability**: Shared kernel should change infrequently; breaking changes affect all contexts
3. **No Business Logic**: Infrastructure and primitives only; domain logic stays in bounded contexts

---

## Context Map

### Relationships

```text
┌─────────────────────────────────────────────────┐
│              Shared Kernel                      │
│  (Neba.Domain, Neba.Application,                │
│   Neba.Infrastructure, Neba.Contracts)          │
└─────────────────────────────────────────────────┘
                      ▲
                      │ Depends on
                      │
┌─────────────────────────────────────────────────┐
│          Website Bounded Context                │
│  (Read-only public website features)            │
│  - Bowlers, Titles, Awards, Documents           │
└─────────────────────────────────────────────────┘


Future Contexts:

┌─────────────────────────────────────────────────┐
│        Tournaments Bounded Context              │
│  (Tournament operations, scheduling, scoring)   │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│         Members Bounded Context                 │
│  (Membership management, profiles, contact)     │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│          Admin Bounded Context                  │
│  (Administrative operations, permissions)       │
└─────────────────────────────────────────────────┘
```

### Integration Patterns

**Shared Kernel Pattern**: All contexts depend on the shared kernel for common primitives and infrastructure.

**Conformist Pattern** (Planned): Future contexts may need to conform to external systems (e.g., USBC APIs, existing NEBA systems).

**Published Language** (Planned): When contexts need to communicate, they will use published DTOs and contracts.

---

## Module Independence

### Physical Organization

Each bounded context is a separate set of .NET projects following Clean Architecture layers:

- `[Context].Domain` - Domain model
- `[Context].Application` - Use cases and handlers
- `[Context].Infrastructure` - Database, repositories, external integrations
- `[Context].Endpoints` - HTTP API endpoints
- `[Context].Contracts` - DTOs and contracts

### Dependency Rules

```text
Endpoints → Infrastructure → Application → Domain
     ↓            ↓               ↓           ↓
  Contracts ← Contracts ←────── Domain ← (Shared Kernel)
```

- **Domain** depends only on Shared Kernel (Neba.Domain)
- **Application** depends on Domain and Application abstractions (Neba.Application)
- **Infrastructure** depends on Application and provides implementations
- **Endpoints** depends on Infrastructure and Contracts
- **Contracts** depends on Domain for identifiers and value objects

### Isolation Benefits

1. **Independent Development**: Teams can work on different contexts without conflicts
2. **Clear Boundaries**: Each context has its own database schema, models, and language
3. **Scalability**: Contexts can be extracted to microservices if needed in the future
4. **Testability**: Each context can be tested independently
5. **Domain Focus**: Each context uses its own ubiquitous language

---

## Evolution Strategy

### Adding New Contexts

When adding a new bounded context:

1. **Define the boundary**: What responsibilities does this context have?
2. **Identify the ubiquitous language**: Document key terms and definitions
3. **Model the domain**: Aggregates, entities, value objects
4. **Create the project structure**: Domain, Application, Infrastructure, Endpoints, Contracts
5. **Define the database schema**: New PostgreSQL schema for the context
6. **Document integration points**: How does this context interact with others?

### Refactoring Existing Code

As new contexts are added, some shared concepts may need to move:

- **Move to Shared Kernel**: If truly shared across multiple contexts
- **Duplicate**: If contexts have similar but divergent concepts
- **Context-Specific**: If only used within one context

### Migration Path

Current state: Website context with read-only features

Planned evolution:

1. **Phase 1**: Add Tournaments context (tournament operations, scoring)
2. **Phase 2**: Add Members context (membership management)
3. **Phase 3**: Add Admin context (administrative operations)
4. **Phase 4**: Define integration events for cross-context communication

---

## Related Documentation

- [Ubiquitous Language - Awards & Titles](../ubiquitous_language/awards-and-titles.md)
- [ADR-001: ULID and Shadow Key Pattern](adr-001-ulid-shadow-keys.md)
- [Database Schema Documentation](../database/schema.md) (Coming Soon)
- [API Reference](../api/reference.md) (Coming Soon)
