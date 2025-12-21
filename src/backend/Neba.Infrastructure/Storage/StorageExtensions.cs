using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neba.Application.Storage;

namespace Neba.Infrastructure.Storage;

#pragma warning disable S1144 // Unused private types or members should be removed
#pragma warning disable S2325 // Private methods that don't access instance data should be "static"

internal static class StorageExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddStorageService(IConfiguration config)
        {
            string connectionString = config.GetConnectionString("azure-storage")
                    ?? throw new InvalidOperationException("Azure Storage connection string is not configured.");

            services.AddSingleton(_ => new BlobServiceClient(connectionString));

            services.AddSingleton<IStorageService, AzureStorageService>();

            services.AddHealthChecks()
                .AddAzureBlobStorage(
                    sp => sp.GetRequiredService<BlobServiceClient>(),
                    name: "Azure Blob Storage",
                    tags: ["infrastructure", "storage"]
                );

            return services;
        }
    }
}
