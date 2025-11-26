
using System.Diagnostics.CodeAnalysis;

namespace Neba.Infrastructure;


// Suppress CA1812: Avoid uninstantiated internal classes, since this is only instantiated in release mode.
#if DEBUG
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via configuration binding in release mode.")]
#endif
internal sealed record KeyVaultOptions
{
    /// <summary>
    /// Gets or sets the URL of the Azure Key Vault.
    /// </summary>
    public required Uri VaultUrl { get; init; }
}

