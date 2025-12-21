using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Neba.Application.Storage;

/// <summary>
/// Provides an abstraction for storing and retrieving blobs or content
/// within named containers. Implementations may target cloud blob stores,
/// local file storage, or other storage backends.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Uploads text content to the specified <paramref name="containerName"/> and <paramref name="blobName"/>.
    /// </summary>
    /// <param name="containerName">The target container to store the blob in.</param>
    /// <param name="blobName">The name/key of the blob to create or overwrite.</param>
    /// <param name="content">The text content to upload.</param>
    /// <param name="contentType">The MIME media type of the content (for example "text/plain" or "application/json").
    /// If <c>null</c> or empty, implementations may attempt to infer the type or apply a sensible default.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A string that represents the stored blob location or identifier (implementation-defined).</returns>
    Task<string> UploadAsync(string containerName, string blobName, string content, string contentType,  CancellationToken cancellationToken);

    /// <summary>
    /// Uploads binary content from a stream to the specified <paramref name="containerName"/> and <paramref name="blobName"/>.
    /// </summary>
    /// <param name="containerName">The target container to store the blob in.</param>
    /// <param name="blobName">The name/key of the blob to create or overwrite.</param>
    /// <param name="contentStream">A stream containing the binary content to upload. The caller retains ownership of the stream.</param>
    /// <param name="contentType">The MIME media type of the content (for example "application/octet-stream", "image/png").
    /// If <c>null</c> or empty, implementations may attempt to infer the type from the data or use a default.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A string that represents the stored blob location or identifier (implementation-defined).</returns>
    Task<string> UploadAsync(string containerName, string blobName, Stream contentStream, string contentType, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the content of the specified blob as a string.
    /// </summary>
    /// <param name="containerName">The container where the blob is located.</param>
    /// <param name="blobName">The name/key of the blob to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The blob content as a string.</returns>
    Task<string> GetContentAsync(string containerName, string blobName, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the specified blob as a <see cref="Stream"/> for reading.
    /// Callers are responsible for disposing the returned stream when finished.
    /// </summary>
    /// <param name="containerName">The container where the blob is located.</param>
    /// <param name="blobName">The name/key of the blob to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Stream"/> containing the blob data.</returns>
    Task<Stream> GetStreamAsync(string containerName, string blobName, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether a blob with the specified name exists in the given container.
    /// </summary>
    /// <param name="containerName">The container to check.</param>
    /// <param name="blobName">The name/key of the blob to check for existence.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the blob exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsAsync(string containerName, string blobName, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the specified blob from the given container.
    /// </summary>
    /// <param name="containerName">The container from which to delete the blob.</param>
    /// <param name="blobName">The name/key of the blob to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken);
}
