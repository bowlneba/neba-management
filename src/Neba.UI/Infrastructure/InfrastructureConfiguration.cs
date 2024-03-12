using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

#if DEBUG
using Microsoft.FeatureManagement;
using Neba.UI.Services;
#else
using Microsoft.FeatureManagement;
#endif

namespace Neba.UI.Infrastructure;

internal static class InfrastructureConfiguration
{
    public static void AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json", false, true);

#if DEBUG

        builder.Configuration.AddJsonFile("appsettings.Development.json", false, true);
        builder.Configuration.AddUserSecrets<NebaApiOptions>();

        builder.Services.AddFeatureManagement(builder.Configuration.GetSection("FeatureManagement"));
#else

        builder.Services.AddAzureAppConfiguration();

        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("AppConfig") ??
                                  throw new InvalidOperationException("AppConfig ConnectionString is not set");

            options.Connect(connectionString)
                   .UseFeatureFlags(options =>
                   {
                       options.CacheExpirationInterval = TimeSpan.FromSeconds(5);
                       options.Select(KeyFilter.Any);
                   });
        });

        builder.Services.AddFeatureManagement();

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