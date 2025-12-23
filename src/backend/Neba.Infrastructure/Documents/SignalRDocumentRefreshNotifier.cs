using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Neba.Application.Documents;

namespace Neba.Infrastructure.Documents;

internal sealed class SignalRDocumentRefreshNotifier(IHubContext<DocumentRefreshSignalRHub> hubContext) : IDocumentRefreshNotifier
{
    private readonly IHubContext<DocumentRefreshSignalRHub> _hubContext = hubContext;

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

        await _hubContext.Clients.Group(hubGroupName)
            .SendAsync("JobStatusChanged", status, errorMessage, cancellationToken);
    }
}
