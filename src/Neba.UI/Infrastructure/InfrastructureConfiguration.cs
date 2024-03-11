using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using Microsoft.FeatureManagement;

#if DEBUG
#else
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Neba.UI.Infrastructure;
#endif

namespace Neba.UI.Infrastructure;

internal static class InfrastructureConfiguration
{
    public static void AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json", false, true);

#if DEBUG

        builder.Configuration.AddJsonFile("appsettings.Development.json", false, true);

        builder.Services.AddFeatureManagement(builder.Configuration.GetSection("FeatureManagement"));

#else

#endif

    }

    public static KeyClient AddKeyVault(this IConfigurationManager config)
    {
        var kvUrl = config.GetValue<string>("KeyVault:Url") ??
                    throw new InvalidOperationException("KeyVault:Url is not set");

        var credential = new ManagedIdentityCredential();
        var keyClient = new KeyClient(new Uri(kvUrl), credential);

        config.AddAzureKeyVault(new SecretClient(new Uri(kvUrl), credential), new KeyVaultSecretManager());

        return keyClient;
    }
}