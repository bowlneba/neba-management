namespace Neba.Domain;

/// <summary>
/// Represents a file stored by the system, including its location and metadata.
/// </summary>
public sealed record StoredFile
{
    /// <summary>
    /// Gets the storage location or path of the file (for example, a blob URI or filesystem path).
    /// </summary>
    public string Location { get; init; }

    /// <summary>
    /// Gets the original or stored file name including extension.
    /// </summary>
    public string FileName { get; init; }

    /// <summary>
    /// Gets the MIME content type of the file (for example, "image/png").
    /// </summary>
    public string ContentType { get; init; }

    /// <summary>
    /// Gets the size of the file in bytes.
    /// </summary>
    public long SizeInBytes { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StoredFile"/> record with default values.
    /// </summary>
    public StoredFile()
    {
        Location = string.Empty;
        FileName = string.Empty;
        ContentType = string.Empty;
        SizeInBytes = 0;
    }

}
