---
layout: default
title: SignalR Hubs
---

## Overview
This document describes the SignalR hubs available in the Neba Management application and their real-time notification broadcasts.

## Document Refresh Hub

### Connection Details
**Hub Endpoint:** `/hubs/document-refresh`
**Hub Class:** `DocumentRefreshSignalRHub`
**Implementation:** Single hub for all document types (Bylaws, Tournament Rules, etc.)

### Architecture Pattern
The document refresh hub uses a generic, type-agnostic design:
- One hub serves all document types
- Dynamic group names based on document type: `"{documentType}-refresh"`
- Abstraction layer via `IDocumentRefreshNotifier` separates business logic from SignalR implementation

### Client-to-Server Methods

#### JoinRefreshAsync
Subscribes a client to receive refresh status updates for a specific document type.

**Signature:**
```csharp
Task JoinRefreshAsync(string documentKey, CancellationToken cancellationToken = default)
```

**Parameters:**
- `documentKey`: The document type identifier (e.g., "bylaws", "tournament-rules")
- `cancellationToken`: Optional cancellation token

**Behavior:**
1. Adds the connection to the document-specific group: `"{documentKey}-refresh"`
2. Fetches current job state from HybridCache
3. If a job is currently running, immediately sends the current status to the caller

**Example Usage (JavaScript):**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/document-refresh")
    .build();

await connection.start();
await connection.invoke("JoinRefreshAsync", "bylaws");
```

---

#### LeaveRefreshAsync
Unsubscribes a client from refresh status updates for a specific document type.

**Signature:**
```csharp
Task LeaveRefreshAsync(string documentKey, CancellationToken cancellationToken = default)
```

**Parameters:**
- `documentKey`: The document type identifier (e.g., "bylaws", "tournament-rules")
- `cancellationToken`: Optional cancellation token

**Behavior:**
- Removes the connection from the document-specific group: `"{documentKey}-refresh"`

**Example Usage (JavaScript):**
```javascript
await connection.invoke("LeaveRefreshAsync", "bylaws");
```

---

### Server-to-Client Events

#### DocumentRefreshStatusChange
Broadcast to all clients in a document-specific group when the refresh job status changes.

**Event Name:** `"DocumentRefreshStatusChange"`

**Payload:**
```typescript
(status: DocumentRefreshStatus, errorMessage: string | null) => void
```

**Parameters:**
- `status`: Current status of the refresh job (see DocumentRefreshStatus enum below)
- `errorMessage`: Error message if status is "Failed", otherwise null

**DocumentRefreshStatus Enum Values:**
- `"Retrieving"` - Job is retrieving the document from Google Docs
- `"Uploading"` - Job is uploading the document to Azure Blob Storage
- `"Completed"` - Job completed successfully
- `"Failed"` - Job failed with an error

**When Triggered:**
- **On Join (Initial State):** If a job is currently running, sent immediately to the caller via `Clients.Caller`
- **During Execution:** Broadcast to all group members via `Clients.Group()` at each status transition:
  1. When retrieval starts: `Retrieving`
  2. When upload starts: `Uploading`
  3. When job completes: `Completed`
  4. If job fails: `Failed` (with error message)

**Example Handler (JavaScript):**
```javascript
connection.on("DocumentRefreshStatusChange", (status, errorMessage) => {
    console.log(`Refresh status: ${status}`);

    switch (status) {
        case "Retrieving":
            showMessage("Retrieving document from Google Docs...");
            break;
        case "Uploading":
            showMessage("Uploading document to storage...");
            break;
        case "Completed":
            showMessage("Document refresh completed!");
            setTimeout(() => window.location.href = "/bylaws", 3000);
            break;
        case "Failed":
            showError(`Refresh failed: ${errorMessage}`);
            break;
    }
});
```

**Example Handler (C# Blazor):**
```csharp
await _hubConnection.On<DocumentRefreshStatus, string?>(
    "DocumentRefreshStatusChange",
    async (status, errorMessage) =>
    {
        await InvokeAsync(() =>
        {
            _currentStatus = status;
            _errorMessage = errorMessage;
            StateHasChanged();

            if (status == DocumentRefreshStatus.Completed)
            {
                // Navigate after 3 seconds
                Task.Delay(3000).ContinueWith(_ =>
                    NavigationManager.NavigateTo("/bylaws"));
            }
        });
    });
```

---

## Notification Architecture

### IDocumentRefreshNotifier Interface
An abstraction layer that decouples job handlers from SignalR-specific implementation.

**Purpose:**
- Allows `SyncHtmlDocumentToStorageJobHandler` to broadcast status updates without knowing about SignalR
- Makes the handler testable and framework-agnostic
- Could be swapped for other real-time notification systems (WebSockets, Server-Sent Events, etc.)

**Interface:**
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

### SignalRDocumentRefreshNotifier Implementation
Concrete implementation that broadcasts to SignalR groups.

**Implementation Details:**
- Depends on `IHubContext<DocumentRefreshSignalRHub>`
- Broadcasts `"DocumentRefreshStatusChange"` event to the specified group
- Handles null/empty group names gracefully (no-op)

**Flow:**
1. Job handler calls `IDocumentRefreshNotifier.NotifyStatusAsync()`
2. SignalR implementation broadcasts to group via `IHubContext`
3. All connected clients in the group receive the event

---

## State Management

### HybridCache Integration
The hub and notifier use `HybridCache` for job state tracking:

**Cache Keys:** `"{documentKey}:refresh:current"` (e.g., "bylaws:refresh:current")
**Cache Tags:** `["{documentKey}", "document-refresh-state"]`
**Expiration:** 10 minutes
**Cleanup Strategy:**
- Success: 5-second delay before clearing (allows late joiners to see completion)
- Failure: 1-minute delay before clearing (allows debugging)

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

---

## Complete Usage Example

### Blazor Client Component
```csharp
@page "/bylaws/refresh"
@inject NavigationManager NavigationManager
@implements IAsyncDisposable

<h3>Refreshing Bylaws</h3>
<p>Status: @_currentStatus</p>
@if (!string.IsNullOrEmpty(_errorMessage))
{
    <p class="error">@_errorMessage</p>
}

@code {
    private HubConnection? _hubConnection;
    private DocumentRefreshStatus? _currentStatus;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/hubs/document-refresh"))
            .Build();

        _hubConnection.On<DocumentRefreshStatus, string?>(
            "DocumentRefreshStatusChange",
            async (status, errorMessage) =>
            {
                await InvokeAsync(() =>
                {
                    _currentStatus = status;
                    _errorMessage = errorMessage;
                    StateHasChanged();

                    if (status == DocumentRefreshStatus.Completed)
                    {
                        Task.Delay(3000).ContinueWith(_ =>
                            NavigationManager.NavigateTo("/bylaws"));
                    }
                });
            });

        await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("JoinRefreshAsync", "bylaws");
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
```

### Triggering a Refresh
```csharp
// POST /api/bylaws/refresh
var response = await httpClient.PostAsync("/api/bylaws/refresh", null);

if (response.StatusCode == HttpStatusCode.Accepted)
{
    // New job started, navigate to refresh page
    NavigationManager.NavigateTo("/bylaws/refresh");
}
else if (response.StatusCode == HttpStatusCode.OK)
{
    // Job already in progress, navigate to refresh page
    NavigationManager.NavigateTo("/bylaws/refresh");
}
```

---

## Design Rationale

### Why a Single Hub?
- **Simplicity:** One hub is easier to maintain than multiple hub classes
- **Scalability:** Dynamic groups scale better than hardcoded hub endpoints
- **Flexibility:** Easy to add new document types without code changes
- **Consistency:** Uniform behavior across all document types

### Why the Abstraction Layer?
- **Testability:** Handler can be unit tested without SignalR dependencies
- **Separation of Concerns:** Business logic doesn't depend on infrastructure
- **Flexibility:** Could swap SignalR for other real-time technologies
- **Single Responsibility:** Handler focuses on sync logic, notifier handles broadcasting

### Why HybridCache?
- **Two-tier caching:** In-memory L1 + distributed L2 for performance
- **Tag-based invalidation:** Clean up related cache entries easily
- **Stampede protection:** Prevents duplicate job execution
- **Built-in serialization:** Works seamlessly with distributed cache backends
