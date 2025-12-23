using Neba.Application.Documents;

namespace Neba.Infrastructure.Documents.Sse;

/// <summary>
/// Represents a document refresh status event for SSE streaming.
/// </summary>
public sealed record DocumentRefreshStatusEvent
{
    /// <summary>
    /// The current status of the refresh operation.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Optional error message when status indicates a failure.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Creates an event from a DocumentRefreshStatus.
    /// </summary>
    /// <param name="status">The document refresh status.</param>
    /// <param name="errorMessage">Optional error message.</param>
    /// <returns>A new <see cref="DocumentRefreshStatusEvent"/> instance.</returns>
    public static DocumentRefreshStatusEvent FromStatus(
        DocumentRefreshStatus status,
        string? errorMessage = null)
    {
        ArgumentNullException.ThrowIfNull(status);

        return new DocumentRefreshStatusEvent
        {
            Status = status.Name,
            ErrorMessage = errorMessage,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}
