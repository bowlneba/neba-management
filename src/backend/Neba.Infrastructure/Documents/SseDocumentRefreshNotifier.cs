using Microsoft.Extensions.Logging;
using Neba.Application.Documents;

namespace Neba.Infrastructure.Documents;

/// <summary>
/// SSE-based implementation of <see cref="IDocumentRefreshNotifier"/>.
/// Writes status updates to document-specific channels for streaming to connected clients.
/// </summary>
internal sealed class SseDocumentRefreshNotifier : IDocumentRefreshNotifier
{
    private readonly DocumentRefreshChannelManager _channelManager;
    private readonly ILogger<SseDocumentRefreshNotifier> _logger;

    public SseDocumentRefreshNotifier(
        DocumentRefreshChannelManager channelManager,
        ILogger<SseDocumentRefreshNotifier> logger)
    {
        _channelManager = channelManager;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task NotifyStatusAsync(
        string? hubGroupName,
        DocumentRefreshStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hubGroupName))
        {
            return;
        }

        // Extract document type from hubGroupName (e.g., "bylaws-refresh" -> "bylaws")
        string documentType = ExtractDocumentType(hubGroupName);

        var statusEvent = DocumentRefreshStatusEvent.FromStatus(status, errorMessage);

        await _channelManager.WriteToChannelAsync(documentType, statusEvent, cancellationToken);

        _logger.LogNotifiedChannel(documentType, status.Name);
    }

    /// <summary>
    /// Extracts the document type from a hub group name by removing the "-refresh" suffix.
    /// </summary>
    /// <param name="hubGroupName">The hub group name (e.g., "bylaws-refresh", "tournament-rules-refresh").</param>
    /// <returns>The document type (e.g., "bylaws", "tournament-rules").</returns>
    private static string ExtractDocumentType(string hubGroupName)
    {
        const string refreshSuffix = "-refresh";

        if (hubGroupName.EndsWith(refreshSuffix, StringComparison.OrdinalIgnoreCase))
        {
            return hubGroupName[..^refreshSuffix.Length];
        }

        // If no suffix, return as-is (defensive fallback)
        return hubGroupName;
    }
}
