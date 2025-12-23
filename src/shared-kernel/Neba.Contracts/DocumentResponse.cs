namespace Neba.Contracts;

/// <summary>
/// Response DTO containing a document's content and associated metadata.
/// </summary>
public sealed record DocumentResponse
{
    /// <summary>
    /// The document content (for example, HTML or plain text).
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Read-only metadata key/value pairs describing the document.
    /// </summary>
    public required IReadOnlyDictionary<string, string> Metadata { get; init; }
}