using System.Security.Cryptography;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using EncryptionUI = Neba.UI.Services.Encryption;
using EncryptionApi = Neba.Infrastructure.Security.Encryption;

namespace Neba.FunctionalTests.Security;

public class EncryptionTests
{
    private readonly EncryptionUI _uiEncryption;
    private readonly EncryptionApi _apiEncryption;

    public EncryptionTests()
    {
        var rsa = RSA.Create(2048);
        var uiKey = new JsonWebKey(rsa, false, new[] { KeyOperation.Encrypt, });
        var apiKey = new JsonWebKey(rsa, true, new[] { KeyOperation.Encrypt, KeyOperation.Decrypt });

        var cryptographyClient = new CryptographyClient(apiKey);

        _uiEncryption = new EncryptionUI(uiKey);
        _apiEncryption = new EncryptionApi(cryptographyClient);
    }

    [Fact]
    public async Task WhenAValueIsEncryptedOnTheUI_ApiShouldBeAbleToDecrypt()
    {
        // Arrange
        var value = "Hello, World!";
        var encryptedValue = _uiEncryption.Encrypt(value);

        // Act
        var decryptedValue = await _apiEncryption.DecryptAsync(encryptedValue, default);

        // Assert
        decryptedValue.Should().Be(value);
    }
}
