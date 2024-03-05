using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;

namespace Neba.UI.Infrastructure;

internal static class InfrastructureConfiguration
{
    public static KeyClient AddKeyVault(this IConfigurationManager config)
    {
        var kvUrl = config.GetValue<string>("KeyVault:Url") ?? throw new InvalidOperationException("KeyVault:Url is not set");

#if DEBUG

        var kvClientId = config.GetValue<string>("KeyVault:ClientId");
        var kvClientSecret = config.GetValue<string>("KeyVault:ClientSecret");
        var kvTenantId = config.GetValue<string>("KeyVault:TenantId");

        var credential = new ClientSecretCredential(kvTenantId, kvClientId, kvClientSecret);
        var keyClient = new KeyClient(new Uri(kvUrl), credential);

#else

        var credential = new ManagedIdentityCredential();
        var keyClient = new KeyClient(new Uri(kvUrl), credential);

#endif

        config.AddAzureKeyVault(new SecretClient(new Uri(kvUrl), credential), new KeyVaultSecretManager());

        return keyClient;
    }
}
