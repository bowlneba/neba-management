namespace Neba.Application.Storage;

/// <summary>
/// Represents content together with an associated metadata collection.
/// </summary>
public sealed record ContentWithMetadata
{
    /// <summary>
    /// Gets the content code or identifier. This value is required.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Gets a read-only dictionary of metadata key/value pairs associated with the content.
    /// The dictionary is empty by default.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
