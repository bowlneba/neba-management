namespace Neba.Api.HealthChecks;

internal static class HealthCheckExtensions
{
    #pragma warning disable S2325 // Extension methods should be static
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

            app.MapHealthChecks("/health/bowlneba", new()
            {
                Predicate = check => check.Tags.Contains("bowlneba"),
                ResponseWriter = HealthCheckResponseWriter.Default()
            });

            return app;
        }
    }
}
