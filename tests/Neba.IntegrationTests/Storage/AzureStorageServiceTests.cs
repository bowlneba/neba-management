using System.Net.Mime;
using System.Text;
using Azure.Storage.Blobs;
using Neba.Infrastructure.Storage;
using Neba.Tests.Storage;
using Shouldly;

namespace Neba.IntegrationTests.Storage;

/// <summary>
/// Integration tests for AzureStorageService using Azurite test container.
/// Tests verify blob upload, download, existence checks, and deletion operations.
/// </summary>
[Collection(nameof(Infrastructure.Collections.StorageIntegrationTests))]
public sealed class AzureStorageServiceTests : IAsyncLifetime
{
    private AzureStorageContainer _storageContainer = null!;
    private AzureStorageService _storageService = null!;

    private const string TestContainerName = "test-container";
    private const string TestBlobName = "test-blob.txt";

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
        var blobServiceClient = new BlobServiceClient(_storageContainer.ConnectionString);
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
            CancellationToken.None);

        // Act
        await _storageService.UploadAsync(
            TestContainerName,
            TestBlobName,
            updatedContent,
            contentType,
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
            CancellationToken.None);

        // Assert
        blobUri.ShouldNotBeNullOrEmpty();

        // Verify container was created
        var blobServiceClient = new BlobServiceClient(_storageContainer.ConnectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(newContainerName);
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
        await _storageService.UploadAsync(TestContainerName, blob1Name, content1, contentType, CancellationToken.None);
        await _storageService.UploadAsync(TestContainerName, blob2Name, content2, contentType, CancellationToken.None);

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
        await _storageService.UploadAsync(container1, blobName, content1, contentType, CancellationToken.None);
        await _storageService.UploadAsync(container2, blobName, content2, contentType, CancellationToken.None);

        // Assert
        string retrievedContent1 = await _storageService.GetContentAsync(container1, blobName, CancellationToken.None);
        string retrievedContent2 = await _storageService.GetContentAsync(container2, blobName, CancellationToken.None);

        retrievedContent1.ShouldBe(content1);
        retrievedContent2.ShouldBe(content2);
    }

    #endregion
}
