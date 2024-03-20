using System.Text;
using Azure.Security.KeyVault.Keys.Cryptography;
using Neba.Application.Security;

namespace Neba.Infrastructure.Security;

internal sealed class Encryption : IEncryption
{
    private readonly CryptographyClient _cryptographyClient;

    public Encryption(CryptographyClient cryptographyClient)
    {
        _cryptographyClient = cryptographyClient;
    }

    public async Task<string> EncryptAsync(string? data, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(data))
        {
            return string.Empty;
        }

        var dataBytes = Encoding.UTF8.GetBytes(data);
        var encryptedData = await _cryptographyClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, dataBytes, cancellationToken);

        return Convert.ToBase64String(encryptedData.Ciphertext);
    }

    public async Task<string> DecryptAsync(string? data, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(data))
        {
            return string.Empty;
        }

        var dataBytes = Convert.FromBase64String(data);
        var decryptedData = await _cryptographyClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, dataBytes, cancellationToken);

        return Encoding.UTF8.GetString(decryptedData.Plaintext);
    }
}
