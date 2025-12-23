using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Neba.Application.Documents;

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
    /// <param name="contentType">The MIME media type of the content (for example MediaTypeNames.Text.Plain or "application/json").
    /// If <c>null</c> or empty, implementations may attempt to infer the type or apply a sensible default.</param>
    /// <param name="metadata">Optional metadata to associate with the blob.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A string that represents the stored blob location or identifier (implementation-defined).</returns>
    Task<string> UploadAsync(string containerName, string blobName, string content, string contentType, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads binary content from a stream to the specified <paramref name="containerName"/> and <paramref name="blobName"/>.
    /// </summary>
    /// <param name="containerName">The target container to store the blob in.</param>
    /// <param name="blobName">The name/key of the blob to create or overwrite.</param>
    /// <param name="contentStream">A stream containing the binary content to upload. The caller retains ownership of the stream.</param>
    /// <param name="contentType">The MIME media type of the content (for example "application/octet-stream", "image/png").
    /// If <c>null</c> or empty, implementations may attempt to infer the type from the data or use a default.</param>
    /// <param name="metadata">Optional metadata to associate with the blob.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A string that represents the stored blob location or identifier (implementation-defined).</returns>
    Task<string> UploadAsync(string containerName, string blobName, Stream contentStream, string contentType, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads large binary content from a stream to the specified <paramref name="containerName"/> and <paramref name="blobName"/>
    /// using a chunked block-blob strategy for improved efficiency and reliability.
    /// </summary>
    /// <param name="containerName">The target container to store the blob in.</param>
    /// <param name="blobName">The name/key of the blob to create or overwrite.</param>
    /// <param name="content">A stream containing the large binary content to upload. The stream is read in chunks and staged as blocks.
    /// The caller retains ownership of the stream.</param>
    /// <param name="contentType">The MIME media type of the content (for example "application/pdf", "video/mp4").
    /// If <c>null</c> or empty, implementations may use a default.</param>
    /// <param name="metadata">Optional metadata to associate with the blob.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A string that represents the stored blob location or identifier (implementation-defined).</returns>
    /// <remarks>
    /// This method is optimized for large files by breaking the content into chunks (blocks), staging them individually,
    /// and committing them all at once. This approach provides better memory efficiency and reliability for large uploads
    /// compared to the standard <see cref="UploadAsync(string, string, Stream, string, IDictionary{string, string}?, CancellationToken)"/> method.
    /// Use this method when uploading files larger than a few megabytes.
    /// </remarks>
    Task<string> LargeUploadAsync(string containerName, string blobName, Stream content, string contentType, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the content of the specified blob as a string.
    /// </summary>
    /// <param name="containerName">The container where the blob is located.</param>
    /// <param name="blobName">The name/key of the blob to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The blob content as a string.</returns>
    Task<string> GetContentAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the content and metadata of the specified blob.
    /// </summary>
    /// <param name="containerName">The container where the blob is located.</param>
    /// <param name="blobName">The name/key of the blob to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="DocumentDto"/> containing the blob data and its metadata.</returns>
    Task<DocumentDto> GetContentWithMetadataAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the specified blob as a <see cref="Stream"/> for reading.
    /// Callers are responsible for disposing the returned stream when finished.
    /// </summary>
    /// <param name="containerName">The container where the blob is located.</param>
    /// <param name="blobName">The name/key of the blob to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Stream"/> containing the blob data.</returns>
    Task<Stream> GetStreamAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a blob with the specified name exists in the given container.
    /// </summary>
    /// <param name="containerName">The container to check.</param>
    /// <param name="blobName">The name/key of the blob to check for existence.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the blob exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified blob from the given container.
    /// </summary>
    /// <param name="containerName">The container from which to delete the blob.</param>
    /// <param name="blobName">The name/key of the blob to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken = default);
}
