---
layout: default
title: Server-Sent Events (SSE) for Document Refresh
---

## Overview
This document describes the Server-Sent Events (SSE) implementation for real-time document refresh status updates in the Neba Management application.

## Why SSE Instead of SignalR?

SSE was chosen over SignalR for document refresh notifications because:
- **Simplicity:** Built-in browser API, no client library needed
- **Unidirectional:** Document refresh is server-to-client only (perfect fit for SSE)
- **Lower overhead:** Simple text-based protocol over HTTP
- **Native ASP.NET Core support:** `IAsyncEnumerable<T>` streaming with minimal code
- **Automatic reconnection:** Browsers automatically reconnect on disconnect

## Architecture

### Components

1. **DocumentRefreshSseStreamHandler** - Factory for creating SSE stream handlers
2. **DocumentRefreshChannelManager** - Manages concurrent refresh operations with automatic cleanup
3. **SseDocumentRefreshNotifier** - Writes status updates to channels
4. **DocumentRefreshStatusEvent** - Event payload sent to clients

### Data Flow

```
Background Job (Hangfire)
    ↓
SseDocumentRefreshNotifier.NotifyStatusAsync()
    ↓
DocumentRefreshChannelManager.WriteToChannelAsync()
    ↓
Channel<DocumentRefreshStatusEvent>
    ↓
SSE Stream (one or more connected clients)
```

## SSE Endpoints

### Bylaws Document Refresh Status

**Endpoint:** `GET /bylaws/refresh/status`
**Content-Type:** `text/event-stream`
**Document Type:** `bylaws`

### Tournament Rules Refresh Status

**Endpoint:** `GET /tournaments/refresh/status`
**Content-Type:** `text/event-stream`
**Document Type:** `tournament-rules`

## Event Format

### SSE Stream Format
```
data: {"status":"Retrieving","errorMessage":null}

data: {"status":"Uploading","errorMessage":null}

data: {"status":"Completed","errorMessage":null}

```

Each event is:
1. A line starting with `data: ` followed by JSON
2. An empty line (event separator)

### Event Payload (JSON)

```typescript
{
  status: "Retrieving" | "Uploading" | "Completed" | "Failed",
  errorMessage: string | null
}
```

**Status Values:**
- `Retrieving` - Fetching document from Google Docs
- `Uploading` - Uploading document to Azure Blob Storage
- `Completed` - Refresh completed successfully
- `Failed` - Refresh failed (errorMessage contains details)

## Client Implementation

### JavaScript (Vanilla)

```javascript
const eventSource = new EventSource('/bylaws/refresh/status');

eventSource.onmessage = (event) => {
    const data = JSON.parse(event.data);

    console.log(`Status: ${data.status}`);

    switch (data.status) {
        case 'Retrieving':
            showMessage('Retrieving document from Google Docs...');
            break;
        case 'Uploading':
            showMessage('Uploading document to storage...');
            break;
        case 'Completed':
            showMessage('Document refresh completed!');
            // Close connection and navigate
            eventSource.close();
            setTimeout(() => window.location.href = '/bylaws', 3000);
            break;
        case 'Failed':
            showError(`Refresh failed: ${data.errorMessage}`);
            eventSource.close();
            break;
    }
};

eventSource.onerror = (error) => {
    console.error('SSE connection error:', error);
    eventSource.close();
};
```

### C# (Blazor Server - Not Yet Implemented)

```csharp
@page "/bylaws/refresh"
@inject HttpClient Http
@inject NavigationManager NavigationManager
@implements IAsyncDisposable

<h3>Refreshing Bylaws</h3>
<p>Status: @_currentStatus</p>
@if (!string.IsNullOrEmpty(_errorMessage))
{
    <p class="error">@_errorMessage</p>
}

@code {
    private string? _currentStatus;
    private string? _errorMessage;
    private CancellationTokenSource? _cts;

    protected override async Task OnInitializedAsync()
    {
        _cts = new CancellationTokenSource();

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/bylaws/refresh/status");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(_cts.Token);
            using var reader = new StreamReader(stream);

            while (!_cts.Token.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("data: "))
                {
                    var json = line.Substring(6);
                    var statusEvent = JsonSerializer.Deserialize<DocumentRefreshStatusEvent>(json);

                    await InvokeAsync(() =>
                    {
                        _currentStatus = statusEvent.Status;
                        _errorMessage = statusEvent.ErrorMessage;
                        StateHasChanged();

                        if (statusEvent.Status == "Completed")
                        {
                            Task.Delay(3000).ContinueWith(_ =>
                                NavigationManager.NavigateTo("/bylaws"));
                        }
                    });
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on dispose
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_cts is not null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
    }

    private record DocumentRefreshStatusEvent(string Status, string? ErrorMessage);
}
```

## Channel Management

### Automatic Lifecycle

The `DocumentRefreshChannelManager` automatically manages channel lifecycles:

**Creation:**
- Channel created on first client connection
- Shared across multiple concurrent clients for the same document type

**Cleanup:**
- **No listeners + 5 seconds idle:** Channel removed after brief reconnection window
- **30-second idle timeout:** Removed if no activity and no listeners
- **5-minute maximum lifetime:** Channels removed after 5 minutes regardless of state

### Thread Safety

- Uses `ConcurrentDictionary` for thread-safe channel storage
- Uses `Interlocked` operations for listener counting
- Supports multiple concurrent readers and writers per channel

## State Management

### Initial State on Connection

When a client connects, the SSE handler:
1. Checks `HybridCache` for current job state
2. If a refresh job is in progress, immediately sends the current status
3. Then streams subsequent updates

**Cache Key Pattern:** `{documentType}:refresh:current`
**Example:** `bylaws:refresh:current`

**Cache Tags:** `[documentType, "document-refresh-state"]`
**Example:** `["bylaws", "document-refresh-state"]`

**Expiration:** 10 minutes

### Job State Model

```csharp
public sealed record DocumentRefreshJobState
{
    public required string DocumentType { get; init; }
    public required DocumentRefreshStatus Status { get; init; }
    public required DateTimeOffset StartedAt { get; init; }
    public required string TriggeredBy { get; init; }
    public string? ErrorMessage { get; init; }
}
```

## Notification Architecture

### IDocumentRefreshNotifier Interface

Abstraction layer that decouples background jobs from SSE implementation.

```csharp
public interface IDocumentRefreshNotifier
{
    Task NotifyStatusAsync(
        string? hubGroupName,
        DocumentRefreshStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default);
}
```

### SseDocumentRefreshNotifier Implementation

Concrete implementation that writes to channels:

```csharp
public class SseDocumentRefreshNotifier : IDocumentRefreshNotifier
{
    private readonly DocumentRefreshChannelManager _channelManager;

    public async Task NotifyStatusAsync(
        string? hubGroupName,
        DocumentRefreshStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hubGroupName))
            return;

        var statusEvent = DocumentRefreshStatusEvent.FromStatus(status, errorMessage);
        await _channelManager.WriteToChannelAsync(hubGroupName, statusEvent, cancellationToken);
    }
}
```

**Benefits:**
- Background jobs don't know about SSE
- Easy to test (mock `IDocumentRefreshNotifier`)
- Could swap SSE for WebSockets, SignalR, etc. without changing job handlers

## Complete Usage Flow

### 1. Trigger Refresh

```http
POST /bylaws/refresh
```

Response:
```json
{
  "data": "job-id-12345"
}
```

### 2. Connect to SSE Stream

```javascript
const eventSource = new EventSource('/bylaws/refresh/status');

eventSource.onmessage = (event) => {
    const { status, errorMessage } = JSON.parse(event.data);
    updateUI(status, errorMessage);
};
```

### 3. Receive Status Updates

```
data: {"status":"Retrieving","errorMessage":null}

data: {"status":"Uploading","errorMessage":null}

data: {"status":"Completed","errorMessage":null}

```

### 4. Close Connection on Completion

```javascript
if (data.status === 'Completed' || data.status === 'Failed') {
    eventSource.close();
}
```

## Error Handling

### Client-Side Errors

- **Connection Lost:** Browser automatically reconnects (SSE feature)
- **Parse Error:** Catch JSON parse exceptions
- **Timeout:** Set a maximum wait time and close connection

### Server-Side Errors

- **Channel Errors:** Logged and connection closed gracefully
- **Cancellation:** Expected during normal disconnects
- **No Active Channel:** Status updates are logged but not sent

## Design Rationale

### Why Channel-Based Architecture?

- **Decoupling:** Background jobs don't manage connections
- **Scalability:** Single channel per document type, shared across clients
- **Memory Efficiency:** Automatic cleanup prevents channel leaks
- **Concurrency:** Multiple jobs can write, multiple clients can read

### Why Not WebSockets?

- **Unidirectional:** Document refresh is server-to-client only
- **Simplicity:** SSE is simpler for one-way communication
- **HTTP/2 Compatibility:** Works over standard HTTP
- **No Handshake:** Faster connection establishment

### Why HybridCache for State?

- **Two-tier caching:** In-memory L1 + distributed L2
- **Late Joiners:** Clients connecting mid-refresh see current status
- **Tag-based invalidation:** Easy cleanup on job completion
- **Stampede Protection:** Prevents duplicate refresh jobs

## Testing

### Integration Tests

See [DocumentRefreshIntegrationTests.cs](../../tests/Neba.IntegrationTests/Website/Documents/DocumentRefreshIntegrationTests.cs) for comprehensive examples including:

- SSE stream consumption
- Status progression validation
- Error handling scenarios
- Concurrent client support
- Late joiner scenarios

### Manual Testing

```bash
# Terminal 1: Monitor SSE stream
curl -N http://localhost:5000/bylaws/refresh/status

# Terminal 2: Trigger refresh
curl -X POST http://localhost:5000/bylaws/refresh
```

Expected output in Terminal 1:
```
data: {"status":"Retrieving","errorMessage":null}

data: {"status":"Uploading","errorMessage":null}

data: {"status":"Completed","errorMessage":null}

```
