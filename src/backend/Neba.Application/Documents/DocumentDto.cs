namespace Neba.Application.Documents;

/// <summary>
/// Data transfer object containing a document's content and its metadata.
/// </summary>
public sealed record DocumentDto
{
    /// <summary>
    /// The document content (for example, plain text or serialized data).
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Read-only metadata key/value pairs associated with the document.
    /// </summary>
    public required IReadOnlyDictionary<string, string> Metadata { get; init; }
}
