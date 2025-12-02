namespace Neba.Api.HealthChecks;

internal static class HealthCheckResponseWriter
{
    public static Func<HttpContext, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport, Task> Default()
            => async (context, report) =>
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
        };
}
