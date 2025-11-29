using Scalar.AspNetCore;

namespace Neba.Api.OpenApi;

#pragma warning disable S2325 // Static classes should not have instance constructors
internal static class OpenApiExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureOpenApi()
        {
            services.AddOpenApi();

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

