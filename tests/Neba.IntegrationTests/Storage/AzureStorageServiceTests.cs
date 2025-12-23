using System.Net.Mime;
using System.Text;
using Azure.Storage.Blobs;
using Neba.Infrastructure.Storage;
using Neba.Tests.Storage;
using Shouldly;
using static Neba.Tests.Storage.BlobMetadataFactory;

namespace Neba.IntegrationTests.Storage;

/// <summary>
/// Integration tests for AzureStorageService using Azurite test container.
/// Tests verify blob upload, download, existence checks, and deletion operations.
/// </summary>
public sealed class AzureStorageServiceTests : IAsyncLifetime
{
    private AzureStorageContainer _storageContainer = null!;
    private AzureStorageService _storageService = null!;

    private const string TestContainerName = "test-container";
    private const string TestBlobName = "test-blob.txt";

    /// <summary>
    /// Gets a BlobServiceClient instance for direct Azure operations.
    /// </summary>
    private BlobServiceClient GetBlobServiceClient()
        => new(_storageContainer.ConnectionString);

    /// <summary>
    /// Gets a BlobClient for direct blob operations and property verification.
    /// </summary>
    private BlobClient GetBlobClient(string containerName, string blobName)
    {
        var blobServiceClient = GetBlobServiceClient();
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        return containerClient.GetBlobClient(blobName);
    }

    public async ValueTask InitializeAsync()
    {
        _storageContainer = new AzureStorageContainer();
        await _storageContainer.InitializeAsync();

        var blobServiceClient = new BlobServiceClient(_storageContainer.ConnectionString);
        _storageService = new AzureStorageService(blobServiceClient);
    }

    public async ValueTask DisposeAsync()
    {
        // Clean up all test containers before disposing
        var blobServiceClient = GetBlobServiceClient();
        await foreach (var container in blobServiceClient.GetBlobContainersAsync())
        {
            await blobServiceClient.DeleteBlobContainerAsync(container.Name);
        }

        await _storageContainer.DisposeAsync();
    }

    #region Upload Tests

    [Fact]
    public async Task UploadAsync_WithStringContent_ShouldUploadSuccessfully()
    {
        // Arrange
        const string content = "Hello, Azurite!";
        const string contentType = MediaTypeNames.Text.Plain;

        // Act
        string blobUri = await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            content,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();
        blobUri.ShouldContain(TestContainerName);
        blobUri.ShouldContain(TestBlobName);

        // Verify blob was actually created
        bool exists = await _storageService.ExistsAsync(TestContainerName, TestBlobName, CancellationToken.None);
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task UploadAsync_WithStreamContent_ShouldUploadSuccessfully()
    {
        // Arrange
        const string content = "Stream upload test content";
        const string contentType = "application/octet-stream";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        string blobUri = await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            stream,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();
        blobUri.ShouldContain(TestContainerName);
        blobUri.ShouldContain(TestBlobName);

        // Verify blob was actually created
        bool exists = await _storageService.ExistsAsync(TestContainerName, TestBlobName, CancellationToken.None);
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task UploadAsync_ShouldOverwriteExistingBlob()
    {
        // Arrange
        const string originalContent = "Original content";
        const string updatedContent = "Updated content";
        const string contentType = MediaTypeNames.Text.Plain;

        await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            originalContent,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Act
        await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            updatedContent,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        string retrievedContent = await _storageService.GetContentAsync(
            TestContainerName,
            TestBlobName,
            CancellationToken.None);

        retrievedContent.ShouldBe(updatedContent);
    }

    [Fact]
    public async Task UploadAsync_ShouldCreateContainerIfNotExists()
    {
        // Arrange
        const string newContainerName = "auto-created-container";
        const string content = "Content in new container";
        const string contentType = MediaTypeNames.Text.Plain;

        // Act
        string blobUri = await _storageService.UploadAsync(
            newContainerName,
            TestBlobName,
            content,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify container was created
        var containerClient = GetBlobServiceClient().GetBlobContainerClient(newContainerName);
        bool containerExists = await containerClient.ExistsAsync();
        containerExists.ShouldBeTrue();
    }

    #endregion

    #region Download Tests

    [Fact]
    public async Task GetContentAsync_ShouldReturnUploadedContent()
    {
        // Arrange
        const string expectedContent = "Test content for download";
        const string contentType = MediaTypeNames.Text.Plain;

        await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            expectedContent,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Act
        string actualContent = await _storageService.GetContentAsync(
            TestContainerName,
            TestBlobName,
            CancellationToken.None);

        // Assert
        actualContent.ShouldBe(expectedContent);
    }

    [Fact]
    public async Task GetStreamAsync_ShouldReturnReadableStream()
    {
        // Arrange
        const string expectedContent = "Stream download test content";
        const string contentType = "application/octet-stream";

        await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            expectedContent,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Act
        await using Stream stream = await _storageService.GetStreamAsync(
            TestContainerName,
            TestBlobName,
            CancellationToken.None);

        // Assert
        stream.ShouldNotBeNull();
        stream.CanRead.ShouldBeTrue();

        using var reader = new StreamReader(stream);
        string actualContent = await reader.ReadToEndAsync();
        actualContent.ShouldBe(expectedContent);
    }

    [Fact]
    public async Task GetStreamAsync_WithBinaryContent_ShouldPreserveData()
    {
        // Arrange
        byte[] expectedBytes = [0x01, 0x02, 0x03, 0x04, 0x05];
        const string contentType = "application/octet-stream";

        using var uploadStream = new MemoryStream(expectedBytes);
        await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            uploadStream,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Act
        await using Stream downloadStream = await _storageService.GetStreamAsync(
            TestContainerName,
            TestBlobName,
            CancellationToken.None);

        using var memoryStream = new MemoryStream();
        await downloadStream.CopyToAsync(memoryStream);
        byte[] actualBytes = memoryStream.ToArray();

        // Assert
        actualBytes.ShouldBe(expectedBytes);
    }

    #endregion

    #region Exists and Delete Tests

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenBlobExists()
    {
        // Arrange
        const string content = "Existence test content";
        const string contentType = MediaTypeNames.Text.Plain;

        await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            content,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Act
        bool exists = await _storageService.ExistsAsync(
            TestContainerName,
            TestBlobName,
            CancellationToken.None);

        // Assert
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenBlobDoesNotExist()
    {
        // Arrange
        const string nonExistentBlobName = "non-existent-blob.txt";

        // Ensure container exists
        await _storageService.UploadAsync(
            TestContainerName,
            "dummy-blob.txt",
            "dummy",
            MediaTypeNames.Text.Plain,
            metadata: null,
            CancellationToken.None);

        // Act
        bool exists = await _storageService.ExistsAsync(
            TestContainerName,
            nonExistentBlobName,
            CancellationToken.None);

        // Assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenContainerDoesNotExist()
    {
        // Arrange
        const string nonExistentContainer = "non-existent-container";

        // Act
        bool exists = await _storageService.ExistsAsync(
            nonExistentContainer,
            TestBlobName,
            CancellationToken.None);

        // Assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveBlob()
    {
        // Arrange
        const string content = "Delete test content";
        const string contentType = MediaTypeNames.Text.Plain;

        await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            content,
            contentType,
            metadata: null,
            CancellationToken.None);

        bool existsBeforeDelete = await _storageService.ExistsAsync(
            TestContainerName,
            TestBlobName,
            CancellationToken.None);
        existsBeforeDelete.ShouldBeTrue();

        // Act
        await _storageService.DeleteAsync(
            TestContainerName,
            TestBlobName,
            CancellationToken.None);

        // Assert
        bool existsAfterDelete = await _storageService.ExistsAsync(
            TestContainerName,
            TestBlobName,
            CancellationToken.None);
        existsAfterDelete.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenBlobDoesNotExist()
    {
        // Arrange
        const string nonExistentBlobName = "non-existent-blob.txt";

        // Ensure container exists
        await _storageService.UploadAsync(
            TestContainerName,
            "dummy-blob.txt",
            "dummy",
            MediaTypeNames.Text.Plain,
            metadata: null,
            CancellationToken.None);

        // Act & Assert
        await Should.NotThrowAsync(async () =>
            await _storageService.DeleteAsync(
                TestContainerName,
                nonExistentBlobName,
                CancellationToken.None));
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetContentAsync_ShouldThrow_WhenBlobDoesNotExist()
    {
        // Arrange
        const string nonExistentBlobName = "non-existent-blob.txt";

        // Ensure container exists
        await _storageService.UploadAsync(
            TestContainerName,
            "dummy-blob.txt",
            "dummy",
            MediaTypeNames.Text.Plain,
            metadata: null,
            CancellationToken.None);

        // Act & Assert
        await Should.ThrowAsync<Azure.RequestFailedException>(async () =>
            await _storageService.GetContentAsync(
                TestContainerName,
                nonExistentBlobName,
                CancellationToken.None));
    }

    [Fact]
    public async Task GetStreamAsync_ShouldThrow_WhenBlobDoesNotExist()
    {
        // Arrange
        const string nonExistentBlobName = "non-existent-blob.txt";

        // Ensure container exists
        await _storageService.UploadAsync(
            TestContainerName,
            "dummy-blob.txt",
            "dummy",
            MediaTypeNames.Text.Plain,
            metadata: null,
            CancellationToken.None);

        // Act & Assert
        await Should.ThrowAsync<Azure.RequestFailedException>(async () =>
            await _storageService.GetStreamAsync(
                TestContainerName,
                nonExistentBlobName,
                CancellationToken.None));
    }

    [Fact]
    public async Task UploadAsync_WithLargeContent_ShouldSucceed()
    {
        // Arrange - Create a 1MB string
        const int oneMegabyte = 1024 * 1024;
        string largeContent = new('X', oneMegabyte);
        const string contentType = MediaTypeNames.Text.Plain;

        // Act
        string blobUri = await _storageService.UploadAsync(
            TestContainerName,
            "large-blob.txt",
            largeContent,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        string retrievedContent = await _storageService.GetContentAsync(
            TestContainerName,
            "large-blob.txt",
            CancellationToken.None);

        retrievedContent.Length.ShouldBe(oneMegabyte);
    }

    #endregion

    #region Multiple Blob Tests

    [Fact]
    public async Task MultipleBlobs_InSameContainer_ShouldBeIndependent()
    {
        // Arrange
        const string blob1Name = "blob1.txt";
        const string blob2Name = "blob2.txt";
        const string content1 = "Content for blob 1";
        const string content2 = "Content for blob 2";
        const string contentType = MediaTypeNames.Text.Plain;

        // Act
        await _storageService.UploadAsync(TestContainerName, blob1Name, content1, contentType, metadata: null, CancellationToken.None);
        await _storageService.UploadAsync(TestContainerName, blob2Name, content2, contentType, metadata: null, CancellationToken.None);

        // Assert
        string retrievedContent1 = await _storageService.GetContentAsync(TestContainerName, blob1Name, CancellationToken.None);
        string retrievedContent2 = await _storageService.GetContentAsync(TestContainerName, blob2Name, CancellationToken.None);

        retrievedContent1.ShouldBe(content1);
        retrievedContent2.ShouldBe(content2);

        // Delete one blob
        await _storageService.DeleteAsync(TestContainerName, blob1Name, CancellationToken.None);

        // Verify only blob1 is deleted
        bool blob1Exists = await _storageService.ExistsAsync(TestContainerName, blob1Name, CancellationToken.None);
        bool blob2Exists = await _storageService.ExistsAsync(TestContainerName, blob2Name, CancellationToken.None);

        blob1Exists.ShouldBeFalse();
        blob2Exists.ShouldBeTrue();
    }

    [Fact]
    public async Task SameBlobName_InDifferentContainers_ShouldBeIndependent()
    {
        // Arrange
        const string container1 = "container-1";
        const string container2 = "container-2";
        const string blobName = "same-name.txt";
        const string content1 = "Content in container 1";
        const string content2 = "Content in container 2";
        const string contentType = MediaTypeNames.Text.Plain;

        // Act
        await _storageService.UploadAsync(container1, blobName, content1, contentType, metadata: null, CancellationToken.None);
        await _storageService.UploadAsync(container2, blobName, content2, contentType, metadata: null, CancellationToken.None);

        // Assert
        string retrievedContent1 = await _storageService.GetContentAsync(container1, blobName, CancellationToken.None);
        string retrievedContent2 = await _storageService.GetContentAsync(container2, blobName, CancellationToken.None);

        retrievedContent1.ShouldBe(content1);
        retrievedContent2.ShouldBe(content2);
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public async Task UploadAsync_WithMetadata_ShouldStoreMetadata()
    {
        // Arrange
        const string content = "Content with metadata";
        const string contentType = MediaTypeNames.Text.Plain;
        var metadata = CreateSimple();

        // Act
        string blobUri = await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            content,
            contentType,
            metadata,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify metadata was stored by retrieving blob properties
        var blobClient = GetBlobClient(TestContainerName, TestBlobName);
        var properties = await blobClient.GetPropertiesAsync();

        properties.Value.Metadata.ShouldContainKeyAndValue("author", "TestUser");
        properties.Value.Metadata.ShouldContainKeyAndValue("version", "1.0");
        properties.Value.Metadata.ShouldContainKeyAndValue("environment", "test");
    }

    [Fact]
    public async Task UploadAsync_WithStreamAndMetadata_ShouldStoreMetadata()
    {
        // Arrange
        const string content = "Stream content with metadata";
        const string contentType = "application/octet-stream";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var metadata = CreateDocument();

        // Act
        string blobUri = await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            stream,
            contentType,
            metadata,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify metadata was stored
        var blobClient = GetBlobClient(TestContainerName, TestBlobName);
        var properties = await blobClient.GetPropertiesAsync();

        properties.Value.Metadata.ShouldContainKeyAndValue("documentType", "invoice");
        properties.Value.Metadata.ShouldContainKeyAndValue("year", "2025");
    }

    [Fact]
    public async Task UploadAsync_WithEmptyMetadata_ShouldSucceed()
    {
        // Arrange
        const string content = "Content with empty metadata";
        const string contentType = MediaTypeNames.Text.Plain;
        var metadata = new Dictionary<string, string>();

        // Act
        string blobUri = await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            content,
            contentType,
            metadata,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify blob was created
        bool exists = await _storageService.ExistsAsync(TestContainerName, TestBlobName, CancellationToken.None);
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task UploadAsync_WithNullMetadata_ShouldSucceed()
    {
        // Arrange
        const string content = "Content with null metadata";
        const string contentType = MediaTypeNames.Text.Plain;

        // Act
        string blobUri = await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            content,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify blob was created with no metadata
        var blobClient = GetBlobClient(TestContainerName, TestBlobName);
        var properties = await blobClient.GetPropertiesAsync();

        properties.Value.Metadata.Count.ShouldBe(0);
    }

    [Fact]
    public async Task UploadAsync_OverwriteWithDifferentMetadata_ShouldReplaceMetadata()
    {
        // Arrange
        const string content = "Content for metadata replacement test";
        const string contentType = MediaTypeNames.Text.Plain;

        var originalMetadata = CreateStatus("draft", "1.0");
        var updatedMetadata = CreateWithReviewer();

        // Upload with original metadata
        await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            content,
            contentType,
            originalMetadata,
            CancellationToken.None);

        // Act - Upload again with updated metadata
        await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            content,
            contentType,
            updatedMetadata,
            CancellationToken.None);

        // Assert - Verify metadata was replaced
        var blobClient = GetBlobClient(TestContainerName, TestBlobName);
        var properties = await blobClient.GetPropertiesAsync();

        properties.Value.Metadata.ShouldContainKeyAndValue("status", "published");
        properties.Value.Metadata.ShouldContainKeyAndValue("version", "2.0");
        properties.Value.Metadata.ShouldContainKeyAndValue("reviewedBy", "Admin");
        properties.Value.Metadata.Count.ShouldBe(3);
    }

    [Fact]
    public async Task UploadAsync_WithSpecialCharactersInMetadataValues_ShouldStoreCorrectly()
    {
        // Arrange
        const string content = "Content with special metadata";
        const string contentType = MediaTypeNames.Text.Plain;
        var metadata = CreateWithSpecialCharacters();

        // Act
        string blobUri = await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            content,
            contentType,
            metadata,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify metadata values were stored correctly
        var blobClient = GetBlobClient(TestContainerName, TestBlobName);
        var properties = await blobClient.GetPropertiesAsync();

        properties.Value.Metadata.ShouldContainKeyAndValue("description", "Test with spaces and special chars: @#$%");
        properties.Value.Metadata.ShouldContainKeyAndValue("path", "/documents/2025/invoices");
        properties.Value.Metadata.ShouldContainKeyAndValue("email", "test@example.com");
    }

    [Fact]
    public async Task UploadAsync_WithMultipleBlobs_ShouldMaintainIndependentMetadata()
    {
        // Arrange
        const string blob1Name = "blob1-metadata.txt";
        const string blob2Name = "blob2-metadata.txt";
        const string content = "Test content";
        const string contentType = MediaTypeNames.Text.Plain;

        var metadata1 = CreateCategory("A");
        var metadata2 = CreateCategory("B");

        // Act
        await _storageService.UploadAsync(TestContainerName, blob1Name, content, contentType, metadata1, CancellationToken.None);
        await _storageService.UploadAsync(TestContainerName, blob2Name, content, contentType, metadata2, CancellationToken.None);

        // Assert
        var blob1Client = GetBlobClient(TestContainerName, blob1Name);
        var properties1 = await blob1Client.GetPropertiesAsync();
        properties1.Value.Metadata.ShouldContainKeyAndValue("category", "A");

        var blob2Client = GetBlobClient(TestContainerName, blob2Name);
        var properties2 = await blob2Client.GetPropertiesAsync();
        properties2.Value.Metadata.ShouldContainKeyAndValue("category", "B");
    }

    #endregion
}
