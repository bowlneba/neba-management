using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using Microsoft.FeatureManagement;

#if DEBUG
#else
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
#endif

namespace Neba.UI.Infrastructure;

internal static class InfrastructureConfiguration
{
    public static void AddConfiguration(this WebApplicationBuilder builder)
    {

#if DEBUG

        builder.Services.AddScopedFeatureManagement(builder.Configuration.GetSection("FeatureManagement"));

#else

        builder.Services.AddAzureAppConfiguration();

        var connectionString = builder.Configuration.GetConnectionString("AppConfig") ??
                                  throw new InvalidOperationException("AppConfig ConnectionString is not set");

        builder.Configuration.AddAzureAppConfiguration(options
            => options.Connect(new Uri(connectionString), new ManagedIdentityCredential())
                .UseFeatureFlags(flagOptions =>
                {
                    flagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(10);
                    flagOptions.Select(KeyFilter.Any);
                }));

        builder.Services.AddScopedFeatureManagement();

#endif

    }

    public static KeyClient AddKeyVault(this IConfigurationManager config)
    {
        var kvUrl = config.GetValue<string>("KeyVault:Url") ??
                    throw new InvalidOperationException("KeyVault:Url is not set");

        #if DEBUG
            var clientId = config.GetValue<string>("KeyVault:ClientId") ??
                           throw new InvalidOperationException("KeyVault:ClientId is not set");

            var clientSecret = config.GetValue<string>("KeyVault:ClientSecret") ??
                               throw new InvalidOperationException("KeyVault:ClientSecret is not set");

            var tenantId = config.GetValue<string>("KeyVault:TenantId") ??
                           throw new InvalidOperationException("KeyVault:TenantId is not set");

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        #else
            var credential = new ManagedIdentityCredential();
        #endif

        var keyClient = new KeyClient(new Uri(kvUrl), credential);

        config.AddAzureKeyVault(new SecretClient(new Uri(kvUrl), credential), new KeyVaultSecretManager());

        return keyClient;
    }
}