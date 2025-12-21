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

    public async Task<string> UploadAsync(string containerName, string blobName, string content, string contentType, CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = await GetOrCreateContainerAsync(containerName, cancellationToken);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
            Conditions = null // allows overwrite

        };

        await blobClient.UploadAsync(stream, uploadOptions, cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<string> UploadAsync(string containerName, string blobName, Stream contentStream, string contentType, CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = await GetOrCreateContainerAsync(containerName, cancellationToken);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
            Conditions = null // allows overwrite
        };

        await blobClient.UploadAsync(contentStream, uploadOptions, cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> GetStreamAsync(string containerName, string blobName, CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        Stream response = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);

        return response;
    }

    public async Task<string> GetContentAsync(string containerName, string blobName, CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        Response<BlobDownloadResult> downloadResponse = await blobClient.DownloadContentAsync(cancellationToken);

        return downloadResponse.Value.Content.ToString();
    }

    public async Task<bool> ExistsAsync(string containerName, string blobName, CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        return await blobClient.ExistsAsync(cancellationToken);
    }

    public async Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken)
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
