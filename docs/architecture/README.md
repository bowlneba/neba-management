---
layout: default
title: Architecture Documentation
---

# Architecture Documentation

This directory contains architectural documentation for the NEBA Management application, including bounded context definitions, architecture decision records (ADRs), and design patterns.

## Contents

### Bounded Contexts

- [Bounded Contexts](bounded-contexts.md) - Overview of the modular monolith structure, context boundaries, and integration patterns

### Architecture Decision Records (ADRs)

- [ADR-001: ULID and Shadow Key Pattern](adr-001-ulid-shadow-keys.md) - Hybrid identity strategy combining ULIDs for domain identity with shadow integer keys for database relationships

### Infrastructure Patterns

- [Server-Sent Events (SSE)](sse-document-refresh.md) - Real-time document refresh status updates using Server-Sent Events

## Architecture Overview

The NEBA Management application follows a **Modular Monolith** architecture with **Domain-Driven Design (DDD)** tactical patterns:

- **Bounded Contexts**: Modules organized by business domain (Website, Tournaments, Members, Admin)
- **Clean Architecture**: Dependency inversion with Domain → Application → Infrastructure → Endpoints
- **CQRS Pattern**: Command/Query separation for clear use case handling
- **Schema-per-Context**: PostgreSQL schemas isolate context data
- **Hybrid Identity**: ULID domain identifiers + integer database keys

## Key Principles

1. **Module Independence**: Each bounded context is independently developable and testable
2. **Domain Focus**: Each context uses its own ubiquitous language
3. **Pragmatic DDD**: Apply tactical patterns where they add value
4. **Performance First**: Optimize for database efficiency while maintaining domain clarity
5. **Future-Proof**: Design supports evolution to microservices if needed

## Related Documentation

- [Ubiquitous Language](../ubiquitous_language/awards-and-titles.md) - Domain terminology and definitions
- [Main README](../../README.md) - Project overview and structure
- [GitHub Pages Documentation](https://kippermand.github.io/neba-management/) - Full documentation site
