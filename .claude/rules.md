# Project-Specific Instructions

This project contains custom instruction files in `.github/instructions/` that provide domain-specific guidance for code generation, review, and architecture decisions.

## Instruction Files Usage

**IMPORTANT**: Before working on any task, read the relevant instruction files from `.github/instructions/` based on the file types and technologies involved:

### General Instructions (Always Apply)

- [.github/instructions/instructions.instructions.md](.github/instructions/instructions.instructions.md) - Guidelines for instruction files
- [.github/instructions/self-explanatory-code-commenting.instructions.md](.github/instructions/self-explanatory-code-commenting.instructions.md) - Code commenting standards
- [.github/instructions/security-and-owasp.instructions.md](.github/instructions/security-and-owasp.instructions.md) - Security best practices
- [.github/instructions/ai-prompt-engineering-safety-best-practices.instructions.md](.github/instructions/ai-prompt-engineering-safety-best-practices.instructions.md) - AI prompt safety

### .NET/C# Development

- [.github/instructions/csharp.instructions.md](.github/instructions/csharp.instructions.md) - C# coding standards
- [.github/instructions/dotnet-architecture-good-practices.instructions.md](.github/instructions/dotnet-architecture-good-practices.instructions.md) - .NET architecture patterns
- [.github/instructions/aspnet-rest-apis.instructions.md](.github/instructions/aspnet-rest-apis.instructions.md) - ASP.NET REST API development


### Blazor Development

- [.github/instructions/blazor.instructions.md](.github/instructions/blazor.instructions.md) - Blazor component development
- [.github/instructions/ui-notifications.instructions.md](.github/instructions/ui-notifications.instructions.md) - UI notification patterns
- [.github/instructions/ui-loading.instructions.md](.github/instructions/ui-loading.instructions.md) - UI loading state patterns
- [.github/instructions/tailwindcss.instructions.md](.github/instructions/tailwindcss.instructions.md) - TailwindCSS usage

### UI Testing

- [.github/instructions/ui-testing-playwright.instructions.md](.github/instructions/ui-testing-playwright.instructions.md) - Use for Playwright-based UI tests
- [.github/instructions/ui-testing.instructions.md](.github/instructions/ui-testing.instructions.md) - Use for general UI testing guidance

### Infrastructure & DevOps

- [.github/instructions/bicep-code-best-practices.instructions.md](.github/instructions/bicep-code-best-practices.instructions.md) - Azure Bicep templates
	- Note: Follow the `Resource Property Order` guidance in the Bicep instructions to avoid Sonar rule S6975 (consistent ordering: decorators, resource parent/scope/name/location/zones/sku/kind/scale/plan/identity/dependsOn/tags/properties).
	- Note: Follow the `Resource Property Order` guidance in the Bicep instructions to avoid Sonar rule S6975 (consistent ordering: decorators, resource parent/scope/name/location/zones/sku/kind/scale/plan/identity/dependsOn/tags/properties).
	  Any other decorated elements not listed there should be placed before the resource object and after the other decorators. Any other elements not listed there should be placed before the `properties` object for the resource.
- [.github/instructions/github-actions.ci-cd-best-practices.instructions.md](.github/instructions/github-actions.ci-cd-best-practices.instructions.md) - GitHub Actions workflows

### Documentation

- [.github/instructions/markdown.instructions.md](.github/instructions/markdown.instructions.md) - Markdown formatting
- [.github/instructions/prompt.instructions.md](.github/instructions/prompt.instructions.md) - Prompt file standards

## Architecture Decision Records (ADRs)

**CRITICAL**: Before implementing or modifying code that involves architecture, domain design, data modeling, or cross-cutting concerns, you MUST:

1. **Check for relevant ADRs** in `docs/architecture/`
2. **Follow established patterns** documented in ADRs
3. **Understand the reasoning** behind architectural decisions
4. **Maintain consistency** with existing ADR guidance

### Key ADRs to Review

- **[ADR-001: ULID and Shadow Key Pattern](docs/architecture/adr-001-ulid-shadow-keys.md)** - Entity identity strategy
- **[ADR-002: Cache Key Naming Conventions](docs/architecture/adr-002-cache-key-naming-conventions.md)** - Cache key patterns
- **[ADR-003: Cache Tagging Strategy](docs/architecture/adr-003-cache-tagging-strategy.md)** - Cache invalidation patterns
- **[ADR-004: Navigation vs Owned Collections](docs/architecture/adr-004-navigation-vs-owned-collections.md)** - EF Core relationship patterns, querying with owned entities

### When to Review ADRs

- ✅ Creating or modifying domain entities
- ✅ Implementing repository queries
- ✅ Working with EF Core relationships and navigation properties
- ✅ Implementing caching strategies
- ✅ Making decisions about aggregate boundaries
- ✅ Designing many-to-many relationships
- ✅ Projecting data with owned entities (OwnsOne)

### Common ADR Topics

| Topic | ADR Reference |
|-------|---------------|
| Entity identifiers (ULID vs int) | ADR-001 |
| Cache key naming | ADR-002 |
| Cache invalidation | ADR-003 |
| Navigation properties vs ID collections | ADR-004 |
| Owned entities and AsNoTracking() | ADR-004 |
| EF Core query patterns | ADR-004 |

**If an ADR doesn't exist for your architectural decision, consider creating one in `docs/architecture/`.**

## Usage Guidelines

1. **Architecture First**: Review relevant ADRs before implementing architectural changes
2. **Context-Aware Reading**: Read only the instruction files relevant to the current task
3. **Follow Patterns**: Apply the patterns, conventions, and best practices defined in these files
4. **Consistency**: Ensure all code follows the project's established standards
5. **Security First**: Always consider security guidelines from the security instructions
6. **Documentation**: Follow the commenting and documentation standards

## Custom Commands

Use the slash commands in `.claude/commands/` for specialized tasks and code generation patterns.
