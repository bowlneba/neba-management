using Scalar.AspNetCore;

namespace Neba.Api.OpenApi;

#pragma warning disable S2325 // Static classes should not have instance constructors
#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class OpenApiExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureOpenApi()
        {
            services.AddOpenApi(options =>
            {
                options.AddSchemaTransformer<MonthSchemaTransformer>();

                options.AddDocumentTransformer((document, _, _) =>
                {
                    document.Info = new()
                    {
                        Title = "NEBA API",
                        Description = "API for the New England Bowling Association (NEBA) management system.",
                        Version = "v1",
                        Contact = new()
                        {
                            Name = "NEBA Tech Support",
                            Email = "tech@bowlneba.com"
                        }
                    };

                    return Task.CompletedTask;
                });
            });

            return services;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication UseOpenApi()
        {
            app.MapOpenApi();
            app.MapScalarApiReference("/api-docs", options =>
            {
                options.Theme = ScalarTheme.BluePlanet;
                options.WithTitle("NEBA API Documentation");
            });

            return app;
        }
    }
}
