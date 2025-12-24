namespace Neba.Infrastructure.Storage;

internal sealed class AzureStorageSettings
{
    /// <summary>
    /// The Blob Service URI for Azure Storage when using managed identity.
    /// </summary>
    public string? BlobServiceUri { get; set; }

    /// <summary>
    /// The chunk size in bytes for uploading large blobs.
    /// </summary>
    public int UploadChunkSizeBytes { get; set; } = 8 * 1024 * 1024; // Default to 8 MB
}
