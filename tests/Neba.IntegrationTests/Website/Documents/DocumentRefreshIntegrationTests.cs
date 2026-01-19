using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Neba.Contracts;
using Neba.IntegrationTests.Infrastructure;

namespace Neba.IntegrationTests.Website.Documents;

[Trait("Category", "Integration")]
[Trait("Component", "Website.Documents")]

public sealed class DocumentRefreshIntegrationTests(ITestOutputHelper output)
    : ApiTestsBase
{
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
            output.WriteLine($"[Expected] IOException during SSE stream cleanup: {ex.Message}");
        }
        catch (HttpRequestException ex) when (ex.InnerException is IOException)
        {
            // Expected when closing SSE stream - inner IOException from HTTP layer
            output.WriteLine($"[Expected] HttpRequestException with IOException during SSE cleanup: {ex.Message}");
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
            output.WriteLine($"[Expected] IOException during SSE stream cleanup: {ex.Message}");
        }
        catch (HttpRequestException ex) when (ex.InnerException is IOException)
        {
            // Expected when closing SSE stream - inner IOException from HTTP layer
            output.WriteLine($"[Expected] HttpRequestException with IOException during SSE cleanup: {ex.Message}");
        }
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
#pragma warning restore S3459, S1144, CA1812
}
