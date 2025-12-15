using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
    }
}
