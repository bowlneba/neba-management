using System.Security.Cryptography;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Neba.Infrastructure.Security;

namespace Neba.Infrastructure.UnitTests.Security;

public sealed class EncryptionTests
{
    private readonly Encryption _encryption;

    public EncryptionTests()
    {
        var rsa = RSA.Create(2048);
        var key = new JsonWebKey(rsa, true, new[] { KeyOperation.Encrypt, KeyOperation.Decrypt });

        var cryptographyClient = new CryptographyClient(key);
        _encryption = new Encryption(cryptographyClient);
    }

    [Fact]
    public async Task WhenPlainTextValueIsEncryptedAndThenDecrypted_ShouldReturnTheOriginalValue()
    {
        // Arrange
        var data = "Hello, World!";

        // Act
        var encryptedData = await _encryption.EncryptAsync(data, CancellationToken.None);
        var decryptedData = await _encryption.DecryptAsync(encryptedData, CancellationToken.None);

        // Assert
        decryptedData.Should().Be(data);
    }

    [Fact]
    public async Task EncryptAsync_WhenDoneMultipleTimesOnTheSameValue_ShouldReturnDifferentValues()
    {
        // Arrange
        var data = "Hello, World!";

        // Act
        var encryptedData1 = await _encryption.EncryptAsync(data, CancellationToken.None);
        var encryptedData2 = await _encryption.EncryptAsync(data, CancellationToken.None);

        // Assert
        encryptedData1.Should().NotBe(encryptedData2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task EncryptAsync_WhenDataIsNullOrEmpty_ShouldReturnAnEmptyString(string? data)
    {
        // Act
        var encryptedData = await _encryption.EncryptAsync(data, CancellationToken.None);

        // Assert
        encryptedData.Should().BeEmpty();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    public async Task EncryptAsync_WhenDataIsSpaces_ShouldReturnAnEncryptedValue(string? data)
    {
        // Act
        var encryptedData = await _encryption.EncryptAsync(data, CancellationToken.None);

        // Assert
        encryptedData.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task DecryptAsync_WhenDataIsNullOrEmpty_ShouldReturnAnEmptyString(string? data)
    {
        // Act
        var decryptedData = await _encryption.DecryptAsync(data, CancellationToken.None);

        // Assert
        decryptedData.Should().BeEmpty();
    }

    [Fact]
    public async Task DecryptAsync_WhenProvidedDifferentValuesEncryptedFromTheSameOriginalText_ShouldReturnOriginalText()
    {
        // Arrange
        var data = "Hello, World!";

        // Act
        var encryptedData1 = await _encryption.EncryptAsync(data, CancellationToken.None);
        var encryptedData2 = await _encryption.EncryptAsync(data, CancellationToken.None);
        var decryptedData1 = await _encryption.DecryptAsync(encryptedData1, CancellationToken.None);
        var decryptedData2 = await _encryption.DecryptAsync(encryptedData2, CancellationToken.None);

        // Assert
        decryptedData1.Should().Be(decryptedData2);
        decryptedData2.Should().Be(data);
    }
}
