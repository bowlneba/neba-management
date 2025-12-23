using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Neba.Application.Documents;

namespace Neba.Infrastructure.Documents;

/// <summary>
/// Factory for creating SSE stream handlers for document refresh status updates.
/// </summary>
public static class DocumentRefreshSseStreamHandler
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Creates a delegate that handles SSE streaming for a specific document type.
    /// </summary>
    /// <param name="documentType">The document type (e.g., "bylaws", "tournament-rules").</param>
    /// <returns>A delegate that can be used with MapGet to handle SSE streaming.</returns>
#pragma warning disable CA1031 // Do not catch general exception types
    public static Delegate CreateStreamHandler(string documentType)
    {
        return async (
            DocumentRefreshChannelManager channelManager,
            HybridCache cache,
            ILogger<DocumentRefreshSseStreamHandler> logger,
            CancellationToken cancellationToken) =>
        {
            return Results.Stream(
                async stream =>
                {
                    var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                    try
                    {
                        logger.LogClientConnected(documentType);

                        // Send initial state if available from cache
                        await SendInitialStateAsync(documentType, writer, cache, cancellationToken);

                        // Get or create channel for this document type
                        ChannelReader<DocumentRefreshStatusEvent> channelReader = channelManager.GetOrCreateChannel(documentType);

                        // Stream updates from channel
                        await foreach (DocumentRefreshStatusEvent statusEvent in channelReader.ReadAllAsync(cancellationToken))
                        {
                            await WriteSseEventAsync(writer, statusEvent, cancellationToken);
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        logger.LogClientDisconnected(ex, documentType);
                    }
                    catch (Exception ex)
                    {
                        logger.LogStreamError(ex, documentType);
                    }
                    finally
                    {
                        channelManager.ReleaseListener(documentType);
                        await writer.DisposeAsync();
                    }
                },
                contentType: "text/event-stream");
        };
    }
#pragma warning restore CA1031

    private static async Task SendInitialStateAsync(
        string documentType,
        StreamWriter writer,
        HybridCache cache,
        CancellationToken cancellationToken)
    {
        string cacheKey = $"{documentType}:refresh:current";

        DocumentRefreshJobState? state = await cache.GetOrCreateAsync(
            cacheKey,
            _ => ValueTask.FromResult<DocumentRefreshJobState?>(null),
            tags: [documentType, "document-refresh-state"],
            cancellationToken: cancellationToken);

        if (state is not null)
        {
            var initialEvent = DocumentRefreshStatusEvent.FromStatus(state.Status, state.ErrorMessage);
            await WriteSseEventAsync(writer, initialEvent, cancellationToken);
        }
    }

    private static async Task WriteSseEventAsync(
        StreamWriter writer,
        DocumentRefreshStatusEvent statusEvent,
        CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(statusEvent, s_jsonOptions);
        await writer.WriteLineAsync($"data: {json}".AsMemory(), cancellationToken);
        await writer.WriteLineAsync(ReadOnlyMemory<char>.Empty, cancellationToken);
    }
}
