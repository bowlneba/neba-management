# Blazor UI Implementation: Document Refresh with Real-Time Status

## Context
We have a document viewing system (Bylaws, Tournament Rules) with a manual refresh feature that syncs documents from Google Docs to Azure Blob Storage. The refresh process runs as a background job with real-time status updates via Server-Sent Events (SSE).

## Backend Implementation Summary

### API Endpoints (Minimal APIs)

**Bylaws:**
- GET `/bylaws` - Returns document content with metadata
- POST `/bylaws/refresh` - Triggers background sync job
- GET `/bylaws/refresh/status` - SSE endpoint streaming job status

**Tournament Rules:**
- GET `/tournament/rules` - Returns document content with metadata
- POST `/tournament/rules/refresh` - Triggers background sync job
- GET `/tournament/rules/refresh/status` - SSE endpoint streaming job status

### Refresh Flow
1. User clicks "Refresh" button → POST to `/bylaws/refresh`
2. API returns `DocumentRefreshResponse` with:
   ```json
   {
     "jobId": "string",
     "status": "Retrieving",
     "message": "Refresh started",
     "triggeredBy": "username"
   }
   ```
3. UI connects to SSE endpoint: GET `/bylaws/refresh/status`
4. SSE streams status events as JSON:
   ```json
   {
     "status": "Retrieving|Uploading|Complete|Failed",
     "errorMessage": "optional error message",
     "timestamp": "ISO 8601 timestamp"
   }
   ```
5. Status progression: `Retrieving` → `Uploading` → `Complete` (or `Failed`)

### Document DTOs
```csharp
public class BylawsDto
{
    public string Content { get; set; }          // HTML content
    public DateTime? LastRefreshed { get; set; } // syncedAt - visible to all
    public string? LastRefreshedBy { get; set; } // syncedBy - authorized only (future)
}

public enum RefreshStatus
{
    Retrieving,  // Fetching from Google Docs
    Uploading,   // Uploading to Azure Blob Storage
    Complete,    // Done
    Failed       // Error occurred
}
```

### SSE Connection Details
- Standard EventSource API
- Events named: "message" (standard SSE)
- Auto-reconnect on disconnect (browser handles this)
- Connection timeout: 30 seconds idle
- Maximum connection lifetime: 5 minutes

## UI Requirements

### Document Page Structure

**Pages:**
- `/bylaws` - Bylaws document viewer (Blazor page)
- `/tournament/rules` - Tournament Rules document viewer (Blazor page)

**Components on Document Page:**
1. Document content display (HTML from `Content` property)
2. Metadata display:
   - **Last Refreshed**: `LastRefreshed` timestamp - **visible to all users**
   - **Refreshed By**: `LastRefreshedBy` - **visible to authorized users only** (future authorization)
3. Refresh button - **visible to authorized users only** (future authorization, show to everyone for now)

### Refresh Button

**Location:** UI/UX expert should determine optimal placement on document page

**Behavior:**
- Click → POST to `/bylaws/refresh` (or `/tournament/rules/refresh`)
- Handle response:
  - If new job (202 Accepted): start SSE connection and show progress
  - If already running (200 OK): connect to existing job via SSE and show progress

**Authorization (Future):**
- Button should be conditionally rendered based on authorization
- For now: visible to everyone
- Structure code so adding `@if (IsAuthorized)` is trivial

### Progress Indicator (Spinner with Stages)

**Requirements:**
- Use existing app standards for spinner/progress UI
- Display current stage as text below/within spinner
- Update text based on SSE status events

**Stage Messages:**
- `Retrieving`: "Retrieving document from Google Docs..."
- `Uploading`: "Uploading document to storage..."
- `Complete`: "Refresh complete! Redirecting in 3... 2... 1..."
- `Failed`: Hide spinner, show error alert

**Final Stage (Complete):**
- Show countdown: "Redirecting in 3... 2... 1..."
- After 3 seconds: reload the page (to show updated content)
- Use browser page reload, not Blazor navigation

### Error Handling

**On Failed Status:**
- Close/hide spinner
- Display error using standard app error alert pattern
- Show `errorMessage` from SSE event
- Allow user to stay on page (no auto-navigation)
- User can try refresh again or navigate away manually

### Navigation Away During Refresh

**If user navigates to another page while refresh is running:**
1. SSE connection closes automatically (browser behavior)
2. Background job continues running
3. When job completes (Complete status):
   - Show toast notification: "Bylaws refresh complete" (or "Tournament Rules refresh complete")
   - Toast is clickable → navigates to document page (`/bylaws` or `/tournament/rules`)
   - Use app's standard toast notification system

### Browser Refresh During Job

**If user refreshes browser while job is running:**
1. On page load, check for active job (call POST endpoint, get existing job if running)
2. If job exists and is in progress:
   - Reconnect to SSE endpoint
   - Show progress spinner with current status
   - Continue as if connection never dropped
3. If no active job or job already complete:
   - Just show document normally (no spinner)

### Multi-Tab Behavior
- Each tab manages its own SSE connection
- If user has document open in multiple tabs during refresh:
  - All tabs receive same SSE events
  - All tabs show same progress
  - All tabs reload after 3-second countdown
- This happens automatically via EventSource API (no special handling needed)

## Technical Implementation Guidance

### SSE Connection in Blazor

**Use JavaScript Interop for EventSource:**
```javascript
// wwwroot/js/sse-client.js
export function connectToRefreshStatus(documentType, dotNetHelper) {
    const eventSource = new EventSource(`/${documentType}/refresh/status`);

    eventSource.onmessage = (event) => {
        const data = JSON.parse(event.data);
        dotNetHelper.invokeMethodAsync('OnStatusUpdate', data.status, data.errorMessage);
    };

    eventSource.onerror = () => {
        dotNetHelper.invokeMethodAsync('OnConnectionError');
        eventSource.close();
    };

    return {
        close: () => eventSource.close()
    };
}
```

**In Blazor Component:**
- Call JS interop to establish SSE connection
- Use `[JSInvokable]` methods to receive status updates
- Close connection on disposal or completion

### State Management

**Component should track:**
- `isRefreshing` - bool for showing/hiding spinner
- `currentStatus` - RefreshStatus enum
- `errorMessage` - string for displaying errors
- SSE connection reference (for cleanup)

**Lifecycle:**
1. User clicks refresh → `isRefreshing = true`
2. POST to refresh endpoint
3. Connect to SSE
4. Update `currentStatus` as events arrive
5. On Complete: countdown → reload page
6. On Failed: `isRefreshing = false`, show error
7. On navigate away: cleanup SSE connection

### Authorization Structure (Future-Ready)

**Refresh Button:**
```razor
@* Current (everyone sees it) *@
<button @onclick="TriggerRefresh">Refresh Document</button>

@* Future (when authorization implemented) *@
@if (IsAuthorized)
{
    <button @onclick="TriggerRefresh">Refresh Document</button>
}
```

**Metadata Display:**
```razor
<div class="document-metadata">
    @if (document.LastRefreshed.HasValue)
    {
        <span>Last Refreshed: @document.LastRefreshed.Value.ToString("g")</span>
    }

    @* Future: only show for authorized *@
    @if (!string.IsNullOrEmpty(document.LastRefreshedBy))
    {
        @* Remove @if when authorization is implemented *@
        @* @if (IsAuthorized) *@
        @* { *@
            <span>Refreshed By: @document.LastRefreshedBy</span>
        @* } *@
    }
</div>
```

### Countdown Implementation

**For 3-second countdown on Complete:**
- Use `Timer` or `PeriodicTimer` in Blazor
- Update countdown state: 3 → 2 → 1
- After countdown: `NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true)`
- `forceLoad: true` forces full page reload to get fresh content

## UI/UX Expert Decisions Needed

Please determine:

1. **Refresh Button Placement**
   - Near document title?
   - In a toolbar/action bar?
   - Floating action button?
   - With metadata display?

2. **Progress Spinner Style**
   - Full-page overlay (modal-style)?
   - Inline within document area?
   - Toast/notification style?
   - Use existing app spinner component

3. **Stage Message Display**
   - Below spinner?
   - Inside spinner?
   - Above spinner?
   - Use existing progress text patterns

4. **Countdown Visual**
   - Simple text: "Redirecting in 3..."
   - Animated countdown?
   - Progress bar depleting?
   - Use existing countdown patterns if any

5. **Metadata Display Location**
   - Document header?
   - Document footer?
   - Sidebar?
   - Collapsible info section?

6. **Refresh Button Disabled State**
   - Disable while refresh is running?
   - Hide while refresh is running?
   - Change text to "Refreshing..."?

## Expected User Flows

### Happy Path (User Stays on Page)
1. User viewing `/bylaws` document
2. Clicks "Refresh Document" button
3. Spinner appears: "Retrieving document from Google Docs..."
4. Spinner updates: "Uploading document to storage..."
5. Spinner updates: "Refresh complete! Redirecting in 3... 2... 1..."
6. Page reloads automatically
7. User sees updated document with new "Last Refreshed" timestamp

### User Navigates Away
1. User clicks "Refresh Document"
2. Spinner appears
3. User navigates to another page (e.g., home)
4. Refresh continues in background
5. When complete: toast appears "Bylaws refresh complete"
6. User clicks toast → navigates to `/bylaws`
7. Sees updated document

### Error Flow
1. User clicks "Refresh Document"
2. Spinner appears
3. Error occurs during sync
4. Spinner disappears
5. Error alert displays: "Failed to refresh document: [error message]"
6. User can dismiss alert
7. User can try refresh again

### Browser Refresh During Job
1. Refresh is running, spinner showing
2. User hits F5 or refreshes browser
3. Page reloads
4. On load, component checks for active job
5. Finds job in progress
6. Reconnects to SSE
7. Spinner reappears with current status
8. Continues normally

### Job Already Running (Another User)
1. User clicks "Refresh Document"
2. API returns 200 OK (not 202 Accepted) - job already running
3. Component connects to SSE for existing job
4. Shows spinner with current status
5. Could optionally show: "Refresh in progress (started by John Doe)"
6. Continues to completion normally

## Deliverables

Please provide:

1. **Blazor Component(s)**
   - Document page component with refresh functionality
   - Reusable if possible (works for both Bylaws and Tournament Rules)
   - Proper lifecycle management (OnInitialized, Dispose)

2. **JavaScript Interop**
   - SSE connection management
   - Proper EventSource setup and cleanup
   - Error handling for connection failures

3. **UI Layout**
   - Refresh button placement (with rationale)
   - Progress spinner implementation
   - Metadata display layout
   - Stage message styling

4. **State Management**
   - Component state for tracking refresh progress
   - Clean state transitions
   - Proper disposal/cleanup

5. **Error Handling**
   - SSE connection errors
   - API call errors
   - Timeout handling (30s idle, 5min max connection)

6. **Authorization Structure**
   - Comments showing where to add authorization checks
   - Make it trivial to add `@if (IsAuthorized)` later

7. **Toast Notification**
   - Integration with existing toast system
   - Clickable toast navigation
   - Proper message text

## Technical Constraints

- Must work with Blazor Web App (.NET 10)
- Use existing app UI/design standards
- Use JavaScript interop for SSE (EventSource API)
- No separate `/bylaws/refresh` page - everything on document page
- Must be reusable for multiple document types (Bylaws, Tournament Rules, future docs)
- Must handle reconnection on browser refresh
- Must cleanup SSE connections properly
- Authorization checks should be easy to add later (well-structured conditionals)

## Questions for Implementation

If you have any questions about:
- Existing app patterns (spinner, toast, error alerts)
- Component library or design system in use
- Preferred state management approach
- Existing authorization patterns to match
- Any other UI/UX standards

Please ask before implementing. Otherwise, use best practices for Blazor Web Apps with SSE and modern UI/UX patterns.
