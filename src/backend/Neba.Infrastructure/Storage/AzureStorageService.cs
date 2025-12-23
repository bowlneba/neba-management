using System.Buffers;
using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Neba.Application.Storage;

namespace Neba.Infrastructure.Storage;

internal sealed class AzureStorageService
    : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureStorageSettings _settings;

    public AzureStorageService(BlobServiceClient blobServiceClient, AzureStorageSettings settings)
    {
        _blobServiceClient = blobServiceClient;
        _settings = settings;
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
            Metadata = metadata ?? new Dictionary<string, string>()
        };

        await blobClient.UploadAsync(contentStream, uploadOptions, cancellationToken);

        return blobClient.Name;
    }

    public async Task<string> LargeUploadAsync(string containerName, string blobName, Stream content, string contentType, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = await GetOrCreateContainerAsync(containerName, cancellationToken);
        BlockBlobClient blockBlobClient = containerClient.GetBlockBlobClient(blobName);

        List<string> blockIds = [];
        byte[] buffer = ArrayPool<byte>.Shared.Rent(_settings.UploadChunkSizeBytes);

        try
        {
            int index = 0;
            int bytesRead;

            while((bytesRead = await content.ReadAsync(buffer.AsMemory(0, _settings.UploadChunkSizeBytes), cancellationToken)) > 0)
            {
                string blockId = Convert.ToBase64String(BitConverter.GetBytes(index));

                await using var blockStream = new MemoryStream(buffer, 0, bytesRead, writable: false, publiclyVisible: true);

                await blockBlobClient.StageBlockAsync(blockId, blockStream, cancellationToken: cancellationToken);

                blockIds.Add(blockId);
                index++;
            }

            var headers = new BlobHttpHeaders { ContentType = contentType };
            await blockBlobClient.CommitBlockListAsync(blockIds, headers, metadata, cancellationToken: cancellationToken);

            ArrayPool<byte>.Shared.Return(buffer);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return blockBlobClient.Name;
    }

    public async Task<Stream> GetStreamAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        Response<BlobDownloadStreamingResult> downloadResponse = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

        return downloadResponse.Value.Content;
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

    public async Task<ContentWithMetadata> GetContentWithMetadataAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        Response<BlobDownloadResult> downloadResponse = await blobClient.DownloadContentAsync(cancellationToken);

        var contentWithMetadata = new ContentWithMetadata
        {
            Content = downloadResponse.Value.Content.ToString(),
            Metadata = downloadResponse.Value.Details.Metadata.AsReadOnly()
        };

        return contentWithMetadata;
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
