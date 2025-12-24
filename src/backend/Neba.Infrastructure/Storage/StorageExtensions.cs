using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            services.AddOptions<AzureStorageSettings>()
                .Bind(config.GetSection("AzureStorage"))
                .Validate(settings => settings.UploadChunkSizeBytes > 0,
                    "AzureStorage:UploadChunkSizeBytes must be a positive integer.")
                .ValidateOnStart();

            services.AddSingleton(sp
                => sp.GetRequiredService<IOptions<AzureStorageSettings>>().Value);

            // Prefer managed identity (Azure) over connection string (local Azurite)
            string? blobServiceUri = config["AzureStorage:BlobServiceUri"];

            BlobServiceClient blobServiceClient;

            if (!string.IsNullOrEmpty(blobServiceUri))
            {
                // Azure: Use managed identity with DefaultAzureCredential
                var serviceUri = new Uri(blobServiceUri);
                var credential = new DefaultAzureCredential();
                blobServiceClient = new BlobServiceClient(serviceUri, credential);
            }
            else
            {
                // Local: Use connection string for Azurite
                string connectionString = config.GetConnectionString("azure-storage")
                    ?? throw new InvalidOperationException(
                        "Either AzureStorage:BlobServiceUri (for managed identity) or " +
                        "ConnectionStrings:azure-storage (for local development) must be configured.");

                blobServiceClient = new BlobServiceClient(connectionString);
            }

            services.AddSingleton(blobServiceClient);

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
