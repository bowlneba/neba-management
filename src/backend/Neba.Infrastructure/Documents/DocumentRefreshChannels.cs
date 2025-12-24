using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Neba.Infrastructure.Documents;

/// <summary>
/// Simple dictionary-based channel storage for document refresh notifications.
/// </summary>
/// <remarks>
/// Each document type gets its own unbounded channel that lives for the application lifetime.
/// Channels are created on first access and never cleaned up (simple and sufficient for current needs).
/// </remarks>
public sealed class DocumentRefreshChannels
{
    private readonly ConcurrentDictionary<string, Channel<DocumentRefreshStatusEvent>> _channels = new();

    /// <summary>
    /// Gets or creates a channel for the specified document type.
    /// </summary>
    /// <param name="documentType">The document type (e.g., "bylaws", "tournament-rules").</param>
    /// <returns>The channel for the document type.</returns>
    public Channel<DocumentRefreshStatusEvent> GetOrCreateChannel(string documentType)
    {
        return _channels.GetOrAdd(documentType, _ =>
            Channel.CreateUnbounded<DocumentRefreshStatusEvent>(new UnboundedChannelOptions
            {
                SingleWriter = false,
                SingleReader = false
            }));
    }
}
