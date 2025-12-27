using System.Buffers;
using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Neba.Application.Documents;
using Neba.Application.Storage;

namespace Neba.Infrastructure.Storage;

internal sealed class AzureStorageService(BlobServiceClient blobServiceClient, AzureStorageSettings settings)
        : IStorageService
{

    public async Task<string> UploadAsync(string container, string path, string content, string contentType, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        return await UploadAsync(container, path, stream, contentType, metadata, cancellationToken);
    }

    public async Task<string> UploadAsync(string container, string path, Stream contentStream, string contentType, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = await GetOrCreateContainerAsync(container, cancellationToken);
        BlobClient blobClient = containerClient.GetBlobClient(path);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
            Metadata = metadata ?? new Dictionary<string, string>()
        };

        await blobClient.UploadAsync(contentStream, uploadOptions, cancellationToken);

        return blobClient.Name;
    }

    public async Task<string> LargeUploadAsync(string container, string path, Stream content, string contentType, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = await GetOrCreateContainerAsync(container, cancellationToken);
        BlockBlobClient blockBlobClient = containerClient.GetBlockBlobClient(path);

        List<string> blockIds = [];
        byte[] buffer = ArrayPool<byte>.Shared.Rent(settings.UploadChunkSizeBytes);

        try
        {
            int index = 0;
            int bytesRead;

            while ((bytesRead = await content.ReadAsync(buffer.AsMemory(0, settings.UploadChunkSizeBytes), cancellationToken)) > 0)
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

    public async Task<Stream> GetStreamAsync(string container, string path, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(container);
        BlobClient blobClient = containerClient.GetBlobClient(path);

        Response<BlobDownloadStreamingResult> downloadResponse = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

        return downloadResponse.Value.Content;
    }

    public async Task<string> GetContentAsync(string container, string path, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(container);
        BlobClient blobClient = containerClient.GetBlobClient(path);

        // NOTE: `DownloadContentAsync` will throw Azure.RequestFailedException
        // with Status == 404 when the blob is missing. If callers expect a
        // non-throwing behavior, either perform an existence check
        // (`blobClient.ExistsAsync`) before downloading or catch
        // `RequestFailedException` and map 404 to a domain-appropriate result.
        Response<BlobDownloadResult> downloadResponse = await blobClient.DownloadContentAsync(cancellationToken);

        return downloadResponse.Value.Content.ToString();
    }

    public async Task<DocumentDto> GetContentWithMetadataAsync(string container, string path, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(container);
        BlobClient blobClient = containerClient.GetBlobClient(path);

        Response<BlobDownloadResult> downloadResponse = await blobClient.DownloadContentAsync(cancellationToken);

        var contentWithMetadata = new DocumentDto
        {
            Content = downloadResponse.Value.Content.ToString(),
            Metadata = downloadResponse.Value.Details.Metadata.AsReadOnly()
        };

        return contentWithMetadata;
    }

    public async Task<bool> ExistsAsync(string container, string path, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(container);
        BlobClient blobClient = containerClient.GetBlobClient(path);

        return await blobClient.ExistsAsync(cancellationToken);
    }

    public async Task DeleteAsync(string container, string path, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(container);
        BlobClient blobClient = containerClient.GetBlobClient(path);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public Uri GetBlobUri(string container, string path)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(container);
        BlobClient blobClient = containerClient.GetBlobClient(path);

        return blobClient.Uri;
    }

    private async Task<BlobContainerClient> GetOrCreateContainerAsync(string container, CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(container);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        return containerClient;
    }
}
