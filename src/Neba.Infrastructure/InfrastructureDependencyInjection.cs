﻿using System.Diagnostics;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Neba.Application.Caching;
using Neba.Application.Clock;
using Neba.Application.Security;
using Neba.Infrastructure.Caching;
using Neba.Infrastructure.Clock;
using Neba.Infrastructure.Diagnostics;
using Neba.Infrastructure.Security;
using Uri = System.Uri;

#if DEBUG
#else
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
#endif

namespace Neba.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddSharedInfrastructureServices(this IServiceCollection services,
        IConfigurationManager configuration)
    {
        Debug.Assert(configuration != null, nameof(configuration) + " != null");

        services.AddDiagnostics();

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        #region Feature Flags

#if DEBUG

        services.AddScopedFeatureManagement(configuration.GetSection("FeatureManagement"));

#else
        AddFeatureManagement(services, configuration);

#endif

        #endregion

        services.AddCaching();

        var keyVaultInfo = ConfigureKeyVault(configuration);

        services.AddHealthChecks(keyVaultInfo);
        services.AddEncryption(configuration, keyVaultInfo.credential);

        return services;
    }

#if !DEBUG
    private static void AddFeatureManagement(IServiceCollection services, IConfigurationManager configuration)
    {
        services.AddAzureAppConfiguration();

        var connectionString = configuration.GetValue<string>("APPCONFIG_ENDPOINT") ??
                               throw new InvalidOperationException("APPCONFIG_ENDPOINT is not set");

        configuration.AddAzureAppConfiguration(options =>
        {
            var credential = new ManagedIdentityCredential();

            options.Connect(new Uri(connectionString), credential)
                .UseFeatureFlags(flagOptions =>
                {
                    flagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(10);
                    flagOptions.Select(KeyFilter.Any);
                });

            options.ConfigureKeyVault(keyVault => keyVault.SetCredential(credential));
        });

        services.AddScopedFeatureManagement();
    }

#endif

    private static void AddDiagnostics(this IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();

        services.AddScoped<IObserver<KeyValuePair<string, object?>>, KeyValueObserver>();
        services.AddScoped<IObserver<DiagnosticListener>, DiagnosticObserver>();

        DiagnosticListener.AllListeners.Subscribe(services.BuildServiceProvider()
            .GetRequiredService<IObserver<DiagnosticListener>>());
    }

    private static void AddCaching(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();

        services.AddSingleton<ICacheService, CacheService>();
    }

    private static (string url, TokenCredential credential) ConfigureKeyVault(IConfiguration config)
    {
        var kvUrl = config.GetValue<string>("KeyVault:Url") ??
                    throw new InvalidOperationException("KeyVault:Url is not set");

#if DEBUG

        var kvClientId = config.GetValue<string>("KeyVault:ClientId");
        var kvClientSecret = config.GetValue<string>("KeyVault:ClientSecret");
        var kvTenantId = config.GetValue<string>("KeyVault:TenantId");

        var credential = new ClientSecretCredential(kvTenantId, kvClientId, kvClientSecret);

#else

        var credential = new ManagedIdentityCredential();

#endif

        return (kvUrl, credential);
    }

    private static void AddHealthChecks(this IServiceCollection services, (string kvUrl, TokenCredential credential) keyVaultInfo)
    {
        services.AddHealthChecks()
            .AddAzureKeyVault(new Uri(keyVaultInfo.kvUrl), keyVaultInfo.credential, options => options.AddSecret("Health"));
    }

    private static void AddEncryption(this IServiceCollection services, IConfiguration config, TokenCredential credential)
    {
        var keyUrl = new Uri(config.GetValue<string>("Encryption:Url") ??
                             throw new InvalidOperationException("Cannot get Encryption:Url"));

        var cryptographyClient = new CryptographyClient(keyUrl, credential);

        services.AddSingleton<IEncryption>(new Encryption(cryptographyClient));
    }
}