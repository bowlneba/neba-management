using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Hybrid;
using Neba.Application.Documents;

namespace Neba.Infrastructure.Documents;

internal sealed class DocumentRefreshSignalRHub
    : Hub
{
    private readonly HybridCache _cache;

    public DocumentRefreshSignalRHub(HybridCache cache)
    {
        _cache = cache;
    }

    public async Task JoinRefreshAsync(string documentKey, CancellationToken cancellationToken = default)
    {
        string groupName = $"{documentKey}-refresh";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName, cancellationToken);

        // Send current state to newly connected user
        string cacheKey = $"{documentKey}:refresh:current";
        DocumentRefreshJobState? state = await _cache.GetOrCreateAsync(
            cacheKey,
            _ => ValueTask.FromResult<DocumentRefreshJobState?>(null),
            tags: [documentKey, "document-refresh-state"],
            cancellationToken: cancellationToken);

        if (state is not null)
        {
            await Clients.Caller.SendAsync(
                "DocumentRefreshStatusChanged",
                state.Status,
                state.ErrorMessage,
                cancellationToken: cancellationToken);
        }
    }

    public async Task LeaveRefreshAsync(string documentKey, CancellationToken cancellationToken = default)
    {
        string groupName = $"{documentKey}-refresh";

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName, cancellationToken);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // No-op for now
        await base.OnDisconnectedAsync(exception);
    }
}
