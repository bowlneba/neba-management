using System.Net.Mime;
using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Neba.Application.Documents;
using Neba.Infrastructure.Storage;
using Neba.Tests.Infrastructure;
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

    private const string TestContainer = "test-container";
    private const string TestPath = "test-blob.txt";

    /// <summary>
    /// Gets a BlobServiceClient instance for direct Azure operations.
    /// </summary>
    private BlobServiceClient GetBlobServiceClient()
        => new(_storageContainer.ConnectionString);

    /// <summary>
    /// Gets a BlobClient for direct blob operations and property verification.
    /// </summary>
    private BlobClient GetBlobClient(string container, string path)
    {
        BlobServiceClient blobServiceClient = GetBlobServiceClient();
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(container);
        return containerClient.GetBlobClient(path);
    }

    public async ValueTask InitializeAsync()
    {
        _storageContainer = new AzureStorageContainer();
        await _storageContainer.InitializeAsync();

        var blobServiceClient = new BlobServiceClient(_storageContainer.ConnectionString);
        var settings = new AzureStorageSettings
        {
            UploadChunkSizeBytes = 8 * 1024 * 1024 // 8 MB chunks for testing
        };
        _storageService = new AzureStorageService(blobServiceClient, settings);
    }

    public async ValueTask DisposeAsync()
    {
        // Clean up all test containers before disposing
        BlobServiceClient blobServiceClient = GetBlobServiceClient();
        await foreach (BlobContainerItem container in blobServiceClient.GetBlobContainersAsync())
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
            TestContainer,
            TestPath,
            content,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();
        blobUri.ShouldBe(TestPath);

        // Verify blob was actually created
        bool exists = await _storageService.ExistsAsync(TestContainer, TestPath, CancellationToken.None);
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task UploadAsync_WithStreamContent_ShouldUploadSuccessfully()
    {
        // Arrange
        const string content = "Stream upload test content";
        const string contentType = "application/octet-stream";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        string blobUri = await _storageService.UploadAsync(
            TestContainer,
            TestPath,
            stream,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();
        blobUri.ShouldBe(TestPath);

        // Verify blob was actually created
        bool exists = await _storageService.ExistsAsync(TestContainer, TestPath, CancellationToken.None);
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
            TestContainer,
            TestPath,
            originalContent,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Act
        await _storageService.UploadAsync(
            TestContainer,
            TestPath,
            updatedContent,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        string retrievedContent = await _storageService.GetContentAsync(
            TestContainer,
            TestPath,
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
            TestPath,
            content,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify container was created
        BlobContainerClient containerClient = GetBlobServiceClient().GetBlobContainerClient(newContainerName);
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
            TestContainer,
            TestPath,
            expectedContent,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Act
        string actualContent = await _storageService.GetContentAsync(
            TestContainer,
            TestPath,
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
            TestContainer,
            TestPath,
            expectedContent,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Act
        await using Stream stream = await _storageService.GetStreamAsync(
            TestContainer,
            TestPath,
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

        await using var uploadStream = new MemoryStream(expectedBytes);
        await _storageService.UploadAsync(
            TestContainer,
            TestPath,
            uploadStream,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Act
        await using Stream downloadStream = await _storageService.GetStreamAsync(
            TestContainer,
            TestPath,
            CancellationToken.None);

        await using var memoryStream = new MemoryStream();
        await downloadStream.CopyToAsync(memoryStream);
        byte[] actualBytes = memoryStream.ToArray();

        // Assert
        actualBytes.ShouldBe(expectedBytes);
    }

    [Fact]
    public async Task GetContentWithMetadataAsync_ShouldReturnContentAndMetadata()
    {
        // Arrange
        const string expectedContent = "Content with metadata for retrieval";
        const string contentType = MediaTypeNames.Text.Plain;
        Dictionary<string, string> expectedMetadata = CreateSimple();

        await _storageService.UploadAsync(
            TestContainer,
            TestPath,
            expectedContent,
            contentType,
            expectedMetadata,
            CancellationToken.None);

        // Act
        DocumentDto result = await _storageService.GetContentWithMetadataAsync(
            TestContainer,
            TestPath,
            CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Content.ShouldBe(expectedContent);
        result.Metadata.ShouldNotBeNull();
        result.Metadata.ShouldContainKeyAndValue("author", "TestUser");
        result.Metadata.ShouldContainKeyAndValue("version", "1.0");
        result.Metadata.ShouldContainKeyAndValue("environment", "test");
    }

    [Fact]
    public async Task GetContentWithMetadataAsync_WithNoMetadata_ShouldReturnContentWithEmptyMetadata()
    {
        // Arrange
        const string expectedContent = "Content without metadata";
        const string contentType = MediaTypeNames.Text.Plain;

        await _storageService.UploadAsync(
            TestContainer,
            TestPath,
            expectedContent,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Act
        DocumentDto result = await _storageService.GetContentWithMetadataAsync(
            TestContainer,
            TestPath,
            CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Content.ShouldBe(expectedContent);
        result.Metadata.ShouldNotBeNull();
        result.Metadata.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetContentWithMetadataAsync_ShouldReturnReadOnlyMetadata()
    {
        // Arrange
        const string content = "Test content";
        const string contentType = MediaTypeNames.Text.Plain;
        Dictionary<string, string> metadata = CreateDocument();

        await _storageService.UploadAsync(
            TestContainer,
            TestPath,
            content,
            contentType,
            metadata,
            CancellationToken.None);

        // Act
        DocumentDto result = await _storageService.GetContentWithMetadataAsync(
            TestContainer,
            TestPath,
            CancellationToken.None);

        // Assert
        result.Metadata.ShouldBeAssignableTo<IReadOnlyDictionary<string, string>>();
    }

    [Fact]
    public async Task GetContentWithMetadataAsync_WithComplexMetadata_ShouldPreserveAllMetadataValues()
    {
        // Arrange
        const string content = "Content with complex metadata";
        const string contentType = MediaTypeNames.Text.Plain;
        Dictionary<string, string> metadata = CreateWithSpecialCharacters();

        await _storageService.UploadAsync(
            TestContainer,
            TestPath,
            content,
            contentType,
            metadata,
            CancellationToken.None);

        // Act
        DocumentDto result = await _storageService.GetContentWithMetadataAsync(
            TestContainer,
            TestPath,
            CancellationToken.None);

        // Assert
        result.Metadata.ShouldContainKeyAndValue("description", "Test with spaces and special chars: @#$%");
        result.Metadata.ShouldContainKeyAndValue("path", "/documents/2025/invoices");
        result.Metadata.ShouldContainKeyAndValue("email", "test@example.com");
        result.Metadata.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetContentWithMetadataAsync_ShouldThrow_WhenBlobDoesNotExist()
    {
        // Arrange
        const string nonExistentBlobName = "non-existent-blob.txt";

        // Ensure container exists
        await _storageService.UploadAsync(
            TestContainer,
            "dummy-blob.txt",
            "dummy",
            MediaTypeNames.Text.Plain,
            metadata: null,
            CancellationToken.None);

        // Act & Assert
        await Should.ThrowAsync<Azure.RequestFailedException>(async () =>
            await _storageService.GetContentWithMetadataAsync(
                TestContainer,
                nonExistentBlobName,
                CancellationToken.None));
    }

    [Fact]
    public async Task GetContentWithMetadataAsync_AfterMetadataUpdate_ShouldReturnUpdatedMetadata()
    {
        // Arrange
        const string content = "Content for metadata update test";
        const string contentType = MediaTypeNames.Text.Plain;
        Dictionary<string, string> originalMetadata = CreateStatus("draft", "1.0");

        await _storageService.UploadAsync(
            TestContainer,
            TestPath,
            content,
            contentType,
            originalMetadata,
            CancellationToken.None);

        // Update blob with new metadata
        Dictionary<string, string> updatedMetadata = CreateWithReviewer();
        await _storageService.UploadAsync(
            TestContainer,
            TestPath,
            content,
            contentType,
            updatedMetadata,
            CancellationToken.None);

        // Act
        DocumentDto result = await _storageService.GetContentWithMetadataAsync(
            TestContainer,
            TestPath,
            CancellationToken.None);

        // Assert
        result.Metadata.ShouldContainKeyAndValue("status", "published");
        result.Metadata.ShouldContainKeyAndValue("version", "2.0");
        result.Metadata.ShouldContainKeyAndValue("reviewedBy", "Admin");
        result.Metadata.Count.ShouldBe(3);
        result.Metadata.ShouldNotContainKey("draft");
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
            TestContainer,
            TestPath,
            content,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Act
        bool exists = await _storageService.ExistsAsync(
            TestContainer,
            TestPath,
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
            TestContainer,
            "dummy-blob.txt",
            "dummy",
            MediaTypeNames.Text.Plain,
            metadata: null,
            CancellationToken.None);

        // Act
        bool exists = await _storageService.ExistsAsync(
            TestContainer,
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
            TestPath,
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
            TestContainer,
            TestPath,
            content,
            contentType,
            metadata: null,
            CancellationToken.None);

        bool existsBeforeDelete = await _storageService.ExistsAsync(
            TestContainer,
            TestPath,
            CancellationToken.None);
        existsBeforeDelete.ShouldBeTrue();

        // Act
        await _storageService.DeleteAsync(
            TestContainer,
            TestPath,
            CancellationToken.None);

        // Assert
        bool existsAfterDelete = await _storageService.ExistsAsync(
            TestContainer,
            TestPath,
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
            TestContainer,
            "dummy-blob.txt",
            "dummy",
            MediaTypeNames.Text.Plain,
            metadata: null,
            CancellationToken.None);

        // Act & Assert
        await Should.NotThrowAsync(async () =>
            await _storageService.DeleteAsync(
                TestContainer,
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
            TestContainer,
            "dummy-blob.txt",
            "dummy",
            MediaTypeNames.Text.Plain,
            metadata: null,
            CancellationToken.None);

        // Act & Assert
        await Should.ThrowAsync<Azure.RequestFailedException>(async () =>
            await _storageService.GetContentAsync(
                TestContainer,
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
            TestContainer,
            "dummy-blob.txt",
            "dummy",
            MediaTypeNames.Text.Plain,
            metadata: null,
            CancellationToken.None);

        // Act & Assert
        await Should.ThrowAsync<Azure.RequestFailedException>(async () =>
            await _storageService.GetStreamAsync(
                TestContainer,
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
            TestContainer,
            "large-blob.txt",
            largeContent,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        string retrievedContent = await _storageService.GetContentAsync(
            TestContainer,
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
        await _storageService.UploadAsync(TestContainer, blob1Name, content1, contentType, metadata: null, CancellationToken.None);
        await _storageService.UploadAsync(TestContainer, blob2Name, content2, contentType, metadata: null, CancellationToken.None);

        // Assert
        string retrievedContent1 = await _storageService.GetContentAsync(TestContainer, blob1Name, CancellationToken.None);
        string retrievedContent2 = await _storageService.GetContentAsync(TestContainer, blob2Name, CancellationToken.None);

        retrievedContent1.ShouldBe(content1);
        retrievedContent2.ShouldBe(content2);

        // Delete one blob
        await _storageService.DeleteAsync(TestContainer, blob1Name, CancellationToken.None);

        // Verify only blob1 is deleted
        bool blob1Exists = await _storageService.ExistsAsync(TestContainer, blob1Name, CancellationToken.None);
        bool blob2Exists = await _storageService.ExistsAsync(TestContainer, blob2Name, CancellationToken.None);

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
        Dictionary<string, string> metadata = CreateSimple();

        // Act
        string blobUri = await _storageService.UploadAsync(
            TestContainer,
            TestPath,
            content,
            contentType,
            metadata,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify metadata was stored by retrieving blob properties
        BlobClient blobClient = GetBlobClient(TestContainer, TestPath);
        Response<BlobProperties> properties = await blobClient.GetPropertiesAsync();

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
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        Dictionary<string, string> metadata = CreateDocument();

        // Act
        string blobUri = await _storageService.UploadAsync(
            TestContainer,
            TestPath,
            stream,
            contentType,
            metadata,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify metadata was stored
        BlobClient blobClient = GetBlobClient(TestContainer, TestPath);
        Response<BlobProperties> properties = await blobClient.GetPropertiesAsync();

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
            TestContainer,
            TestPath,
            content,
            contentType,
            metadata,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify blob was created
        bool exists = await _storageService.ExistsAsync(TestContainer, TestPath, CancellationToken.None);
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
            TestContainer,
            TestPath,
            content,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify blob was created with no metadata
        BlobClient blobClient = GetBlobClient(TestContainer, TestPath);
        Response<BlobProperties> properties = await blobClient.GetPropertiesAsync();

        properties.Value.Metadata.Count.ShouldBe(0);
    }

    [Fact]
    public async Task UploadAsync_OverwriteWithDifferentMetadata_ShouldReplaceMetadata()
    {
        // Arrange
        const string content = "Content for metadata replacement test";
        const string contentType = MediaTypeNames.Text.Plain;

        Dictionary<string, string> originalMetadata = CreateStatus("draft", "1.0");
        Dictionary<string, string> updatedMetadata = CreateWithReviewer();

        // Upload with original metadata
        await _storageService.UploadAsync(
            TestContainer,
            TestPath,
            content,
            contentType,
            originalMetadata,
            CancellationToken.None);

        // Act - Upload again with updated metadata
        await _storageService.UploadAsync(
            TestContainer,
            TestPath,
            content,
            contentType,
            updatedMetadata,
            CancellationToken.None);

        // Assert - Verify metadata was replaced
        BlobClient blobClient = GetBlobClient(TestContainer, TestPath);
        Response<BlobProperties> properties = await blobClient.GetPropertiesAsync();

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
        Dictionary<string, string> metadata = CreateWithSpecialCharacters();

        // Act
        string blobUri = await _storageService.UploadAsync(
            TestContainer,
            TestPath,
            content,
            contentType,
            metadata,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify metadata values were stored correctly
        BlobClient blobClient = GetBlobClient(TestContainer, TestPath);
        Response<BlobProperties> properties = await blobClient.GetPropertiesAsync();

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

        Dictionary<string, string> metadata1 = CreateCategory("A");
        Dictionary<string, string> metadata2 = CreateCategory("B");

        // Act
        await _storageService.UploadAsync(TestContainer, blob1Name, content, contentType, metadata1, CancellationToken.None);
        await _storageService.UploadAsync(TestContainer, blob2Name, content, contentType, metadata2, CancellationToken.None);

        // Assert
        BlobClient blob1Client = GetBlobClient(TestContainer, blob1Name);
        Response<BlobProperties> properties1 = await blob1Client.GetPropertiesAsync();
        properties1.Value.Metadata.ShouldContainKeyAndValue("category", "A");

        BlobClient blob2Client = GetBlobClient(TestContainer, blob2Name);
        Response<BlobProperties> properties2 = await blob2Client.GetPropertiesAsync();
        properties2.Value.Metadata.ShouldContainKeyAndValue("category", "B");
    }

    #endregion

    #region UploadLargeAsync Tests

    [Fact]
    public async Task UploadLargeAsync_WithLargeStream_ShouldUploadSuccessfully()
    {
        // Arrange - Create a 10MB stream
        const int tenMegabytes = 10 * 1024 * 1024;
        byte[] largeContent = new byte[tenMegabytes];
#pragma warning disable CA5394 // Random is acceptable for test data generation
        new Random(42).NextBytes(largeContent); // Seed for reproducibility
#pragma warning restore CA5394
        await using var stream = new MemoryStream(largeContent);
        const string contentType = "application/octet-stream";

        // Act
        string blobUri = await _storageService.LargeUploadAsync(
            TestContainer,
            "large-file.bin",
            stream,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();
        blobUri.ShouldContain("large-file.bin");

        // Verify blob was created
        bool exists = await _storageService.ExistsAsync(TestContainer, "large-file.bin", CancellationToken.None);
        exists.ShouldBeTrue();

        // Verify content integrity
        await using Stream downloadedStream = await _storageService.GetStreamAsync(
            TestContainer,
            "large-file.bin",
            CancellationToken.None);

        await using var memoryStream = new MemoryStream();
        await downloadedStream.CopyToAsync(memoryStream);
        byte[] downloadedContent = memoryStream.ToArray();

        downloadedContent.Length.ShouldBe(tenMegabytes);
        downloadedContent.ShouldBe(largeContent);
    }

    [Fact]
    public async Task UploadLargeAsync_WithMultipleChunks_ShouldUploadAllChunks()
    {
        // Arrange - Create a 25MB stream (to ensure multiple 8MB chunks)
        const int twentyFiveMegabytes = 25 * 1024 * 1024;
        byte[] largeContent = new byte[twentyFiveMegabytes];

        // Fill with predictable pattern to verify data integrity
        for (int i = 0; i < largeContent.Length; i++)
        {
            largeContent[i] = (byte)(i % 256);
        }

        await using var stream = new MemoryStream(largeContent);
        const string contentType = "application/octet-stream";

        // Act
        string blobUri = await _storageService.LargeUploadAsync(
            TestContainer,
            "multi-chunk-file.bin",
            stream,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify content integrity across all chunks
        await using Stream downloadedStream = await _storageService.GetStreamAsync(
            TestContainer,
            "multi-chunk-file.bin",
            CancellationToken.None);

        await using var memoryStream = new MemoryStream();
        await downloadedStream.CopyToAsync(memoryStream);
        byte[] downloadedContent = memoryStream.ToArray();

        downloadedContent.Length.ShouldBe(twentyFiveMegabytes);
        downloadedContent.ShouldBe(largeContent);
    }

    [Fact]
    public async Task UploadLargeAsync_WithMetadata_ShouldStoreMetadata()
    {
        // Arrange
        const int fiveMegabytes = 5 * 1024 * 1024;
        byte[] content = new byte[fiveMegabytes];
#pragma warning disable CA5394 // Random is acceptable for test data generation
        new Random(123).NextBytes(content);
#pragma warning restore CA5394
        await using var stream = new MemoryStream(content);
        const string contentType = "video/mp4";
        Dictionary<string, string> metadata = CreateDocument();

        // Act
        string blobUri = await _storageService.LargeUploadAsync(
            TestContainer,
            "video-with-metadata.mp4",
            stream,
            contentType,
            metadata,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify metadata was stored
        BlobClient blobClient = GetBlobClient(TestContainer, "video-with-metadata.mp4");
        Response<BlobProperties> properties = await blobClient.GetPropertiesAsync();

        properties.Value.Metadata.ShouldContainKeyAndValue("documentType", "invoice");
        properties.Value.Metadata.ShouldContainKeyAndValue("year", "2025");
        properties.Value.ContentType.ShouldBe(contentType);
    }

    [Fact]
    public async Task UploadLargeAsync_ShouldCreateContainerIfNotExists()
    {
        // Arrange
        const string newContainerName = "large-upload-container";
        const int fiveMegabytes = 5 * 1024 * 1024;
        byte[] content = new byte[fiveMegabytes];
        await using var stream = new MemoryStream(content);
        const string contentType = "application/octet-stream";

        // Act
        string blobUri = await _storageService.LargeUploadAsync(
            newContainerName,
            "large-file.bin",
            stream,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify container was created
        BlobContainerClient containerClient = GetBlobServiceClient().GetBlobContainerClient(newContainerName);
        bool containerExists = await containerClient.ExistsAsync();
        containerExists.ShouldBeTrue();
    }

    [Fact]
    public async Task UploadLargeAsync_ShouldOverwriteExistingBlob()
    {
        // Arrange
        const int fiveMegabytes = 5 * 1024 * 1024;
        byte[] originalContent = new byte[fiveMegabytes];
        byte[] updatedContent = new byte[fiveMegabytes];

#pragma warning disable CA5394 // Random is acceptable for test data generation
        new Random(1).NextBytes(originalContent);
        new Random(2).NextBytes(updatedContent);
#pragma warning restore CA5394

        const string contentType = "application/octet-stream";
        const string blobName = "overwrite-test.bin";

        // Upload original content
        await using (var stream = new MemoryStream(originalContent))
        {
            await _storageService.LargeUploadAsync(
                TestContainer,
                blobName,
                stream,
                contentType,
                metadata: null,
                CancellationToken.None);
        }

        // Act - Upload updated content
        await using (var stream = new MemoryStream(updatedContent))
        {
            await _storageService.LargeUploadAsync(
                TestContainer,
                blobName,
                stream,
                contentType,
                metadata: null,
                CancellationToken.None);
        }

        // Assert
        await using Stream downloadedStream = await _storageService.GetStreamAsync(
            TestContainer,
            blobName,
            CancellationToken.None);

        await using var memoryStream = new MemoryStream();
        await downloadedStream.CopyToAsync(memoryStream);
        byte[] downloadedContent = memoryStream.ToArray();

        downloadedContent.ShouldBe(updatedContent);
        downloadedContent.ShouldNotBe(originalContent);
    }

    [Fact]
    public async Task UploadLargeAsync_WithSmallFile_ShouldStillWork()
    {
        // Arrange - Upload a small file (less than one chunk) using UploadLargeAsync
        const string content = "Small content using large upload method";
        byte[] contentBytes = Encoding.UTF8.GetBytes(content);
        await using var stream = new MemoryStream(contentBytes);
        const string contentType = MediaTypeNames.Text.Plain;

        // Act
        string blobUri = await _storageService.LargeUploadAsync(
            TestContainer,
            "small-file-large-upload.txt",
            stream,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify content
        string retrievedContent = await _storageService.GetContentAsync(
            TestContainer,
            "small-file-large-upload.txt",
            CancellationToken.None);

        retrievedContent.ShouldBe(content);
    }

    [Fact]
    public async Task UploadLargeAsync_WithEmptyStream_ShouldCreateEmptyBlob()
    {
        // Arrange
        await using var stream = new MemoryStream();
        const string contentType = "application/octet-stream";

        // Act
        string blobUri = await _storageService.LargeUploadAsync(
            TestContainer,
            "empty-file.bin",
            stream,
            contentType,
            metadata: null,
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify blob exists and is empty
        bool exists = await _storageService.ExistsAsync(TestContainer, "empty-file.bin", CancellationToken.None);
        exists.ShouldBeTrue();

        await using Stream downloadedStream = await _storageService.GetStreamAsync(
            TestContainer,
            "empty-file.bin",
            CancellationToken.None);

        await using var memoryStream = new MemoryStream();
        await downloadedStream.CopyToAsync(memoryStream);
        memoryStream.ToArray().Length.ShouldBe(0);
    }

    [Fact]
    public async Task UploadLargeAsync_WithDifferentContentTypes_ShouldStoreCorrectContentType()
    {
        // Arrange
        const int fiveMegabytes = 5 * 1024 * 1024;
        byte[] content = new byte[fiveMegabytes];

        (string, string)[] testCases = new[]
        {
            ("video.mp4", "video/mp4"),
            ("document.pdf", "application/pdf"),
            ("image.png", "image/png"),
            ("data.json", "application/json")
        };

        foreach ((string? blobName, string? contentType) in testCases)
        {
            await using var stream = new MemoryStream(content);

            // Act
            await _storageService.LargeUploadAsync(
                TestContainer,
                blobName,
                stream,
                contentType,
                metadata: null,
                CancellationToken.None);

            // Assert
            BlobClient blobClient = GetBlobClient(TestContainer, blobName);
            Response<BlobProperties> properties = await blobClient.GetPropertiesAsync();
            properties.Value.ContentType.ShouldBe(contentType);
        }
    }

    #endregion

    #region GetBlobUri Tests

    [Fact]
    public void GetBlobUri_ShouldReturnValidUri()
    {
        // Arrange
        const string container = "documents";
        const string path = "bylaws.html";

        // Act
        Uri uri = _storageService.GetBlobUri(container, path);

        // Assert
        uri.ShouldNotBeNull();
        uri.ToString().ShouldContain(container);
        uri.ToString().ShouldContain(path);
        uri.IsAbsoluteUri.ShouldBeTrue();
    }

    [Fact]
    public void GetBlobUri_ShouldIncludePathWithSubdirectories()
    {
        // Arrange
        const string container = "documents";
        const string path = "awards/2024/hall-of-fame.pdf";

        // Act
        Uri uri = _storageService.GetBlobUri(container, path);

        // Assert
        uri.ShouldNotBeNull();
        uri.ToString().ShouldContain("awards/2024/hall-of-fame.pdf");
    }

    [Fact]
    public void GetBlobUri_ShouldPointToCorrectBlobServiceHost()
    {
        // Arrange
        const string container = "test-container";
        const string path = "test-file.txt";

        // Act
        Uri uri = _storageService.GetBlobUri(container, path);

        // Assert
        uri.ShouldNotBeNull();
        uri.Host.ShouldContain("127.0.0.1"); // Azurite runs on localhost
    }

    [Fact]
    public void GetBlobUri_ForDifferentContainers_ShouldReturnDifferentUris()
    {
        // Arrange
        const string path = "same-file.txt";

        // Act
        Uri uri1 = _storageService.GetBlobUri("container-1", path);
        Uri uri2 = _storageService.GetBlobUri("container-2", path);

        // Assert
        uri1.ShouldNotBe(uri2);
        uri1.ToString().ShouldContain("container-1");
        uri2.ToString().ShouldContain("container-2");
    }

    [Fact]
    public void GetBlobUri_ForDifferentPaths_ShouldReturnDifferentUris()
    {
        // Arrange
        const string container = "documents";

        // Act
        Uri uri1 = _storageService.GetBlobUri(container, "file1.txt");
        Uri uri2 = _storageService.GetBlobUri(container, "file2.txt");

        // Assert
        uri1.ShouldNotBe(uri2);
        uri1.ToString().ShouldContain("file1.txt");
        uri2.ToString().ShouldContain("file2.txt");
    }

    [Fact]
    public void GetBlobUri_WithSpecialCharactersInPath_ShouldEncodeCorrectly()
    {
        // Arrange
        const string container = "documents";
        const string path = "files with spaces/document (1).pdf";

        // Act
        Uri uri = _storageService.GetBlobUri(container, path);

        // Assert
        uri.ShouldNotBeNull();
        uri.IsAbsoluteUri.ShouldBeTrue();
        // URI encoding should handle spaces and parentheses
        uri.ToString().ShouldContain(container);
    }

    [Fact]
    public async Task GetBlobUri_ForExistingBlob_ShouldPointToAccessibleResource()
    {
        // Arrange
        const string container = "test-uri-container";
        const string path = "test-file.txt";
        const string content = "Test content for URI verification";

        await _storageService.UploadAsync(
            container,
            path,
            content,
            MediaTypeNames.Text.Plain,
            metadata: null,
            CancellationToken.None);

        // Act
        Uri uri = _storageService.GetBlobUri(container, path);

        // Assert - Verify the URI points to the correct blob by comparing with an authenticated client
        BlobClient authenticatedBlobClient = GetBlobClient(container, path);
        uri.ShouldBe(authenticatedBlobClient.Uri);

        // Verify the blob exists using the authenticated client
        bool exists = await authenticatedBlobClient.ExistsAsync();
        exists.ShouldBeTrue();
    }

    [Fact]
    public void GetBlobUri_ForNonExistentBlob_ShouldStillReturnValidUri()
    {
        // Arrange
        const string container = "non-existent-container";
        const string path = "non-existent-file.txt";

        // Act
        Uri uri = _storageService.GetBlobUri(container, path);

        // Assert
        uri.ShouldNotBeNull();
        uri.IsAbsoluteUri.ShouldBeTrue();
        uri.ToString().ShouldContain(container);
        uri.ToString().ShouldContain(path);
        // Note: URI generation doesn't validate existence
    }

    #endregion
}
