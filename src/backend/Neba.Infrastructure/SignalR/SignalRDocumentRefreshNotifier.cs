using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Neba.Application.Documents;

namespace Neba.Infrastructure.SignalR;

internal sealed class SignalRDocumentRefreshNotifier(
    IServiceProvider serviceProvider,
    ILogger<SignalRDocumentRefreshNotifier> logger)
        : IDocumentRefreshNotifier
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<SignalRDocumentRefreshNotifier> _logger = logger;
    private readonly Dictionary<string, Type> _hubRegistry = [];

    public void RegisterHub<THub>(string groupName)
        where THub : Hub
        => _hubRegistry[groupName] = typeof(THub);

    public async Task NotifyStatusAsync(
        string? hubGroupName,
        DocumentRefreshStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hubGroupName))
        {
            _logger.LogNoHubGroupName();

            return;
        }

        if (!_hubRegistry.TryGetValue(hubGroupName, out var hubType))
        {
            _logger.LogHubGroupNotRegistered(hubGroupName);

            return;
        }

        try
        {
            var hubContextType = typeof(IHubContext<>).MakeGenericType(hubType);
            var hubContext = _serviceProvider.GetService(hubContextType) as IHubContext<Hub>;

            if (hubContext is null)
            {
                return;
            }

            await hubContext.Clients.Group(hubGroupName)
                .SendAsync("DocumentRefreshStatusChanged", status, errorMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogErrorSendingNotification(ex, hubGroupName);

            throw;
        }
    }
}
