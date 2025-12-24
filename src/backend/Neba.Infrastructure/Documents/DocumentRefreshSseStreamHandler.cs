using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Neba.Application.Documents;

namespace Neba.Infrastructure.Documents;

/// <summary>
/// Factory for creating SSE stream handlers for document refresh status updates.
/// </summary>
public static class DocumentRefreshSseStreamHandler
{
    /// <summary>
    /// Creates a delegate that handles SSE streaming for a specific document type.
    /// </summary>
    /// <param name="documentType">The document type (e.g., "bylaws", "tournament-rules").</param>
    /// <returns>A delegate that can be used with MapGet to handle SSE streaming.</returns>
    public static Delegate CreateStreamHandler(string documentType)
    {
        return (
            DocumentRefreshChannels channels,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            // Get the channel for this document type
            Channel<DocumentRefreshStatusEvent> channel = channels.GetOrCreateChannel(documentType);

            // Create an async enumerable that yields initial state + channel updates
            IAsyncEnumerable<DocumentRefreshStatusEvent> events = GetEventsAsync(
                documentType,
                channel.Reader,
                cache,
                cancellationToken);

            // Use built-in SSE formatting
            return Task.FromResult(Results.ServerSentEvents(events));
        };
    }

    private static async IAsyncEnumerable<DocumentRefreshStatusEvent> GetEventsAsync(
        string documentType,
        ChannelReader<DocumentRefreshStatusEvent> channelReader,
        HybridCache cache,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Send the initial state if available from cache, otherwise send a default "ready" state
        string cacheKey = $"{documentType}:refresh:current";

        DocumentRefreshJobState? state = await cache.GetOrCreateAsync(
            cacheKey,
            _ => ValueTask.FromResult<DocumentRefreshJobState?>(null),
            tags: [documentType, "document-refresh-state"],
            cancellationToken: cancellationToken);

        if (state is not null)
        {
            var initialEvent = DocumentRefreshStatusEvent.FromStatus(state.Status, state.ErrorMessage);
            yield return initialEvent;
        }
        else
        {
            // No cached state means no refresh has been performed yet, send "Completed" as default
            var initialEvent = DocumentRefreshStatusEvent.FromStatus(DocumentRefreshStatus.Completed.Name);
            yield return initialEvent;
        }

        // Stream updates from the channel
        await foreach (DocumentRefreshStatusEvent statusEvent in channelReader.ReadAllAsync(cancellationToken))
        {
            yield return statusEvent;
        }
    }
}
