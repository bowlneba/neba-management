namespace Neba.Application.Documents;

/// <summary>
/// Response returned after initiating or querying a document refresh operation.
/// </summary>
public sealed record DocumentRefreshResponse
{
    /// <summary>
    /// The identifier of the background job responsible for the refresh.
    /// </summary>
    public required string JobId { get; init; }

    /// <summary>
    /// The current <see cref="RefreshStatus"/> of the refresh operation.
    /// </summary>
    public required RefreshStatus Status { get; init; }

    /// <summary>
    /// Optional human-readable message providing additional context such as
    /// error details or progress information.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Identifier of the user or system that triggered the refresh.
    /// </summary>
    public required string TriggeredBy { get; init; }
}
