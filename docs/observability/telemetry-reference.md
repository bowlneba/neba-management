---
layout: default
title: Telemetry Reference
---

This document provides a comprehensive reference of all OpenTelemetry instrumentation in the NEBA Management application. It describes metrics, traces, and logs across all components and bounded contexts.

> **Last Updated**: January 2026
> **Architecture**: See [ADR-0050: OpenTelemetry](../architecture/adr-0050-opentelemetry-without-aspire-apphost.md) for implementation details

---

## Overview

The NEBA Management application uses OpenTelemetry for distributed tracing, metrics collection, and structured logging. Telemetry is automatically exported to:

- **Development**: Aspire Dashboard (OTLP endpoint at `http://localhost:18889`)
- **Production**: Azure Monitor Application Insights

### Telemetry Naming Conventions

All custom telemetry follows OpenTelemetry semantic conventions:

**Metrics**: `neba.{component}.{resource}.{action}`

- Examples: `neba.cache.hits`, `neba.database.query.duration`

**Activity Sources**: `Neba.{Component}.{Subcomponent}`

- Examples: `Neba.Handlers`, `Neba.Web.Server.JavaScript`

**Traces**: `{component}.{action}`

- Examples: `query.GetBylawsQuery`, `javascript.page.performance`

---

## Automatic Instrumentation

The following instrumentation is configured automatically via `Neba.ServiceDefaults`:

### HTTP Instrumentation

- **ASP.NET Core**: All HTTP requests/responses
- **HTTP Client**: All outbound HTTP calls (including Refit API calls)
- **Filters**: Health check endpoints (`/health`, `/alive`) excluded from traces

### Database Instrumentation

- **Entity Framework Core**: All database queries with statement text
- **PostgreSQL**: Connection pooling, command execution
- **Enrichment**: Schema name, command type, parameter count

### Runtime Instrumentation

- **GC**: Garbage collection metrics
- **Thread Pool**: Thread pool usage
- **Exceptions**: Unhandled exception tracking

### Azure SDK Instrumentation

- **Azure Blob Storage**: Blob operations (upload, download, delete)
- **Activity Source**: `Azure.Storage.Blobs`

---

## Database Telemetry

**Meter**: `Neba.Database`
**Activity Source**: `Neba.Database`
**Location**: `Neba.Infrastructure/Database/SlowQueryInterceptor.cs`

### Metrics

#### `neba.database.query.duration`

- **Type**: Histogram
- **Unit**: milliseconds
- **Description**: Database query execution duration
- **Tags**:
  - `db.operation`: Query operation type
  - `db.statement`: SQL statement (truncated)
  - `db.command_type`: Text, StoredProcedure, etc.

#### `neba.database.query.slow`

- **Type**: Counter
- **Description**: Number of slow database queries (threshold: 1000ms)
- **Tags**:
  - `db.operation`: Query operation type
  - `db.statement`: SQL statement (truncated)

### Traces

#### `db.slow_query`

- **Created When**: Query execution exceeds 1000ms threshold
- **Tags**:
  - `db.operation`: Query operation type
  - `db.statement`: Full SQL statement
  - `db.command_type`: Command type
  - `db.duration_ms`: Actual duration
- **Status**: Warning level

### Usage Example

```csharp
// Automatic - no code required
var tournaments = await _dbContext.Tournaments
    .Where(t => t.Year == 2024)
    .ToListAsync();

// Metrics recorded automatically:
// - neba.database.query.duration: 45ms
// If slow (>1000ms):
// - neba.database.query.slow: +1
// - Activity span created with full SQL
```

---

## Cache Telemetry

**Meter**: `Neba.Cache`
**Location**: `Neba.Infrastructure/Caching/CacheMetrics.cs`

### Metrics

#### `neba.cache.hits`

- **Type**: Counter
- **Description**: Number of cache hits
- **Tags**:
  - `cache.key`: Cache key that was hit
  - `query.type`: Query type being cached
  - `cache.hit`: true

#### `neba.cache.misses`

- **Type**: Counter
- **Description**: Number of cache misses
- **Tags**:
  - `cache.key`: Cache key that was missed
  - `query.type`: Query type being cached
  - `cache.hit`: false

#### `neba.cache.operation.duration`

- **Type**: Histogram
- **Unit**: milliseconds
- **Description**: Duration of cache operations (get, set)
- **Tags**:
  - `cache.key`: Cache key involved
  - `query.type`: Query type being cached
  - `cache.hit`: Whether operation was a hit

### Usage Example

```csharp
// In CachedQueryHandlerDecorator
var cacheKey = CacheKeys.Build(queryType, ...parameters);

// On cache hit:
CacheMetrics.RecordCacheHit(cacheKey, queryType);

// On cache miss:
CacheMetrics.RecordCacheMiss(cacheKey, queryType);

// Record operation duration:
CacheMetrics.RecordOperationDuration(elapsed.TotalMilliseconds, cacheKey, queryType, isHit);
```

### Query Patterns

Cache metrics support identifying cache effectiveness by query type:

```kusto
// Application Insights query for cache hit rate by query type
customMetrics
| where name in ("neba.cache.hits", "neba.cache.misses")
| extend queryType = tostring(customDimensions["query.type"])
| summarize
    hits = sumif(value, name == "neba.cache.hits"),
    misses = sumif(value, name == "neba.cache.misses"),
    hit_rate = sumif(value, name == "neba.cache.hits") * 100.0 / sum(value)
  by queryType
| order by hit_rate desc
```

---

## Background Jobs Telemetry

### Hangfire Infrastructure

**Meter**: `Neba.Hangfire`
**Location**: `Neba.Infrastructure/BackgroundJobs/HangfireMetrics.cs`

#### Metrics

##### `neba.hangfire.job.executions`

- **Type**: Counter
- **Description**: Number of Hangfire job executions
- **Tags**:
  - `job.name`: Name of the job
  - `job.type`: Job type (recurring, scheduled, etc.)

##### `neba.hangfire.job.successes`

- **Type**: Counter
- **Description**: Number of successful job executions
- **Tags**:
  - `job.name`: Name of the job

##### `neba.hangfire.job.failures`

- **Type**: Counter
- **Description**: Number of failed job executions
- **Tags**:
  - `job.name`: Name of the job
  - `error.type`: Exception type

##### `neba.hangfire.job.duration`

- **Type**: Histogram
- **Unit**: milliseconds
- **Description**: Duration of job executions
- **Tags**:
  - `job.name`: Name of the job
  - `result`: success or failure

### Document Sync Job

**Meter**: `Neba.BackgroundJobs`
**Activity Source**: `Neba.BackgroundJobs`
**Location**: `Neba.Application/Documents/SyncHtmlDocumentToStorageMetrics.cs`

#### Metrics

##### `neba.backgroundjob.sync_document.executions`

- **Type**: Counter
- **Description**: Number of document sync job executions
- **Tags**:
  - `document.key`: Document identifier
  - `triggered.by`: User or system trigger

##### `neba.backgroundjob.sync_document.successes`

- **Type**: Counter
- **Description**: Successful document syncs
- **Tags**:
  - `document.key`: Document identifier

##### `neba.backgroundjob.sync_document.failures`

- **Type**: Counter
- **Description**: Failed document syncs
- **Tags**:
  - `document.key`: Document identifier
  - `error.type`: Exception type

##### `neba.backgroundjob.sync_document.duration`
- **Type**: Histogram
- **Unit**: milliseconds
- **Description**: Total duration of sync operation
- **Tags**:
  - `document.key`: Document identifier
  - `result`: success or failure

##### `neba.backgroundjob.sync_document.retrieve.duration`
- **Type**: Histogram
- **Unit**: milliseconds
- **Description**: Duration of document retrieval phase
- **Tags**:
  - `document.key`: Document identifier

##### `neba.backgroundjob.sync_document.upload.duration`
- **Type**: Histogram
- **Unit**: milliseconds
- **Description**: Duration of blob upload phase
- **Tags**:
  - `document.key`: Document identifier

#### Traces

##### `backgroundjob.sync_document`
- **Created When**: Document sync job starts
- **Tags**:
  - `document.key`: Document identifier
  - `triggered.by`: Who triggered the sync
- **Child Spans**: Google Docs export, blob upload

---

## CQRS Handler Telemetry

**Activity Source**: `Neba.Handlers`
**Location**: `Neba.Infrastructure/Tracing/`

### Command Handler Traces

**Pattern**: `command.{CommandType}`

#### Automatic Tracing
Every command execution automatically creates a span:

```csharp
// TracedCommandHandlerDecorator
using Activity? activity = s_activitySource.StartActivity($"command.{_commandType}");
```

**Examples**:
- `command.CreateTournamentCommand`
- `command.UpdateBylawCommand`
- `command.DeleteMemberCommand`

### Query Handler Traces

**Pattern**: `query.{QueryType}`

#### Automatic Tracing
Every query execution automatically creates a span:

```csharp
// TracedQueryHandlerDecorator
using Activity? activity = s_activitySource.StartActivity($"query.{_queryType}");
```

**Examples**:
- `query.GetAllTournamentsQuery`
- `query.GetBylawsQuery`
- `query.SearchMembersQuery`

### Usage in Application Insights

```kusto
// View all command executions with durations
dependencies
| where type == "InProc" and name startswith "command."
| summarize
    count = count(),
    avg_duration = avg(duration),
    p95_duration = percentile(duration, 95)
  by name
| order by count desc
```

---

## Google Docs Integration

**Meter**: `Neba.GoogleDocs`
**Location**: `Neba.Infrastructure/Documents/GoogleDocsMetrics.cs`

### Metrics

#### `neba.google.docs.export.duration`
- **Type**: Histogram
- **Unit**: milliseconds
- **Description**: Duration of Google Docs export operations
- **Tags**:
  - `document.id`: Google Doc ID
  - `export.format`: html, pdf, etc.

#### `neba.google.docs.export.success`
- **Type**: Counter
- **Description**: Successful exports
- **Tags**:
  - `document.id`: Google Doc ID
  - `export.format`: Export format

#### `neba.google.docs.export.failure`
- **Type**: Counter
- **Description**: Failed exports
- **Tags**:
  - `document.id`: Google Doc ID
  - `export.format`: Export format
  - `error.type`: Exception type

#### `neba.google.docs.export.size`
- **Type**: Histogram
- **Unit**: bytes
- **Description**: Size of exported documents
- **Tags**:
  - `document.id`: Google Doc ID
  - `export.format`: Export format

---

## Server-Sent Events (SSE)

**Meter**: `Neba.Infrastructure.SSE`
**Activity Source**: `Neba.Infrastructure.SSE`
**Location**: `Neba.Infrastructure/Documents/SseStreamTelemetry.cs`

### Metrics

#### `neba.sse.connection.count`
- **Type**: Counter
- **Description**: Number of SSE connections opened

#### `neba.sse.connections.active`
- **Type**: UpDownCounter
- **Description**: Currently active SSE connections

#### `neba.sse.connection.duration`
- **Type**: Histogram
- **Unit**: seconds
- **Description**: Duration of SSE connections
- **Tags**:
  - `disconnection.reason`: Timeout, client_closed, error

#### `neba.sse.events.published`
- **Type**: Counter
- **Description**: Number of events published to clients
- **Tags**:
  - `event.type`: Type of SSE event

### Traces

#### `sse.connection.opened`
- **Created When**: Client opens SSE connection

#### `sse.event.published`
- **Created When**: Event sent to client
- **Tags**:
  - `event.type`: Event type
  - `recipients`: Number of connected clients

---

## Frontend Telemetry (Blazor Server)

### API Service

**Meter**: `Neba.Web.Server`
**Activity Source**: `Neba.Web.Server`
**Location**: `Neba.Web.Server/Services/NebaWebsiteApiService.cs`

#### Metrics

##### `neba.web.server.api.calls`
- **Type**: Counter
- **Description**: API calls from frontend to backend
- **Tags**:
  - `api.endpoint`: Endpoint path
  - `http.method`: GET, POST, etc.
  - `http.status_code`: HTTP status code

##### `neba.web.server.api.errors`
- **Type**: Counter
- **Description**: API call errors
- **Tags**:
  - `api.endpoint`: Endpoint path
  - `http.method`: HTTP method
  - `error.type`: Exception type

##### `neba.web.server.api.duration`
- **Type**: Histogram
- **Unit**: milliseconds
- **Description**: API call duration
- **Tags**:
  - `api.endpoint`: Endpoint path
  - `http.method`: HTTP method
  - `http.status_code`: Status code

#### Traces

##### `frontend.api_call`
- **Created When**: Frontend makes API call to backend
- **Tags**:
  - `api.endpoint`: Endpoint path
  - `http.method`: HTTP method
  - `http.status_code`: Response status

### SignalR Circuits

**Meter**: `Neba.Web.Server.SignalR`
**Activity Source**: `Neba.Web.Server.SignalR`
**Location**: `Neba.Web.Server/Telemetry/CircuitHealthTelemetry.cs`

#### Metrics

##### `neba.web.server.signalr.circuit.opened`
- **Type**: Counter
- **Description**: SignalR circuits opened (user connections)

##### `neba.web.server.signalr.circuit.closed`
- **Type**: Counter
- **Description**: SignalR circuits closed
- **Tags**:
  - `circuit.close_reason`: User navigation, timeout, error

##### `neba.web.server.signalr.circuit.duration`
- **Type**: Histogram
- **Unit**: seconds
- **Description**: Circuit lifetime duration

##### `neba.web.server.signalr.connection.errors`
- **Type**: Counter
- **Description**: Circuit connection errors
- **Tags**:
  - `error.type`: Exception type

##### `neba.web.server.signalr.circuits.active`
- **Type**: UpDownCounter
- **Description**: Currently active circuits (connected users)

#### Traces

##### `circuit.opened`
- **Created When**: User establishes SignalR connection
- **Tags**:
  - `circuit.id`: Circuit identifier

##### `circuit.closed`
- **Created When**: Circuit disconnects
- **Tags**:
  - `circuit.id`: Circuit identifier
  - `circuit.duration_seconds`: How long circuit was active

### Navigation Tracking

**Meter**: `Neba.Web.Server.Navigation`
**Activity Source**: `Neba.Web.Server.Navigation`
**Location**: `Neba.Web.Server/Telemetry/NavigationTelemetry.cs`

#### Metrics

##### `neba.web.server.navigation.count`
- **Type**: Counter
- **Description**: User navigation events
- **Tags**:
  - `from.path`: Previous page path
  - `to.path`: Destination page path

##### `neba.web.server.navigation.duration`
- **Type**: Histogram
- **Unit**: milliseconds
- **Description**: Time between navigations
- **Tags**:
  - `from.path`: Previous page
  - `to.path`: Destination page

#### Traces

##### `navigation`
- **Created When**: User navigates between pages
- **Tags**:
  - `navigation.from`: Source path
  - `navigation.to`: Destination path

### Component Lifecycle

**Meter**: `Neba.Web.Server.ComponentLifecycle`
**Activity Source**: `Neba.Web.Server.ComponentLifecycle`
**Location**: `Neba.Web.Server/Telemetry/ComponentLifecycleTelemetry.cs`

#### Metrics

##### `neba.web.server.component.initializations`
- **Type**: Counter
- **Description**: Component initialization count
- **Tags**:
  - `component.name`: Component name

##### `neba.web.server.component.initialization.duration`
- **Type**: Histogram
- **Unit**: milliseconds
- **Description**: OnInitializedAsync duration
- **Tags**:
  - `component.name`: Component name

##### `neba.web.server.component.render.duration`
- **Type**: Histogram
- **Unit**: milliseconds
- **Description**: OnAfterRenderAsync duration
- **Tags**:
  - `component.name`: Component name
  - `is.first_render`: true/false

##### `neba.web.server.component.disposals`
- **Type**: Counter
- **Description**: Component disposal count
- **Tags**:
  - `component.name`: Component name

#### Traces

##### `component.{lifecycleEvent}`
- **Lifecycle Events**: Initialize, Render, Dispose
- **Tags**:
  - `component.name`: Component name
  - `component.lifecycle.event`: Event type

### Component Errors

**Meter**: `Neba.Web.Server.Components`
**Activity Source**: `Neba.Web.Server.Components`
**Location**: `Neba.Web.Server/Telemetry/ComponentTelemetry.cs`

#### Metrics

##### `neba.web.server.component.errors`
- **Type**: Counter
- **Description**: Component error boundary catches
- **Tags**:
  - `component.name`: Component name
  - `error.type`: Exception type

#### Traces

##### `component.error`
- **Created When**: Error boundary catches exception
- **Tags**:
  - `component.name`: Component where error occurred
  - `error.type`: Exception type
  - `exception.message`: Error message
  - `exception.stacktrace`: Stack trace

---

## JavaScript Telemetry

**Meter**: `Neba.Web.Server.JavaScript`
**Activity Source**: `Neba.Web.Server.JavaScript`
**Location**:
- Bridge: `Neba.Web.Server/Telemetry/JavaScriptTelemetryBridge.cs`
- Helper: `Neba.Web.Server/wwwroot/js/telemetry-helper.js`

### Metrics

#### `neba.web.server.javascript.interactions`
- **Type**: Counter
- **Description**: JavaScript user interactions tracked
- **Tags**:
  - `event.name`: Event identifier (e.g., "map.route_calculated")
  - Custom tags from event properties

#### `neba.web.server.javascript.operation.duration`
- **Type**: Histogram
- **Unit**: milliseconds
- **Description**: Duration of JavaScript operations
- **Tags**:
  - `event.name`: Operation name
  - `success`: true/false
  - Custom tags from event properties

### Traces

#### `javascript.{eventName}`
- **Created When**: JavaScript code calls `trackEvent()`
- **Tags**:
  - `event.name`: Event identifier
  - `event.source`: "javascript"
  - Any custom properties passed from JS

#### `javascript.error`
- **Created When**: JavaScript code calls `trackError()`
- **Tags**:
  - `error.message`: Error message
  - `error.source`: Source location
  - `exception.stacktrace`: JavaScript stack trace

### JavaScript API Usage

#### Initialization

```javascript
import { initializeTelemetry } from '/js/telemetry-helper.js';

// Initialize with DotNet reference
await initializeTelemetry(DotNet.createJSObjectReference(dotNetBridge));
```

#### Track Events

```javascript
import { trackEvent } from '/js/telemetry-helper.js';

// Track user interaction
trackEvent('map.route_calculated', {
    duration_ms: 150,
    waypoint_count: 5,
    success: true
});
```

#### Track Errors

```javascript
import { trackError } from '/js/telemetry-helper.js';

try {
    await someOperation();
} catch (error) {
    trackError(error.message, 'map.route', error.stack);
    throw error;
}
```

#### Automatic Performance Tracking

```javascript
import { withTelemetry } from '/js/telemetry-helper.js';

// Wrap async function with automatic telemetry
const calculateRoute = withTelemetry('map.calculate_route', async (from, to) => {
    // Your code here
    return result;
});

// Automatically tracks:
// - Duration
// - Success/failure
// - Error details if thrown
```

#### Create Custom Timers

```javascript
import { createTimer } from '/js/telemetry-helper.js';

const timer = createTimer('data.load');

// ... perform operation ...

timer.stop(true, {
    record_count: 100,
    data_source: 'api'
});
```

### Web Performance Metrics

#### Resource Loading

**Event**: `resource.loaded`
- **Tracked Automatically**: Called by `trackResourcePerformance()`
- **Properties**:
  - `resource_name`: File name
  - `resource_type`: script, stylesheet, image, etc.
  - `duration_ms`: Total load time
  - `transfer_size`: Bytes transferred
  - `dns_time`: DNS lookup duration
  - `tcp_time`: TCP connection time
  - `ttfb`: Time to first byte
  - `download_time`: Download duration

#### Page Performance

**Event**: `page.performance`
- **Tracked Automatically**: Called by `trackNavigationPerformance()`
- **Properties**:
  - `navigation_type`: navigate, reload, back_forward
  - `redirect_count`: Number of redirects
  - `dns_time`: DNS lookup
  - `tcp_time`: TCP connection
  - `request_time`: Request duration
  - `response_time`: Response duration
  - `dom_processing_time`: DOM processing
  - `dom_interactive_time`: Time to interactive
  - `dom_content_loaded_time`: DOMContentLoaded event
  - `load_event_time`: Load event
  - `total_load_time`: Full page load

#### Core Web Vitals

**Event**: `web_vitals.lcp` (Largest Contentful Paint)
- **Properties**:
  - `value`: LCP time in milliseconds
  - `element`: Tag name of largest element

**Event**: `web_vitals.fid` (First Input Delay)
- **Properties**:
  - `value`: FID time in milliseconds
  - `event_type`: Type of first interaction

**Event**: `web_vitals.cls` (Cumulative Layout Shift)
- **Properties**:
  - `value`: CLS score

---

## Adding New Telemetry

### Adding Metrics

1. **Create a meter** in the appropriate class:

```csharp
private static readonly Meter s_meter = new("Neba.{Component}");
```

2. **Define the metric**:

```csharp
private static readonly Counter<long> s_operations = s_meter.CreateCounter<long>(
    "neba.{component}.{resource}.{action}",
    description: "Description following OpenTelemetry conventions");
```

3. **Record measurements** with tags:

```csharp
TagList tags = new()
{
    { "resource.id", resourceId },
    { "operation.type", operationType }
};
s_operations.Add(1, tags);
```

### Adding Traces

1. **Create an activity source**:

```csharp
private static readonly ActivitySource s_activitySource = new("Neba.{Component}");
```

2. **Register in ServiceDefaults** (`OpenTelemetryExtensions.cs`):

```csharp
.WithTracing(tracing => tracing
    .AddSource("Neba.*")  // Wildcard covers all
```

3. **Create spans**:

```csharp
using Activity? activity = s_activitySource.StartActivity("{action}.{operation}");
activity?.SetTag("resource.id", resourceId);
activity?.SetTag("operation.type", operationType);

try
{
    // Perform operation
    activity?.SetTag("result", "success");
}
catch (Exception ex)
{
    activity?.SetExceptionTags(ex);  // Extension method
    throw;
}
```

### JavaScript Telemetry

Add telemetry calls in your JavaScript code:

```javascript
import { trackEvent, withTelemetry, trackError } from '/js/telemetry-helper.js';

// Option 1: Manual tracking
trackEvent('feature.action', {
    param1: value1,
    duration_ms: elapsed
});

// Option 2: Automatic wrapper
const myFunction = withTelemetry('feature.action', async () => {
    // Your code
});

// Option 3: Error tracking
try {
    await operation();
} catch (error) {
    trackError(error.message, 'feature.name', error.stack);
    throw error;
}
```

---

## Querying Telemetry

### Local Development (Aspire Dashboard)

Access the dashboard at `http://localhost:18889` when running the application.

**Useful Views**:
1. **Traces**: See distributed traces across services
2. **Metrics**: Real-time charts for all metrics
3. **Structured Logs**: Searchable logs with trace correlation
4. **Resources**: Service health and discovery

**Filtering Traces**:
```
# Filter by service
service.name == "Neba.Web.Server"

# Filter by duration
duration > 1s

# Filter by custom tag
db.context.schema == "website"
```

### Production (Azure Monitor)

#### View All Custom Metrics

```kusto
customMetrics
| where name startswith "neba."
| summarize
    count = sum(value),
    avg = avg(value),
    p95 = percentile(value, 95)
  by name
| order by count desc
```

#### Cache Hit Rate Analysis

```kusto
customMetrics
| where name in ("neba.cache.hits", "neba.cache.misses")
| extend queryType = tostring(customDimensions["query.type"])
| summarize
    hits = sumif(value, name == "neba.cache.hits"),
    misses = sumif(value, name == "neba.cache.misses"),
    total = sum(value),
    hit_rate = sumif(value, name == "neba.cache.hits") * 100.0 / sum(value)
  by queryType
| order by total desc
```

#### Slow Query Analysis

```kusto
customMetrics
| where name == "neba.database.query.slow"
| extend
    operation = tostring(customDimensions["db.operation"]),
    statement = tostring(customDimensions["db.statement"])
| summarize count = sum(value) by operation, statement
| order by count desc
```

#### API Performance by Endpoint

```kusto
customMetrics
| where name == "neba.web.server.api.duration"
| extend
    endpoint = tostring(customDimensions["api.endpoint"]),
    method = tostring(customDimensions["http.method"])
| summarize
    count = count(),
    avg_ms = avg(value),
    p95_ms = percentile(value, 95),
    p99_ms = percentile(value, 99)
  by endpoint, method
| order by count desc
```

#### Circuit (User Session) Analysis

```kusto
customMetrics
| where name == "neba.web.server.signalr.circuit.duration"
| extend reason = tostring(customDimensions["circuit.close_reason"])
| summarize
    count = count(),
    avg_duration_sec = avg(value),
    p95_duration_sec = percentile(value, 95)
  by reason
```

#### Distributed Trace Example

```kusto
// Find a slow request
requests
| where timestamp > ago(1h)
| where duration > 2000  // > 2 seconds
| project operation_Id, name, duration

// View full trace
dependencies
| where operation_Id == "abc123..."
| union (requests | where operation_Id == "abc123...")
| project timestamp, type, name, duration, resultCode
| order by timestamp asc
```

#### JavaScript Performance Events

```kusto
customEvents
| where name == "page.performance"
| extend
    totalLoad = todouble(customDimensions["total_load_time"]),
    domProcessing = todouble(customDimensions["dom_processing_time"]),
    ttfb = todouble(customDimensions["ttfb"])
| summarize
    count = count(),
    avg_load = avg(totalLoad),
    p95_load = percentile(totalLoad, 95)
  by bin(timestamp, 1h)
```

---

## Best Practices

### Metric Guidelines

1. **Use appropriate metric types**:
   - **Counter**: Cumulative values that only increase (request counts, error counts)
   - **Histogram**: Distributions of values (durations, sizes)
   - **UpDownCounter**: Values that can increase or decrease (active connections, queue depth)

2. **Add meaningful tags**:
   - Include tags that help with filtering and aggregation
   - Keep cardinality reasonable (avoid unique IDs as tags)
   - Use consistent tag names across metrics

3. **Choose appropriate units**:
   - Durations: milliseconds for operations, seconds for long-running
   - Sizes: bytes for data transfers
   - Counts: unitless

### Trace Guidelines

1. **Create spans for significant operations**:
   - External service calls
   - Database queries (automatic via EF Core)
   - Business logic boundaries
   - Background job executions

2. **Add contextual tags**:
   - Resource identifiers
   - Operation types
   - User context (non-PII)
   - Input parameters (non-sensitive)

3. **Handle errors properly**:
   ```csharp
   try
   {
       // Operation
   }
   catch (Exception ex)
   {
       activity?.SetExceptionTags(ex);  // Sets error.type, exception.message, etc.
       throw;
   }
   ```

### Logging Guidelines

1. **Use structured logging** with `LoggerMessage` source generators:
   ```csharp
   [LoggerMessage(Level = LogLevel.Information, Message = "Processing {ResourceId}")]
   static partial void LogProcessing(ILogger logger, string resourceId);
   ```

2. **Include trace context**: Logs automatically correlated with traces via Activity.Current

3. **Log at appropriate levels**:
   - **Debug**: Detailed diagnostic information
   - **Information**: Normal operation events
   - **Warning**: Unusual but not error conditions
   - **Error**: Error conditions that need attention
   - **Critical**: System-level failures

---

## Troubleshooting

### No Telemetry Appearing

**Local Development**:
1. Verify Aspire Dashboard is running: `docker ps | grep aspire`
2. Check OTLP endpoint: `http://localhost:18889`
3. Check console for OpenTelemetry errors
4. Verify `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable

**Production**:
1. Verify Application Insights connection string is configured
2. Check Azure Monitor resource in Azure Portal
3. Look for `ApplicationInsights` logs in application logs

### Missing Custom Metrics

1. Verify meter name follows pattern: `Neba.*`
2. Check ServiceDefaults includes: `.AddMeter("Neba.*")`
3. Ensure metrics are being recorded (add breakpoint)

### Missing Traces

1. Verify activity source name follows pattern: `Neba.*`
2. Check ServiceDefaults includes: `.AddSource("Neba.*")`
3. Ensure activity is being created (not null)

### JavaScript Telemetry Not Working

1. Verify telemetry bridge is initialized in `_Layout.razor`
2. Check browser console for JavaScript errors
3. Verify DotNet reference is created correctly
4. Check that module is imported correctly

---

## Maintenance

This document should be updated whenever:

1. **New metrics are added**: Add entry in relevant section
2. **New traces are added**: Document activity source and span names
3. **Bounded contexts are added**: Add new sections for context-specific telemetry
4. **Telemetry patterns change**: Update examples and best practices
5. **Query patterns are discovered**: Add to "Querying Telemetry" section

**Review Schedule**: Quarterly review of telemetry usage and effectiveness

---

## Related Documentation

- [ADR-0050: OpenTelemetry Implementation](../architecture/adr-0050-opentelemetry-without-aspire-apphost.md)
- [OpenTelemetry Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/)
- [Azure Monitor OpenTelemetry](https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-enable)
- [Aspire Dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard)
