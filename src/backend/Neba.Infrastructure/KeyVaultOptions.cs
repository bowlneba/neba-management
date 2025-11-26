namespace Neba.Infrastructure;

internal sealed record KeyVaultOptions
{
    /// <summary>
    /// Gets or sets the URL of the Azure Key Vault.
    /// </summary>
    public required Uri VaultUrl { get; init; }
}

