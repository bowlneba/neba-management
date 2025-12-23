namespace Neba.Application.Documents;
/// <summary>
/// Notifies interested clients about document refresh progress and status changes.
/// </summary>
/// <remarks>
/// Implementations typically deliver notifications to connected clients (for example via SignalR).
/// The optional <c>hubGroupName</c> parameter can be used to target a specific group of clients.
/// </remarks>
public interface IDocumentRefreshNotifier
{
    /// <summary>
    /// Notify interested clients about a document refresh status change.
    /// </summary>
    /// <param name="hubGroupName">Optional hub group name to target a subset of clients (for example a SignalR group). If <c>null</c>, notify all connected clients.</param>
    /// <param name="status">The current <see cref="DocumentRefreshStatus"/> of the refresh operation.</param>
    /// <param name="errorMessage">Optional human-readable error message when <paramref name="status"/> indicates a failure.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the notification operation.</param>
    /// <returns>A task that completes when the notification has been sent or the operation is cancelled.</returns>
    Task NotifyStatusAsync(string? hubGroupName, DocumentRefreshStatus status, string? errorMessage = null, CancellationToken cancellationToken = default);
}
