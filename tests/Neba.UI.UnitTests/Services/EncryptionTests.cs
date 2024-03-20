using System.Security.Cryptography;
using Azure.Security.KeyVault.Keys;
using Neba.UI.Services;

namespace Neba.UI.UnitTests.Services;

public sealed class EncryptionTests
{
    private readonly Encryption _encryption;

    public EncryptionTests()
    {
        var rsa = RSA.Create(2048);
        var jsonWebKey = new JsonWebKey(rsa);

        _encryption = new Encryption(jsonWebKey);
    }

    [Fact]
    public void Encrypt_WhenDoneMultipleTimesOnTheSameValue_ShouldReturnDifferentValues()
    {
        // Arrange
        var data = "Hello, World!";

        // Act
        var encryptedData1 = _encryption.Encrypt(data);
        var encryptedData2 = _encryption.Encrypt(data);

        // Assert
        encryptedData2.Should().NotBe(encryptedData1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Encrypt_WhenDataIsNullOrEmpty_ShouldReturnAnEmptyString(string? data)
    {
        // Act
        var result = _encryption.Encrypt(data);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    public void Encrypt_WhenDataIsSpaces_ShouldReturnAnEncryptedValue(string data)
    {
        // Act
        var result = _encryption.Encrypt(data);

        // Assert
        result.Should().NotBeEmpty();
    }
}
