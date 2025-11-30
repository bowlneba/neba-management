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
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        Status = report.Status.ToString(),
                        Checks = report.Entries.Select(entry => new
                        {
                            Name = entry.Key,
                            Status = entry.Value.Status.ToString(),
                            entry.Value.Description,
                            entry.Value.Data,
                            Exception = entry.Value.Exception?.Message,
                            Duration = entry.Value.Duration.ToString()
                        }),
                        TotalDuration = report.TotalDuration.ToString()
                    };

                    await context.Response.WriteAsJsonAsync(response);
                }
            });

            return app;
        }
    }
}
