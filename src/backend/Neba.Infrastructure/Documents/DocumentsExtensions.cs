using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neba.Application.BackgroundJobs;
using Neba.Application.Documents;

namespace Neba.Infrastructure.Documents;

#pragma warning disable S1144 // Remove unused constructor of private type.
#pragma warning disable S2325 // Extension methods should be static

internal static class DocumentsExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddGoogleDocs(IConfiguration config)
        {
            services.AddOptions<GoogleDocsSettings>()
                .Bind(config.GetSection("GoogleDocs"))
                .Validate(settings => settings.Credentials is not null,
                    "GoogleDocs:Credentials section must be provided.")
                .Validate(settings => !string.IsNullOrWhiteSpace(settings.Credentials.PrivateKey),
                    "GoogleDocs:Credentials:PrivateKey must be provided.")
                .Validate(settings => !string.IsNullOrWhiteSpace(settings.Credentials.ClientEmail),
                    "GoogleDocs:Credentials:ClientEmail must be provided.")
                .Validate(settings => !string.IsNullOrWhiteSpace(settings.Credentials.PrivateKeyId),
                    "GoogleDocs:Credentials:PrivateKeyId must be provided.")
                .Validate(settings => !string.IsNullOrWhiteSpace(settings.Credentials.ClientX509CertUrl),
                    "GoogleDocs:Credentials:ClientX509CertUrl must be provided.")
                .ValidateOnStart();

            services.AddSingleton(sp =>
            {
                GoogleDocsSettings settings = sp.GetRequiredService<IOptions<GoogleDocsSettings>>().Value;

                settings.Credentials.PrivateKey = settings.Credentials.PrivateKey?
                    .Replace("\\n", "\n", StringComparison.OrdinalIgnoreCase);

                return settings;
            });

            services.AddSingleton<DocumentMapper>();
            services.AddSingleton<IDocumentsService, GoogleDocsService>();

            return services;
        }

        public IServiceCollection AddDocumentBackgroundJobs()
        {
            services.AddScoped<IBackgroundJobHandler<SyncHtmlDocumentToStorageJob>, SyncHtmlDocumentToStorageJobHandler>();

            return services;
        }

        public IServiceCollection AddDocumentRefreshNotification()
        {
            // Simple dictionary of channels - one per document type
#pragma warning disable S1135 // Track issues in a proper issue tracker
            // TODO: If we need automatic cleanup or multi-tenant channels in the future, consider:
            //   - Reference counting for active listeners
            //   - Automatic channel disposal when idle
            //   - Per-tenant channel isolation
            //   For now, simple global channels are sufficient
#pragma warning restore S1135
            services.AddSingleton<DocumentRefreshChannels>();
            services.AddSingleton<IDocumentRefreshNotifier, SseDocumentRefreshNotifier>();

            return services;
        }
    }
}
