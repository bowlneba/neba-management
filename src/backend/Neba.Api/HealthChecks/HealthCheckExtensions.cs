namespace Neba.Api.HealthChecks;

internal static class HealthCheckExtensions
{
#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable S1144 // Remove unused constructor of private type.
    extension(WebApplication app)
    {
        /// <summary>
        /// Configures the health check endpoints for the application.
        /// </summary>
        /// <returns>The web application for method chaining.</returns>
        public WebApplication UseHealthChecks()
        {
            app.MapGet("/ping", () => Results.Ok("Pong"));

            app.MapHealthChecks("/health", new()
            {
                Predicate = _ => true,
                ResponseWriter = HealthCheckResponseWriter.Default()
            });

            app.MapHealthChecks("/health/website", new()
            {
                Predicate = check => check.Tags.Contains("website"),
                ResponseWriter = HealthCheckResponseWriter.Default()
            });

            app.MapHealthChecks("/health/jobs", new()
            {
                Predicate = check => check.Tags.Contains("background-jobs"),
                ResponseWriter = HealthCheckResponseWriter.Default()
            });

            return app;
        }
    }
}
