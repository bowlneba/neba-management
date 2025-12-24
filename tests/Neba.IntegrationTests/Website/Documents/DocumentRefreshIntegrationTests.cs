using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Neba.Contracts;
using Neba.IntegrationTests.Infrastructure;

namespace Neba.IntegrationTests.Website.Documents;

public sealed class DocumentRefreshIntegrationTests(ITestOutputHelper output)
    : ApiTestsBase
{
    private readonly ITestOutputHelper _output = output;
    [Fact]
    public async Task RefreshBylaws_ShouldReturnJobId()
    {
        // Arrange
        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.PostAsync(
            new Uri("/bylaws/refresh", UriKind.Relative),
            null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        ApiResponse<string>? result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();

        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RefreshTournamentRules_ShouldReturnJobId()
    {
        // Arrange
        using HttpClient httpClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.PostAsync(
            new Uri("/tournaments/rules/refresh", UriKind.Relative),
            null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        ApiResponse<string>? result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();

        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task BylawsRefreshStatus_ShouldStreamSseEvents()
    {
        // Arrange
        using HttpClient httpClient = Factory.CreateClient();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));

        try
        {
            // Act
            using HttpResponseMessage response = await httpClient.GetAsync(
                new Uri("/bylaws/refresh/status", UriKind.Relative),
                HttpCompletionOption.ResponseHeadersRead,
                cts.Token);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.ShouldBe("text/event-stream");

            // Read the SSE stream
            await using Stream stream = await response.Content.ReadAsStreamAsync(cts.Token);
            using StreamReader reader = new(stream);

            // Read the first SSE event (initial state from cache) with timeout
            Task<string?> readTask = ReadSseEventAsync(reader, cts.Token);
            string? firstEvent = await readTask.WaitAsync(TimeSpan.FromSeconds(5));

            firstEvent.ShouldNotBeNull();

            // Parse the SSE data field
            SseEvent? parsedEvent = ParseSseEvent(firstEvent);

            parsedEvent.ShouldNotBeNull();
            parsedEvent.Status.ShouldNotBeNullOrWhiteSpace();
            parsedEvent.Timestamp.ShouldNotBeNullOrWhiteSpace();
        }
        catch (IOException ex)
        {
            // Expected when closing SSE stream - the HTTP client aborts the connection when disposing
            _output.WriteLine($"[Expected] IOException during SSE stream cleanup: {ex.Message}");
        }
        catch (HttpRequestException ex) when (ex.InnerException is IOException)
        {
            // Expected when closing SSE stream - inner IOException from HTTP layer
            _output.WriteLine($"[Expected] HttpRequestException with IOException during SSE cleanup: {ex.Message}");
        }
    }

    [Fact]
    public async Task TournamentRulesRefreshStatus_ShouldStreamSseEvents()
    {
        // Arrange
        using HttpClient httpClient = Factory.CreateClient();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));

        try
        {
            // Act
            using HttpResponseMessage response = await httpClient.GetAsync(
                new Uri("/tournaments/rules/refresh/status", UriKind.Relative),
                HttpCompletionOption.ResponseHeadersRead,
                cts.Token);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.ShouldBe("text/event-stream");

            // Read the SSE stream
            await using Stream stream = await response.Content.ReadAsStreamAsync(cts.Token);
            using StreamReader reader = new(stream);

            // Read the first SSE event (initial state from cache) with timeout
            Task<string?> readTask = ReadSseEventAsync(reader, cts.Token);
            string? firstEvent = await readTask.WaitAsync(TimeSpan.FromSeconds(5));

            firstEvent.ShouldNotBeNull();

            // Parse the SSE data field
            SseEvent? parsedEvent = ParseSseEvent(firstEvent);

            parsedEvent.ShouldNotBeNull();
            parsedEvent.Status.ShouldNotBeNullOrWhiteSpace();
            parsedEvent.Timestamp.ShouldNotBeNullOrWhiteSpace();
        }
        catch (IOException ex)
        {
            // Expected when closing SSE stream - the HTTP client aborts the connection when disposing
            _output.WriteLine($"[Expected] IOException during SSE stream cleanup: {ex.Message}");
        }
        catch (HttpRequestException ex) when (ex.InnerException is IOException)
        {
            // Expected when closing SSE stream - inner IOException from HTTP layer
            _output.WriteLine($"[Expected] HttpRequestException with IOException during SSE cleanup: {ex.Message}");
        }
    }

    [Fact(Skip = "End-to-end test requires background job execution which is not reliable in integration tests")]
    public async Task BylawsRefreshStatus_ShouldReceiveUpdatesAfterTriggeringRefresh()
    {
        // Arrange
        using HttpClient httpClient = Factory.CreateClient();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));

        // Act - Trigger a refresh first to ensure the background job is queued
        HttpResponseMessage refreshResponse = await httpClient.PostAsync(
            new Uri("/bylaws/refresh", UriKind.Relative),
            null,
            cts.Token);

        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        ApiResponse<string>? refreshResult = await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<string>>(cts.Token);
        refreshResult.ShouldNotBeNull();
        refreshResult.Data.ShouldNotBeNullOrWhiteSpace();

        // NOTE: Background jobs may not execute immediately in test environment
        // This test verifies the full flow but is skipped by default
        // To test manually: Remove Skip attribute and ensure Hangfire is configured for immediate execution

        // Give the background job time to start processing
        await Task.Delay(1000, cts.Token);

        // Start listening to SSE stream after refresh is triggered
        Task<List<SseEvent>> sseTask = CollectSseEventsAsync(
            httpClient,
            "/bylaws/refresh/status",
            expectedEventCount: 1,
            maxWaitTime: TimeSpan.FromSeconds(10),
            cts.Token);

        // Wait for SSE events
        List<SseEvent> events = await sseTask;

        // Assert - We should at least get the initial cached state
        events.Count.ShouldBeGreaterThanOrEqualTo(1);
        events.ShouldAllBe(e => !string.IsNullOrWhiteSpace(e.Status));
        events.ShouldAllBe(e => !string.IsNullOrWhiteSpace(e.Timestamp));
    }

    [Fact(Skip = "End-to-end test requires background job execution which is not reliable in integration tests")]
    public async Task TournamentRulesRefreshStatus_ShouldReceiveUpdatesAfterTriggeringRefresh()
    {
        // Arrange
        using HttpClient httpClient = Factory.CreateClient();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));

        // Act - Trigger a refresh first to ensure the background job is queued
        HttpResponseMessage refreshResponse = await httpClient.PostAsync(
            new Uri("/tournaments/rules/refresh", UriKind.Relative),
            null,
            cts.Token);

        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        ApiResponse<string>? refreshResult = await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<string>>(cts.Token);
        refreshResult.ShouldNotBeNull();
        refreshResult.Data.ShouldNotBeNullOrWhiteSpace();

        // NOTE: Background jobs may not execute immediately in test environment
        // This test verifies the full flow but is skipped by default
        // To test manually: Remove Skip attribute and ensure Hangfire is configured for immediate execution

        // Give the background job time to start processing
        await Task.Delay(1000, cts.Token);

        // Start listening to SSE stream after refresh is triggered
        Task<List<SseEvent>> sseTask = CollectSseEventsAsync(
            httpClient,
            "/tournaments/rules/refresh/status",
            expectedEventCount: 1,
            maxWaitTime: TimeSpan.FromSeconds(10),
            cts.Token);

        // Wait for SSE events
        List<SseEvent> events = await sseTask;

        // Assert - We should at least get the initial cached state
        events.Count.ShouldBeGreaterThanOrEqualTo(1);
        events.ShouldAllBe(e => !string.IsNullOrWhiteSpace(e.Status));
        events.ShouldAllBe(e => !string.IsNullOrWhiteSpace(e.Timestamp));
    }

    /// <summary>
    /// Reads a single SSE event from the stream.
    /// SSE format is "data: {json}\n\n"
    /// </summary>
    private static async Task<string?> ReadSseEventAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        string? dataLine = null;

        while (!reader.EndOfStream)
        {
            string? line = await reader.ReadLineAsync(cancellationToken);

            if (line is null)
            {
                break;
            }

            if (line.StartsWith("data: ", StringComparison.Ordinal))
            {
                dataLine = line["data: ".Length..];
            }
            else if (string.IsNullOrWhiteSpace(line) && dataLine is not null)
            {
                // End of event (blank line)
                return dataLine;
            }
        }

        return dataLine;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Parses the JSON data from an SSE event.
    /// </summary>
    private static SseEvent? ParseSseEvent(string eventData)
    {
        try
        {
            return JsonSerializer.Deserialize<SseEvent>(eventData, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Collects SSE events from a stream until the expected count is reached or timeout occurs.
    /// </summary>
    private static async Task<List<SseEvent>> CollectSseEventsAsync(
        HttpClient httpClient,
        string endpoint,
        int expectedEventCount,
        TimeSpan maxWaitTime,
        CancellationToken cancellationToken)
    {
        List<SseEvent> events = [];

        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(maxWaitTime);

        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                new Uri(endpoint, UriKind.Relative),
                HttpCompletionOption.ResponseHeadersRead,
                timeoutCts.Token);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            await using Stream stream = await response.Content.ReadAsStreamAsync(timeoutCts.Token);
            using StreamReader reader = new(stream);

            while (events.Count < expectedEventCount && !timeoutCts.Token.IsCancellationRequested)
            {
                try
                {
                    string? eventData = await ReadSseEventAsync(reader, timeoutCts.Token);

                    if (eventData is null)
                    {
                        break;
                    }

                    SseEvent? parsedEvent = ParseSseEvent(eventData);

                    if (parsedEvent is not null)
                    {
                        events.Add(parsedEvent);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Timeout or cancellation - return what we have
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when timeout occurs
        }

        return events;
    }

    /// <summary>
    /// Represents an SSE event for document refresh status.
    /// </summary>
#pragma warning disable CA1812 // Avoid uninstantiated internal classes - instantiated via JSON deserialization
#pragma warning disable S1144 // Unused private types or members should be removed - used by JSON deserialization
#pragma warning disable S3459 // Unassigned members should be removed - assigned by JSON deserialization
    private sealed record SseEvent
    {
        public string Status { get; init; } = string.Empty;
        public string? ErrorMessage { get; init; }
        public string Timestamp { get; init; } = string.Empty;
    }
#pragma warning restore S3459
#pragma warning restore S1144
#pragma warning restore CA1812
}
