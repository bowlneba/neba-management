# neba-management

Centralized platform for managing the New England Bowlers Association (NEBA). Handles tournament operations, enforces NEBA and USBC rules, and streamlines governance and member management.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)

## Code Status

### Quality Scans

[![CodeQL](https://github.com/bowlneba/neba-management/workflows/CodeQL/badge.svg)](https://github.com/bowlneba/neba-management/security/code-scanning)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=bowlneba_neba-management&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=bowlneba_neba-management)

[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=bowlneba_neba-management&metric=bugs)](https://sonarcloud.io/summary/new_code?id=bowlneba_neba-management)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=bowlneba_neba-management&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=bowlneba_neba-management)

[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=bowlneba_neba-management&metric=coverage)](https://sonarcloud.io/summary/new_code?id=bowlneba_neba-management)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=bowlneba_neba-management&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=bowlneba_neba-management)

[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=bowlneba_neba-management&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=bowlneba_neba-management)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=bowlneba_neba-management&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=bowlneba_neba-management)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=bowlneba_neba-management&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=bowlneba_neba-management)


## Architecture

This application follows a **Modular Monolith** architecture with **Domain-Driven Design (DDD)** tactical patterns and **Clean Architecture** principles.

### Structure

```text
src/backend/
├── Core Infrastructure (Shared Kernel)
│   ├── Neba.Domain              # Domain primitives (IDs, base classes, value objects)
│   ├── Neba.Application         # Application abstractions (CQRS patterns)
│   ├── Neba.Infrastructure      # Cross-cutting infrastructure (DB, Key Vault, HTTP)
│   ├── Neba.Contracts           # Shared DTOs and response types
│   └── Neba.Api                 # API host (Program.cs, OpenAPI, health checks)
│
└── Website Bounded Context
    ├── Neba.Website.Domain          # Domain entities and business logic
    ├── Neba.Website.Application     # Use cases and query handlers
    ├── Neba.Website.Infrastructure  # Database context and repositories
    ├── Neba.Website.Endpoints       # HTTP endpoints
    └── Neba.Website.Contracts       # DTOs specific to website context
```

### Key Patterns

- **Bounded Contexts**: Modules are organized by business domain (Website context for public-facing features)
- **CQRS**: Command/Query separation using `IQuery<T>` and `IQueryHandler<TQuery, TResult>`
- **Repository Pattern**: Interface-based data access abstraction
- **Aggregate Roots**: Domain entities (e.g., Bowler) with consistency boundaries
- **Value Objects**: Immutable domain concepts (e.g., Name, Month)
- **Strongly-Typed IDs**: Type-safe identifiers (BowlerId, TitleId, SeasonAwardId)

### Database Strategy

**Schema-per-Context Pattern** using PostgreSQL:

- `website` schema for Website bounded context
- EF Core with `SearchPath` configuration for automatic schema qualification
- **Hybrid Identity Strategy**:
  - `domain_id` (ULID, 26-character) for domain identity
  - `id` (int, shadow property) for database foreign key relationships
  - See [ADR: ULID and Shadow Key Pattern](docs/architecture/adr-001-ulid-shadow-keys.md)

### Technology Stack

- **Backend**: ASP.NET Core (.NET 10), EF Core, PostgreSQL
- **Frontend**: Blazor
- **Infrastructure**: Azure App Service, Azure Key Vault, GitHub Actions
- **Testing**: xUnit, bUnit, Playwright

### Documentation

- [Architecture Documentation](docs/architecture/) - Bounded contexts, ADRs, patterns
- [Ubiquitous Language](docs/_ubiquitous_language/) - Domain terminology
- [GitHub Pages Documentation](https://kippermand.github.io/neba-management/) - Full documentation site

## Implementation Plan

### Public Website

- [x] Champions (History)
- [x] Bowler of the Year
  - [x] Open
  - [x] Senior
  - [x] Super Senior
  - [x] Woman
  - [x] Youth
  - [x] Rookie
- [x] High Average
- [x] High Block
- [x] Organization Bylaws
- [x] Tournament Rules
- [x] Hall of Fame (including HOF indicator for champions)
- [x] Bowling Centers
- [ ] Tournaments
- [ ] Tournament Documents
- [ ] Sponsors
- [ ] News
- [ ] About
- [ ] Stats

### Website Administration

- [ ] Authentication/Authorization

---

### Platform & Operational Improvements (No Particular Order)

(This section covers technical, quality-of-life, and operational improvements. Name subject to change.)

- [x] API Caching
- [x] Health Checks
- [x] SonarCloud Integration
- [x] Background Jobs
- [x] Global Exception Handling
- [ ] Open Telemetry
- [ ] Rate Limiting & Throttling
- [ ] Security Audits & Vulnerability Scanning
- [x] API Documentation (OpenAPI/Swagger)

---

### Documentation Status

- [ ] Administrative Website Manual
- [x] Ubiquitous Language Definitions
  - [x] Awards & Titles domain
- [x] Domain-Driven Design (DDD) Artifacts
  - [x] Bounded Contexts
  - [x] Architecture Decision Records (ADRs)
  - [x] Aggregates (documented in ubiquitous language)
  - [x] Entities & Value Objects (documented in ubiquitous language)
  - [ ] Domain Events (infrastructure not yet implemented)
- [ ] API Reference
- [ ] User Guides & Tutorials

This checklist will be updated as features are completed and new requirements are defined.
