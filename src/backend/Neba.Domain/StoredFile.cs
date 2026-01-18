namespace Neba.Domain;

/// <summary>
/// Represents a file stored by the system, including its location and metadata.
/// </summary>
public sealed record StoredFile
{
    /// <summary>
    /// Gets the storage location or path of the file (for example, a blob URI or filesystem path).
    /// </summary>
    public string Container { get; init; } = string.Empty;

    /// <summary>
    /// Gets the original or stored file name including extension.
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Gets the MIME content type of the file (for example, "image/png").
    /// </summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the size of the file in bytes.
    /// </summary>
    public long SizeInBytes { get; init; }
}
