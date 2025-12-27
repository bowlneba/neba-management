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
    /// Uploads text content to the specified <paramref name="container"/> and <paramref name="path"/>.
    /// </summary>
    /// <param name="container">The target container to store the content in.</param>
    /// <param name="path">The path within the container where the content will be stored.</param>
    /// <param name="content">The text content to upload.</param>
    /// <param name="contentType">The MIME media type of the content (for example MediaTypeNames.Text.Plain or "application/json").
    /// If <c>null</c> or empty, implementations may attempt to infer the type or apply a sensible default.</param>
    /// <param name="metadata">Optional metadata to associate with the stored content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A string that represents the stored content location or identifier (implementation-defined).</returns>
    Task<string> UploadAsync(string container, string path, string content, string contentType, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads binary content from a stream to the specified <paramref name="container"/> and <paramref name="path"/>.
    /// </summary>
    /// <param name="container">The target container to store the content in.</param>
    /// <param name="path">The path within the container where the content will be stored.</param>
    /// <param name="contentStream">A stream containing the binary content to upload. The caller retains ownership of the stream.</param>
    /// <param name="contentType">The MIME media type of the content (for example "application/octet-stream", "image/png").
    /// If <c>null</c> or empty, implementations may attempt to infer the type from the data or use a default.</param>
    /// <param name="metadata">Optional metadata to associate with the stored content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A string that represents the stored content location or identifier (implementation-defined).</returns>
    Task<string> UploadAsync(string container, string path, Stream contentStream, string contentType, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads large binary content from a stream to the specified <paramref name="container"/> and <paramref name="path"/>
    /// using a chunked block-blob strategy for improved efficiency and reliability.
    /// </summary>
    /// <param name="container">The target container to store the content in.</param>
    /// <param name="path">The path within the container where the content will be stored.</param>
    /// <param name="content">A stream containing the large binary content to upload. The stream is read in chunks and staged as blocks.
    /// The caller retains ownership of the stream.</param>
    /// <param name="contentType">The MIME media type of the content (for example "application/pdf", "video/mp4").
    /// If <c>null</c> or empty, implementations may use a default.</param>
    /// <param name="metadata">Optional metadata to associate with the stored content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A string that represents the stored content location or identifier (implementation-defined).</returns>
    /// <remarks>
    /// This method is optimized for large files by breaking the content into chunks (blocks), staging them individually,
    /// and committing them all at once. This approach provides better memory efficiency and reliability for large uploads
    /// compared to the standard <see cref="UploadAsync(string, string, Stream, string, IDictionary{string, string}?, CancellationToken)"/> method.
    /// Use this method when uploading files larger than a few megabytes.
    /// </remarks>
    Task<string> LargeUploadAsync(string container, string path, Stream content, string contentType, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the content at the specified path as a string.
    /// </summary>
    /// <param name="container">The container where the content is located.</param>
    /// <param name="path">The path of the content to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The content as a string.</returns>
    Task<string> GetContentAsync(string container, string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the content and metadata at the specified path.
    /// </summary>
    /// <param name="container">The container where the content is located.</param>
    /// <param name="path">The path of the content to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="DocumentDto"/> containing the content data and its metadata.</returns>
    Task<DocumentDto> GetContentWithMetadataAsync(string container, string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the content at the specified path as a <see cref="Stream"/> for reading.
    /// Callers are responsible for disposing the returned stream when finished.
    /// </summary>
    /// <param name="container">The container where the content is located.</param>
    /// <param name="path">The path of the content to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Stream"/> containing the content data.</returns>
    Task<Stream> GetStreamAsync(string container, string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether content exists at the specified path in the given container.
    /// </summary>
    /// <param name="container">The container to check.</param>
    /// <param name="path">The path to check for existence.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the content exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsAsync(string container, string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the content at the specified path from the given container.
    /// </summary>
    /// <param name="container">The container from which to delete the content.</param>
    /// <param name="path">The path of the content to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteAsync(string container, string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the URI of the content at the specified path in the given container.
    /// </summary>
    /// <param name="container">The container where the content is located.</param>
    /// <param name="path">The path of the content.</param>
    /// <returns>A <see cref="Uri"/> representing the content's location.</returns>
    Uri GetBlobUri(string container, string path);
}
