---
layout: default
title: ADR-005 - OpenTelemetry with Service Defaults Pattern (Without Aspire AppHost)
---

# ADR-005: OpenTelemetry with Service Defaults Pattern (Without Aspire AppHost)

**Status**: Accepted

**Date**: 2026-01-13

**Context**: Platform & Operational Improvements - OpenTelemetry Implementation

---

## Context and Problem Statement

The NEBA Management application needs comprehensive observability through OpenTelemetry (distributed tracing, metrics, and structured logging) as outlined in the platform improvement roadmap (README:122). We must decide how to implement this capability given our architecture:

**Current Architecture**:

- **Modular monolith** with clean architecture and domain-driven design
- **2 entry points**: `Neba.Api` (backend API) and `Neba.Web.Server` (Blazor frontend)
- **Infrastructure dependencies**: PostgreSQL (database + hybrid cache backend), Hangfire (background jobs), Azure Blob Storage, Azure Key Vault
- **Deployment target**: Azure App Service
- **Service communication**: Simple frontend-to-API via Refit HTTP client
- **Growth plan**: Adding 1 more bounded context (3 total) with shared API/Web hosts

**Key Considerations**:

1. **Observability**: Need distributed tracing across API, frontend, database, PostgreSQL-backed cache, HTTP calls, and background jobs
2. **Consistency**: Both hosts (API + Web) should emit telemetry in the same format
3. **Azure Integration**: Must work seamlessly with Azure Monitor Application Insights
4. **Developer Experience**: Local development should have good telemetry debugging
5. **Simplicity**: Solution should match our architecture scale (2 hosts, not microservices)
6. **Future-proof**: Should support growth to 3 bounded contexts without major refactoring

**.NET Aspire** offers built-in OpenTelemetry integration but comes with orchestration features (AppHost) designed for more complex distributed systems. We need to determine if full Aspire adoption makes sense or if we should use a lighter approach.

## Decision

We will implement OpenTelemetry using the **Service Defaults pattern** (inspired by Aspire) **without adopting the full Aspire AppHost orchestration**.

### What We Are Doing

**Create `Neba.ServiceDefaults` project** that provides:

1. **Centralized OpenTelemetry configuration** (traces, metrics, logs)
2. **Standardized health checks**
3. **HTTP client defaults** with resilience (retry/circuit breaker via Polly)
4. **Service discovery setup** (for future flexibility)
5. **Azure Monitor integration** for production telemetry

Both `Neba.Api` and `Neba.Web.Server` will reference this project and call a single extension method:

```csharp
// Neba.Api/Program.cs and Neba.Web.Server/Program.cs
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults(); // <-- All observability, health checks, resilience
```

### What We Are NOT Doing

**We are explicitly NOT adopting**:

1. **Aspire AppHost** - No orchestration project to manage local service startup
2. **azd deployment tooling** - Keeping existing GitHub Actions → Azure deployment
3. **Aspire resource definitions** - Manual configuration via appsettings remains

## Implementation Architecture

### Clean Architecture Layering

The `Neba.ServiceDefaults` project fits into the **shared infrastructure layer** of our clean architecture, operating at a different level than domain or application concerns:

```text
┌─────────────────────────────────────────────────────────────────┐
│ Entry Points (Hosts)                                             │
│ ├─ Neba.Api              [references ServiceDefaults]           │
│ └─ Neba.Web.Server       [references ServiceDefaults]           │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ ServiceDefaults (Shared Infrastructure - Cross-Cutting)          │
│ ├─ OpenTelemetry configuration (traces, metrics, logs)          │
│ ├─ HTTP resilience (retry, circuit breaker)                     │
│ ├─ Health check defaults                                        │
│ └─ Service discovery setup                                      │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ Bounded Context Layers (Website, Future Contexts)               │
│                                                                  │
│ Endpoints Layer:       Neba.Website.Endpoints                   │
│ Application Layer:     Neba.Website.Application                 │
│ Domain Layer:          Neba.Website.Domain                      │
│ Infrastructure Layer:  Neba.Website.Infrastructure              │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ Shared Kernel (Core Infrastructure)                             │
│ ├─ Neba.Infrastructure   (Database, Hybrid Cache, Hangfire, Azure) │
│ ├─ Neba.Application      (CQRS abstractions)                   │
│ ├─ Neba.Domain           (Domain primitives, base classes)     │
│ └─ Neba.Contracts        (Shared DTOs)                         │
└─────────────────────────────────────────────────────────────────┘
```

### Architectural Position

**ServiceDefaults is NOT part of the core domain or application layers.** It exists as a **horizontal infrastructure concern** that:

1. **Sits above the shared kernel**: While `Neba.Infrastructure` provides domain-agnostic technical capabilities (database, cache, storage), `ServiceDefaults` provides **host-level observability and resilience**.

2. **Referenced only by entry points**: Only the actual executable projects (`Neba.Api`, `Neba.Web.Server`) reference ServiceDefaults. Bounded context layers and shared kernel projects remain unaware of it.

3. **No domain coupling**: ServiceDefaults contains zero domain logic. It's purely technical infrastructure for cross-cutting observability.

4. **Complements Neba.Infrastructure**:
   - `Neba.Infrastructure`: Provides database context, hybrid cache (PostgreSQL-backed), Hangfire, Azure services (domain-supporting infrastructure)
   - `Neba.ServiceDefaults`: Provides telemetry, resilience, health checks (host-supporting infrastructure)

### Project Structure with Architecture Layers

```text
src/
├── backend/
│   ├── Neba.Api/                    # Entry Point (references ServiceDefaults + Infrastructure)
│   ├── Neba.ServiceDefaults/        # ← NEW: Host-level observability infrastructure
│   │
│   ├── Neba.Infrastructure/         # Shared Kernel: Domain-supporting infrastructure
│   ├── Neba.Application/            # Shared Kernel: CQRS abstractions
│   ├── Neba.Domain/                 # Shared Kernel: Domain primitives
│   ├── Neba.Contracts/              # Shared Kernel: DTOs
│   │
│   ├── Neba.Website.Endpoints/      # Website Context: HTTP endpoints
│   ├── Neba.Website.Application/    # Website Context: Use cases
│   ├── Neba.Website.Domain/         # Website Context: Domain model
│   ├── Neba.Website.Infrastructure/ # Website Context: Database, repositories
│   └── Neba.Website.Contracts/      # Website Context: DTOs
│
└── frontend/
    ├── Neba.Web.Server/             # Entry Point (references ServiceDefaults)
    └── Neba.Web.Client/             # Blazor WebAssembly client
```

### Dependency Flow

**Entry Points** → ServiceDefaults → Bounded Contexts → Shared Kernel

```text
Neba.Api (Host)
  ├─ Neba.ServiceDefaults          [Host infrastructure]
  ├─ Neba.Website.Endpoints         [Bounded context]
  │   └─ Neba.Website.Application
  │       └─ Neba.Website.Domain
  └─ Neba.Infrastructure            [Shared kernel]

Neba.Web.Server (Host)
  ├─ Neba.ServiceDefaults          [Host infrastructure]
  ├─ Neba.Web.Server.Services       [Frontend-specific logic]
  └─ (References Neba.Api via Refit)
```

### Why This Layering Matters

1. **Clean Separation**: Domain/application layers never reference ServiceDefaults. They remain pure business logic.

2. **Reusability**: When we add a 3rd bounded context, ServiceDefaults provides the same observability without duplication.

3. **Testability**: Unit and integration tests don't need ServiceDefaults. Only the actual host projects that run in production/development use it.

4. **Infrastructure Segregation**:
   - **Neba.Infrastructure**: "What does the domain need?" (database, cache, storage)
   - **Neba.ServiceDefaults**: "What does the host need?" (telemetry, resilience, health)

5. **Future AppHost Migration**: If we adopt Aspire AppHost later, ServiceDefaults becomes the exact project Aspire expects (it's the standard pattern).

### Neba.ServiceDefaults Project

**Package Dependencies**:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <!-- Core OpenTelemetry -->
    <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" />
    <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Runtime" />

    <!-- Azure Monitor for production -->
    <PackageReference Include="Azure.Monitor.OpenTelemetry.AspNetCore" />

    <!-- Resilience and service discovery -->
    <PackageReference Include="Microsoft.Extensions.Http.Resilience" />
    <PackageReference Include="Microsoft.Extensions.ServiceDiscovery" />
  </ItemGroup>
</Project>
```

**Core Extension Method**:

```csharp
// ServiceDefaultsExtensions.cs
namespace Neba.ServiceDefaults;

public static class ServiceDefaultsExtensions
{
    public static IHostApplicationBuilder AddServiceDefaults(
        this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Add standard resilience handler (retry, circuit breaker, timeout)
            http.AddStandardResilienceHandler();

            // Add service discovery support
            http.AddServiceDiscovery();
        });

        return builder;
    }
}
```

### OpenTelemetry Configuration

**Instrumentation Setup**:

```csharp
private static IHostApplicationBuilder ConfigureOpenTelemetry(
    this IHostApplicationBuilder builder)
{
    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
    });

    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation();
        })
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.EnrichWithIDbCommand = (activity, command) =>
                    {
                        // Extract schema from connection string or command text
                        // This will be enriched by each bounded context's DbContext
                        var commandText = command.CommandText;
                        if (!string.IsNullOrEmpty(commandText))
                        {
                            // Simple regex to extract schema from "FROM schema.table" or "INSERT INTO schema.table"
                            var schemaMatch = System.Text.RegularExpressions.Regex.Match(
                                commandText,
                                @"(?:FROM|INTO|UPDATE|DELETE\s+FROM)\s+([a-zA-Z_][a-zA-Z0-9_]*)\.([a-zA-Z_][a-zA-Z0-9_]*)",
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                            if (schemaMatch.Success)
                            {
                                activity.SetTag("db.schema", schemaMatch.Groups[1].Value);
                                activity.SetTag("db.table", schemaMatch.Groups[2].Value);
                            }
                        }
                    };
                })
                .AddSource("Neba.*")              // Custom activity sources from all contexts
                .AddSource("Hangfire.*")          // Background jobs
                .AddSource("Azure.*");            // Azure SDK calls
        });

    builder.AddOpenTelemetryExporters();

    return builder;
}
```

**Environment-Specific Exporters**:

```csharp
private static IHostApplicationBuilder AddOpenTelemetryExporters(
    this IHostApplicationBuilder builder)
{
    // Production/Staging: Export to Azure Monitor Application Insights
    if (!builder.Environment.IsDevelopment())
    {
        builder.Services.AddOpenTelemetry()
            .UseAzureMonitor(options =>
            {
                // Connection string from environment or Key Vault
                options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
            });
    }
    else
    {
        // Development: Export to Aspire Dashboard via OTLP
        // Dashboard runs in Docker Compose alongside PostgreSQL, Redis, Azurite
        builder.Services.AddOpenTelemetry()
            .UseOtlpExporter(options =>
            {
                // Aspire Dashboard OTLP endpoint (default port)
                options.Endpoint = new Uri(
                    builder.Configuration["OpenTelemetry:OtlpEndpoint"]
                    ?? "http://localhost:4317");
            });
    }

    return builder;
}
```

**Default Health Checks**:

```csharp
private static IHostApplicationBuilder AddDefaultHealthChecks(
    this IHostApplicationBuilder builder)
{
    builder.Services.AddHealthChecks()
        // Base health check for the app
        .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

    return builder;
}
```

### Integration in Host Projects

**Neba.Api/Program.cs**:

```csharp
using Neba.Api.ErrorHandling;
using Neba.Api.OpenApi;
using Neba.Infrastructure;
using Neba.ServiceDefaults; // ← Add reference
using Neba.Website.Application;
using Neba.Website.Endpoints;
using Neba.Website.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add observability, resilience, health checks
builder.AddServiceDefaults(); // ← Single call for all defaults

// Existing configuration...
builder.Services.ConfigureOpenApi();
builder.Services.AddInfrastructure(builder.Configuration, [
    WebsiteApplicationAssemblyReference.Assembly
]);
// ... rest of configuration
```

**Neba.Web.Server/Program.cs**:

```csharp
using Azure.Identity;
using Neba.ServiceDefaults; // ← Add reference
using Neba.Web.Server;
using Neba.Web.Server.BackgroundJobs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add observability, resilience, health checks
builder.AddServiceDefaults(); // ← Same call, consistent telemetry

// Existing configuration...
builder.Services.AddOptions<NebaApiConfiguration>()
    .Bind(builder.Configuration.GetSection("NebaApi"));
// ... rest of configuration
```

### Custom Activity Sources and Schema Enrichment

**Bounded Context Activity Sources**:

Each bounded context creates its own `ActivitySource` for custom tracing:

```csharp
// Neba.Website.Application/Telemetry/WebsiteTelemetry.cs
namespace Neba.Website.Application.Telemetry;

public static class WebsiteTelemetry
{
    public static readonly ActivitySource ActivitySource =
        new("Neba.Website.Application", "1.0.0");

    public const string SchemaName = "website";
}

// Neba.App.Application/Telemetry/AppTelemetry.cs (future)
namespace Neba.App.Application.Telemetry;

public static class AppTelemetry
{
    public static readonly ActivitySource ActivitySource =
        new("Neba.App.Application", "1.0.0");

    public const string SchemaName = "app";
}

// Neba.Auth.Application/Telemetry/AuthTelemetry.cs (future)
namespace Neba.Auth.Application.Telemetry;

public static class AuthTelemetry
{
    public static readonly ActivitySource ActivitySource =
        new("Neba.Auth.Application", "1.0.0");

    public const string SchemaName = "auth";
}
```

**Schema Enrichment in DbContext**:

Each bounded context's `DbContext` enriches telemetry with its schema:

```csharp
// Neba.Website.Infrastructure/Database/WebsiteDbContext.cs
public class WebsiteDbContext : DbContext
{
    public WebsiteDbContext(DbContextOptions<WebsiteDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Register command interceptor to enrich telemetry with schema
        optionsBuilder.AddInterceptors(new SchemaEnrichmentInterceptor(WebsiteTelemetry.SchemaName));
    }

    // ... entity configurations
}

// Neba.Infrastructure/Database/Interceptors/SchemaEnrichmentInterceptor.cs
public class SchemaEnrichmentInterceptor : DbCommandInterceptor
{
    private readonly string _schemaName;

    public SchemaEnrichmentInterceptor(string schemaName)
    {
        _schemaName = schemaName;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        EnrichActivity(command);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        EnrichActivity(command);
        return base.ReaderExecuted(command, eventData, result);
    }

    private void EnrichActivity(DbCommand command)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("db.context.schema", _schemaName);

            // Also extract table name if available
            var commandText = command.CommandText;
            if (!string.IsNullOrEmpty(commandText))
            {
                var tableMatch = System.Text.RegularExpressions.Regex.Match(
                    commandText,
                    $@"{_schemaName}\.([a-zA-Z_][a-zA-Z0-9_]*)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (tableMatch.Success)
                {
                    activity.SetTag("db.table", tableMatch.Groups[1].Value);
                }
            }
        }
    }
}
```

**Usage in Command/Query Handlers**:

```csharp
// Neba.Website.Application/Queries/GetTournamentChampionsQueryHandler.cs
public class GetTournamentChampionsQueryHandler
    : IQueryHandler<GetTournamentChampionsQuery, TournamentChampionsViewModel>
{
    private readonly WebsiteDbContext _context;

    public GetTournamentChampionsQueryHandler(WebsiteDbContext context)
    {
        _context = context;
    }

    public async Task<TournamentChampionsViewModel> Handle(
        GetTournamentChampionsQuery query,
        CancellationToken cancellationToken)
    {
        using var activity = WebsiteTelemetry.ActivitySource
            .StartActivity("GetTournamentChampions");

        activity?.SetTag("tournament.type", query.TournamentType);
        activity?.SetTag("tournament.year", query.Year);
        activity?.SetTag("bounded.context", "website");

        // Database query automatically enriched with "db.context.schema": "website"
        var champions = await _context.Titles
            .Where(t => t.TournamentType == query.TournamentType && t.Year == query.Year)
            .ToListAsync(cancellationToken);

        // ... rest of handler logic

        return result;
    }
}
```

**Resulting Trace Tags**:

When viewing traces in Aspire Dashboard or Azure Monitor, you'll see:

```text
Span: GetTournamentChampions
  - tournament.type: "singles"
  - tournament.year: 2024
  - bounded.context: "website"

  Child Span: SQL Query
    - db.context.schema: "website"
    - db.table: "titles"
    - db.statement: "SELECT ... FROM website.titles WHERE ..."
```

### Multi-Schema Telemetry Strategy

**Key Design Principles**:

1. **Schema-per-Context**: Each bounded context owns its schema name (`website`, `app`, `auth`)
2. **Decentralized Enrichment**: Bounded contexts enrich their own telemetry via `DbCommandInterceptor`
3. **No Central Configuration**: ServiceDefaults doesn't need to know about specific schemas
4. **ActivitySource Pattern**: Each context has its own `ActivitySource` for custom tracing
5. **Automatic Discovery**: OpenTelemetry's `.AddSource("Neba.*")` captures all bounded context sources

**Benefits of This Approach**:

- ✅ **Scalable**: Adding new bounded contexts (app, auth) requires no changes to ServiceDefaults
- ✅ **Clean Separation**: Each context manages its own telemetry enrichment
- ✅ **Queryable**: Filter traces by schema in Aspire Dashboard or Azure Monitor (e.g., `db.context.schema == "website"`)
- ✅ **Bounded Context Isolation**: Website context can't accidentally tag traces with "app" schema
- ✅ **DDD Alignment**: Schema boundaries align with bounded context boundaries

**Example: Adding Auth Context Later**:

1. Create `Neba.Auth.Application/Telemetry/AuthTelemetry.cs` with `SchemaName = "auth"`
2. Create `Neba.Auth.Infrastructure/Database/AuthDbContext.cs` with `SchemaEnrichmentInterceptor("auth")`
3. No changes needed to `Neba.ServiceDefaults` - already captures `Neba.*` activity sources
4. Traces automatically tagged with `"db.context.schema": "auth"`

**Telemetry Flow Diagram**:

```text
┌─────────────────────────────────────────────────────────────────────┐
│ Neba.ServiceDefaults (Host-Level)                                   │
│ • AddSource("Neba.*") - captures all bounded context activity       │
│ • EF Core instrumentation with generic enrichment                   │
│ • No knowledge of specific schemas (website/app/auth)               │
└─────────────────────────────────────────────────────────────────────┘
                               ↓ discovers
┌─────────────────────────────────────────────────────────────────────┐
│ Bounded Context: Website                                            │
│                                                                      │
│ WebsiteTelemetry.ActivitySource ("Neba.Website.Application")       │
│ WebsiteTelemetry.SchemaName = "website"                            │
│                                                                      │
│ WebsiteDbContext + SchemaEnrichmentInterceptor("website")          │
│ ↳ Enriches all DB traces with: db.context.schema = "website"       │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│ Bounded Context: App (Future)                                       │
│                                                                      │
│ AppTelemetry.ActivitySource ("Neba.App.Application")               │
│ AppTelemetry.SchemaName = "app"                                    │
│                                                                      │
│ AppDbContext + SchemaEnrichmentInterceptor("app")                  │
│ ↳ Enriches all DB traces with: db.context.schema = "app"           │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│ Bounded Context: Auth (Future)                                      │
│                                                                      │
│ AuthTelemetry.ActivitySource ("Neba.Auth.Application")             │
│ AuthTelemetry.SchemaName = "auth"                                  │
│                                                                      │
│ AuthDbContext + SchemaEnrichmentInterceptor("auth")                │
│ ↳ Enriches all DB traces with: db.context.schema = "auth"          │
└─────────────────────────────────────────────────────────────────────┘
                               ↓ all export to
┌─────────────────────────────────────────────────────────────────────┐
│ Development: Aspire Dashboard (OTLP)                                │
│ • Filter by: db.context.schema == "website"                         │
│ • Filter by: bounded.context == "app"                               │
│ • View distributed traces across all contexts                       │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│ Production: Azure Monitor Application Insights                      │
│ • Kusto query: traces | where customDimensions.db_context_schema    │
│ • Application Map shows all bounded contexts                        │
│ • End-to-end transaction tracking across schemas                    │
└─────────────────────────────────────────────────────────────────────┘
```

## Rationale

### Why Service Defaults Pattern?

**Benefits**:

1. **Consistency**: Both API and Web hosts have identical telemetry configuration
2. **Single Source of Truth**: All observability config in one project
3. **Reusability**: Easy to add 3rd bounded context later - just call `AddServiceDefaults()`
4. **Schema-Agnostic**: ServiceDefaults doesn't hardcode bounded context schemas
5. **Aspire-compatible**: If we later adopt full Aspire, this pattern is a seamless migration path
6. **Low overhead**: No orchestration complexity for 2-service architecture

### Why Aspire Dashboard (Without AppHost)?

**Benefits of Using Aspire Dashboard in Docker Compose**:

1. **Best-in-class local observability**: Rich UI for traces, metrics, logs without AppHost complexity
2. **Matches production tooling**: Same OpenTelemetry standards used locally and in Azure Monitor
3. **Zero application configuration**: Just export OTLP; dashboard auto-discovers services
4. **Lightweight**: Single Docker container alongside existing PostgreSQL/Azurite
5. **No orchestration overhead**: Don't need AppHost to get the excellent dashboard
6. **Team productivity**: Developers can debug distributed traces visually during development

**This gives us the best Aspire feature (observability dashboard) without the orchestration complexity we don't need.**

### Why NOT Full Aspire AppHost?

**Reasons to Skip AppHost**:

1. **Overkill for 2 services**: AppHost is designed for complex microservices orchestration
2. **Current setup works**: appsettings.Development.json handles local config fine
3. **No local dev pain**: Starting 2 services manually is manageable
4. **CI/CD stability**: GitHub Actions → Azure deployment is proven; azd tooling is still maturing
5. **TestContainers already works**: Integration tests use TestContainers for PostgreSQL/Azurite
6. **Modular monolith stability**: Not splitting into microservices, so orchestration is unnecessary

**When We Might Reconsider Aspire AppHost**:

- Service count grows beyond 5
- Complex inter-service communication patterns emerge
- Local development setup becomes painful
- Team explicitly requests unified orchestration

### Why This Beats Alternatives

| Approach | Pros | Cons |
| ---------- | ------ | ------ |
| **ServiceDefaults + Aspire Dashboard (Chosen)** | ✅ Best local observability (Aspire Dashboard)  ✅ Production-ready (Azure Monitor)  ✅ Low complexity  ✅ Aspire-compatible  ✅ No orchestration overhead | ⚠️ Manual OTEL config  ⚠️ Docker Compose dependency |
| **Full Aspire AppHost** | ✅ Full orchestration  ✅ Dashboard included  ✅ Auto service discovery | ❌ Overkill for 2 services  ❌ CI/CD rewrite needed  ❌ Adds unnecessary complexity |
| **Manual OTEL in Each Host** | ✅ Simple initially | ❌ Configuration drift  ❌ Duplication  ❌ Hard to maintain consistency  ❌ No local dashboard |
| **ServiceDefaults without Dashboard** | ✅ Consistent config  ✅ Azure Monitor works | ❌ Poor local dev experience  ❌ Console-only debugging |
| **No Observability** | ✅ Zero effort | ❌ Debugging pain  ❌ No production insights  ❌ Not acceptable |

## Consequences

### Positive

- **Excellent Local Observability**: Aspire Dashboard provides rich UI for traces, metrics, logs during development
- **Production-Ready Monitoring**: Azure Monitor Application Insights for staging/production environments
- **Comprehensive Telemetry**: Distributed tracing across API, frontend, database, cache, HTTP clients, and Hangfire
- **Consistent Configuration**: Both hosts emit traces/metrics in identical format via ServiceDefaults
- **Resilient HTTP**: Refit client (Neba.Web.Server → Neba.Api) gets automatic retry/circuit breaker
- **Low Complexity**: No AppHost orchestration overhead for our 2-service architecture
- **Developer Productivity**: Visual debugging of distributed traces, no more console log hunting
- **Infrastructure Integration**: Aspire Dashboard runs in existing Docker Compose alongside PostgreSQL/Azurite
- **Future-Proof**: Pattern supports growth to 3rd bounded context without refactoring
- **Aspire Migration Path**: If needed later, ServiceDefaults project is already Aspire-compatible

### Negative

- **Manual Configuration**: Must configure OTEL instrumentation explicitly (vs. AppHost automatic setup)
- **Docker Compose Dependency**: Local development requires Docker Compose running for Aspire Dashboard
- **Learning Curve**: Team must understand OpenTelemetry concepts (activity sources, spans, metrics)

### Neutral

- **Local Development**: Existing appsettings.Development.json pattern continues (no change)
- **CI/CD Pipeline**: GitHub Actions remain unchanged (no azd adoption)
- **Service Discovery**: Configured but not heavily used (simple API base URL via configuration)

## Implementation Steps

### Phase 1: Create ServiceDefaults Project

1. Add `src/backend/Neba.ServiceDefaults/Neba.ServiceDefaults.csproj`
2. Add OpenTelemetry NuGet packages
3. Implement `ServiceDefaultsExtensions.AddServiceDefaults()`
4. Implement `ConfigureOpenTelemetry()` private method
5. Implement environment-specific exporter configuration

### Phase 2: Integrate with Hosts

1. Add project reference from `Neba.Api` to `Neba.ServiceDefaults`
2. Add project reference from `Neba.Web.Server` to `Neba.ServiceDefaults`
3. Call `builder.AddServiceDefaults()` in both `Program.cs` files (after `CreateBuilder`)
4. Remove any duplicate health check/resilience configuration

### Phase 3: Configure Docker Compose for Aspire Dashboard

**Create/Update docker-compose.yml**:

Add Aspire Dashboard service to existing Docker Compose setup (alongside PostgreSQL, Azurite):

```yaml
services:
  # Existing services (postgres, azurite)...

  aspire-dashboard:
    image: mcr.microsoft.com/dotnet/aspire-dashboard:latest
    container_name: neba-aspire-dashboard
    ports:
      - "18888:18888"  # Dashboard UI
      - "4317:18889"   # OTLP gRPC endpoint
      - "4318:18890"   # OTLP HTTP endpoint
    environment:
      - DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true
      - ASPNETCORE_URLS=http://+:18888
    restart: unless-stopped
    networks:
      - neba-network

networks:
  neba-network:
    driver: bridge
```

**Configure OTLP Endpoint**:

Add to `appsettings.Development.json` (both API and Web Server):

```json
{
  "OpenTelemetry": {
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

### Phase 4: Configure Production Exporters

**Production**:

- Add Application Insights connection string to Azure Key Vault
- Configure `ApplicationInsights:ConnectionString` in production config
- No OTLP endpoint needed (uses Azure Monitor directly)

### Phase 5: Add Custom Activity Sources

1. Create static `ActivitySource` in application layer assemblies
2. Register sources in `ConfigureOpenTelemetry()` via `.AddSource("Neba.*")`
3. Add tracing to command/query handlers where domain insights are valuable

### Phase 6: Validate Telemetry

**Local Development**:

1. Start Docker Compose: `docker-compose up -d`
2. Verify Aspire Dashboard is running: http://localhost:18888
3. Start `Neba.Api` and `Neba.Web.Server`
4. Navigate the application to generate telemetry
5. View traces in Aspire Dashboard:
   - **Traces**: See distributed traces across Web Server → API → PostgreSQL
   - **Metrics**: View HTTP request rates, durations, error rates
   - **Logs**: Structured logs with trace correlation
   - **Resources**: See all detected services (API, Web Server)

**Azure Production**:

1. Deploy to Azure App Service
2. Navigate to Application Insights in Azure Portal
3. Verify traces/metrics appear in Application Insights
4. Validate end-to-end distributed traces (Web Server → API → PostgreSQL)
5. Set up Azure Monitor workbooks for custom dashboards

## Configuration Examples

### appsettings.Development.json (Local Development with Aspire Dashboard)

Add to both `Neba.Api/appsettings.Development.json` and `Neba.Web.Server/appsettings.Development.json`:

```json
{
  "OpenTelemetry": {
    "OtlpEndpoint": "http://localhost:4317"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "OpenTelemetry": "Information"
    }
  }
}
```

### appsettings.json (Production)

```json
{
  "ApplicationInsights": {
    "ConnectionString": ""  // Populated from Key Vault
  }
}
```

### Azure Key Vault Secrets

Add secret: `ApplicationInsights--ConnectionString` with value from Azure Portal.

### Docker Compose Configuration

Complete `docker-compose.yml` example showing Aspire Dashboard integration:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:17
    container_name: neba-postgres
    environment:
      POSTGRES_DB: bowlneba
      POSTGRES_USER: neba
      POSTGRES_PASSWORD: neba
    ports:
      - "19630:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - neba-network

  azurite:
    image: mcr.microsoft.com/azure-storage/azurite
    container_name: neba-azurite
    ports:
      - "19631:10000"  # Blob service
      - "19632:10001"  # Queue service
      - "19633:10002"  # Table service
    networks:
      - neba-network

  aspire-dashboard:
    image: mcr.microsoft.com/dotnet/aspire-dashboard:latest
    container_name: neba-aspire-dashboard
    ports:
      - "18888:18888"  # Dashboard UI (http://localhost:18888)
      - "4317:18889"   # OTLP gRPC endpoint
      - "4318:18890"   # OTLP HTTP endpoint (not used, but available)
    environment:
      - DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true
      - ASPNETCORE_URLS=http://+:18888
      - DASHBOARD__OTLP__GRPCENDPOINT=http://+:18889
      - DASHBOARD__OTLP__HTTPENDPOINT=http://+:18890
    restart: unless-stopped
    networks:
      - neba-network

volumes:
  postgres_data:

networks:
  neba-network:
    driver: bridge
```

### Local Development Workflow

**Starting Infrastructure**:

```bash
# Start all infrastructure services including Aspire Dashboard
docker-compose up -d

# Verify services are running
docker-compose ps

# View Aspire Dashboard
open http://localhost:18888
```

**Running Applications**:

```bash
# Terminal 1: Start API
cd src/backend/Neba.Api
dotnet run

# Terminal 2: Start Web Server
cd src/frontend/Neba.Web.Server
dotnet run
```

**Viewing Telemetry**:

1. Navigate to http://localhost:18888
2. Dashboard automatically discovers services emitting OTLP telemetry
3. View:
   - **Resources**: See Neba.Api and Neba.Web.Server listed
   - **Traces**: Click any trace to see distributed span waterfall
   - **Metrics**: View HTTP request rates, durations, error rates
   - **Structured Logs**: View logs with trace correlation
   - **Console Logs**: View stdout/stderr from services

**Stopping Infrastructure**:

```bash
# Stop all services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v
```

## Monitoring and Observability

### Observability by Environment

**Local Development (Aspire Dashboard)**:

- **URL**: http://localhost:18888
- **Protocol**: OTLP over gRPC (port 4317)
- **Features**:
  - Real-time trace visualization with span waterfall
  - Live metrics dashboard (requests/sec, duration, errors)
  - Structured logs with trace correlation
  - Resource discovery (automatically detects API and Web Server)
  - Console log streaming from both services
  - No authentication required (dev mode)
  - Zero configuration (auto-discovers OTLP exporters)

**Production/Staging (Azure Monitor Application Insights)**:

- **Service**: Azure Application Insights
- **Protocol**: Azure Monitor native protocol
- **Features**:
  - Distributed application map
  - End-to-end transaction details
  - Performance metrics and anomaly detection
  - Custom dashboards and workbooks
  - Alerting and notifications
  - Long-term retention and analytics
  - Integration with Azure services

### What Will Be Traced

**Automatic Instrumentation**:

- HTTP requests (ASP.NET Core)
- HTTP client calls (Refit: Neba.Web.Server → Neba.Api)
- Database queries (EF Core: PostgreSQL)
- Cache operations (HybridCache with PostgreSQL backend)
- Runtime metrics (GC, thread pool, exceptions)

**Custom Instrumentation** (via Activity Sources):

- Command/query handlers
- Domain operations
- Background job execution (Hangfire)
- Google Docs fetching
- Azure Blob Storage operations

### Local Development Queries (Aspire Dashboard)

**Using the Dashboard UI**:

1. **Traces Tab**: View all traces, filter by service, status, duration
2. **Metrics Tab**: Real-time charts for HTTP requests, database queries
3. **Structured Logs Tab**: Filter logs by service, level, trace ID
4. **Resources Tab**: See all discovered services and their health
5. **Console Logs Tab**: Live stdout/stderr from Neba.Api and Neba.Web.Server

**Example Trace Flow** (Web Server → API → Database):

```text
[Neba.Web.Server] HTTP GET /tournaments/champions
  └─ [HTTP Client] GET http://localhost:5150/api/tournaments/champions
      └─ [Neba.Api] HTTP GET /api/tournaments/champions
          └─ [EF Core] SELECT * FROM website.titles WHERE...
```

### Azure Monitor Queries (Production)

**Find slow API requests**:

```kusto
requests
| where duration > 1000  // > 1 second
| project timestamp, name, url, duration, resultCode
| order by duration desc
```

**Trace dependency calls by bounded context**:

```kusto
dependencies
| where type == "SQL"
| extend schema = tostring(customDimensions.db_context_schema)
| where schema == "website"  // or "app", "auth"
| project timestamp, name, data, duration, success, schema
| order by duration desc
```

**Distributed trace for single request**:

```kusto
union requests, dependencies
| where operation_Id == "<operation_id>"
| project timestamp, itemType, name, duration
| order by timestamp
```

**Count queries by bounded context schema**:

```kusto
dependencies
| where type == "SQL"
| extend schema = tostring(customDimensions.db_context_schema)
| summarize count() by schema
| order by count_ desc
```

**Find cross-context operations** (queries touching multiple schemas in one request):

```kusto
dependencies
| where type == "SQL"
| extend schema = tostring(customDimensions.db_context_schema)
| summarize schemas = make_set(schema) by operation_Id
| where array_length(schemas) > 1
| project operation_Id, schemas
```

**Performance by bounded context**:

```kusto
dependencies
| where type == "SQL"
| extend schema = tostring(customDimensions.db_context_schema)
| summarize
    avg_duration = avg(duration),
    p95_duration = percentile(duration, 95),
    count = count()
    by schema
| order by p95_duration desc
```

## Alternatives Considered

### Alternative 1: Full Aspire with AppHost

**Approach**: Adopt complete Aspire stack including orchestration.

**Rejected because**:

- Adds orchestration complexity unnecessary for 2 services
- Requires rewriting CI/CD pipeline for azd
- Current local dev experience is acceptable
- AppHost designed for microservices, not modular monoliths

### Alternative 2: Manual OpenTelemetry in Each Host

**Approach**: Configure OTEL directly in `Neba.Api` and `Neba.Web.Server` Program.cs files.

**Rejected because**:

- Configuration duplication across 2 hosts (soon to be 3 contexts)
- Risk of telemetry format inconsistency
- Harder to maintain and update OTEL configuration
- No centralized resilience configuration

### Alternative 3: Third-Party Observability (Seq, Elastic, etc.)

**Approach**: Use commercial/OSS observability platform instead of OpenTelemetry + Azure Monitor.

**Rejected because**:

- Additional cost and infrastructure to manage
- Azure Monitor already available with Azure App Service
- OpenTelemetry is vendor-neutral standard (can switch later)
- Team already familiar with Azure ecosystem

### Alternative 4: Application Insights SDK Directly

**Approach**: Use legacy Application Insights SDK instead of OpenTelemetry.

**Rejected because**:

- OpenTelemetry is the modern standard (Microsoft recommends it)
- OTEL provides vendor neutrality
- Better instrumentation coverage (EF Core, HTTP, runtime)
- Future-proof for multi-cloud scenarios

## Related Decisions

- [Platform Improvement Roadmap](../../README.md#platform--operational-improvements-no-particular-order) - OpenTelemetry requirement
- [Bounded Contexts](bounded-contexts.md) - Multi-context architecture this pattern supports
- ADR-001: ULID and Shadow Key Pattern - Database performance considerations for tracing

## References

- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/languages/net/)
- [Azure Monitor OpenTelemetry](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable?tabs=aspnetcore)
- [.NET Aspire Service Defaults](https://learn.microsoft.com/dotnet/aspire/fundamentals/service-defaults)
- [Microsoft.Extensions.Http.Resilience](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
- [OpenTelemetry Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/)

## Success Metrics

### Technical Metrics

- ✅ Both `Neba.Api` and `Neba.Web.Server` emit telemetry to Application Insights
- ✅ Distributed traces span Web Server → API → PostgreSQL
- ✅ EF Core queries appear as SQL dependencies with statement text
- ✅ Refit HTTP calls automatically traced with retry attempts visible
- ✅ Hangfire background jobs emit custom activity spans

### Operational Metrics

- ✅ P95 latency for API endpoints visible in Azure Monitor
- ✅ Database query performance tracked per endpoint
- ✅ Cache hit/miss rates measurable
- ✅ Error rates and exceptions correlated to traces
- ✅ End-to-end request duration from browser to database

### Developer Experience

- ✅ Adding new bounded context requires only `builder.AddServiceDefaults()` call
- ✅ Custom tracing added via `ActivitySource.StartActivity()` pattern
- ✅ Local development logs show trace/span IDs for correlation
- ✅ Production debugging uses Application Insights distributed trace view

## Future Enhancements

### Potential Additions

1. **Metrics Dashboard**: Custom Azure Monitor workbooks for NEBA-specific metrics
2. **Alerting**: Configure alerts for P95 latency, error rates, dependency failures
3. **Log Correlation**: Structured logging with trace/span correlation
4. **Custom Metrics**: Domain-specific metrics (tournaments processed, cache effectiveness)
5. **Baggage Propagation**: Pass domain context (user ID, tenant) through traces

### When to Revisit This Decision

- **Service count exceeds 5**: Consider Aspire AppHost for orchestration
- **Azure Monitor limitations**: Consider alternative observability platforms
- **Multi-cloud deployment**: Leverage OpenTelemetry vendor neutrality
- **Complex service mesh**: Re-evaluate orchestration and service discovery needs

---

## Summary

This ADR documents our decision to implement OpenTelemetry using a **hybrid approach that extracts maximum value from .NET Aspire without the orchestration overhead**:

### What We're Getting

- ✅ **ServiceDefaults pattern** for consistent observability across hosts
- ✅ **Aspire Dashboard** running in Docker Compose for local development
- ✅ **Azure Monitor** for production observability
- ✅ **OpenTelemetry standard** ensuring vendor neutrality

### What We're Avoiding

- ❌ **Aspire AppHost** orchestration (unnecessary for 2 services)
- ❌ **azd deployment** complexity (keeping proven CI/CD)
- ❌ **Service mesh overhead** (simple HTTP communication is fine)

### The Result

A pragmatic, production-ready observability solution that:
1. Gives developers excellent local debugging via Aspire Dashboard
2. Provides enterprise monitoring in Azure Monitor for production
3. Maintains clean architecture with host-level infrastructure separation
4. Scales gracefully as we add the 3rd bounded context
5. Keeps the door open for full Aspire adoption if service count explodes

**This is the right observability solution for a modular monolith deployed to Azure.**

---

**Decision Made By**: Development Team
**Approved By**: Technical Lead
**Implementation Target**: Q1 2026
