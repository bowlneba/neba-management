namespace Neba.Api.HealthChecks;

internal static class HealthCheckExtensions
{
    #pragma warning disable S2325 // Extension methods should be static
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds health check services to the dependency injection container.
        /// </summary>
        /// <returns>The service collection for method chaining.</returns>
        public IServiceCollection ConfigureHealthChecks()
        {
            services.AddHealthChecks();

            return services;
        }
    }

    extension(WebApplication app)
    {
        /// <summary>
        /// Configures the health check endpoints for the application.
        /// </summary>
        /// <returns>The web application for method chaining.</returns>
        public WebApplication UseHealthChecks()
        {
            app.MapHealthChecks("/health", new()
            {
                Predicate = _ => true,
                ResponseWriter = HealthCheckResponseWriter.Default()
            });

            app.MapHealthChecks("/health/bowlneba", new()
            {
                Predicate = check => check.Tags.Contains("bowlneba"),
                ResponseWriter = HealthCheckResponseWriter.Default()
            });

            return app;
        }
    }
}
