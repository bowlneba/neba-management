using System.Security.Cryptography;
using System.Text;
using Azure.Security.KeyVault.Keys;

namespace Neba.UI.Services;

internal sealed class Encryption : IEncryption
{
    private readonly string _key;

    public Encryption(JsonWebKey webKey)
    {
        using var rsa = RSA.Create(2048);
        RSAParameters rsaParameters = new()
        {
            Modulus = webKey.N,
            Exponent = webKey.E
        };

        rsa.ImportParameters(rsaParameters);

        _key = rsa.ToXmlString(false);
    }

    public string Encrypt(string? data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return string.Empty;
        }

        using var rsa = new RSACryptoServiceProvider(2048);
        rsa.FromXmlString(_key);

        var dataBytes = Encoding.UTF8.GetBytes(data);
        var encryptedData = rsa.Encrypt(dataBytes, true);

        return Convert.ToBase64String(encryptedData);
    }
}

internal interface IEncryption
{
    string Encrypt(string? data);
}