namespace Neba.Application.Security;

public interface IEncryption
{
    Task<string> EncryptAsync(string? data, CancellationToken cancellationToken);

    Task<string> DecryptAsync(string? data, CancellationToken cancellationToken);
}
