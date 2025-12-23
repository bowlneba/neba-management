using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Neba.Application.Storage;

namespace Neba.Infrastructure.Storage;

internal sealed class AzureStorageService
    : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadAsync(string containerName, string blobName, string content, string contentType, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        return await UploadAsync(containerName, blobName, stream, contentType, metadata, cancellationToken);
    }

    public async Task<string> UploadAsync(string containerName, string blobName, Stream contentStream, string contentType, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = await GetOrCreateContainerAsync(containerName, cancellationToken);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
            Metadata = metadata
        };

        await blobClient.UploadAsync(contentStream, uploadOptions, cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> GetStreamAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        // NOTE: This call will throw Azure.RequestFailedException with Status == 404
        // if the blob does not exist. Consumers may prefer a different behavior
        // (e.g., returning null, a domain-specific exception, or checking
        // existence first via `ExistsAsync`). Consider wrapping this call in a
        // try/catch to translate 404 into a more appropriate result for your
        // domain or call `blobClient.ExistsAsync(cancellationToken)` beforehand.
        Stream response = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);

        return response;
    }

    public async Task<string> GetContentAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        // NOTE: `DownloadContentAsync` will throw Azure.RequestFailedException
        // with Status == 404 when the blob is missing. If callers expect a
        // non-throwing behavior, either perform an existence check
        // (`blobClient.ExistsAsync`) before downloading or catch
        // `RequestFailedException` and map 404 to a domain-appropriate result.
        Response<BlobDownloadResult> downloadResponse = await blobClient.DownloadContentAsync(cancellationToken);

        return downloadResponse.Value.Content.ToString();
    }

    public async Task<bool> ExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        return await blobClient.ExistsAsync(cancellationToken);
    }

    public async Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    private async Task<BlobContainerClient> GetOrCreateContainerAsync(string containerName, CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        return containerClient;
    }
}
