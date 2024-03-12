using Microsoft.AspNetCore.Builder;
using Neba.Infrastructure.Middleware;

namespace Neba.Infrastructure;

public static class RequestPipeline
{
    public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app)
    {
#if !DEBUG
        app.UseAzureAppConfiguration();
#endif
        app.UseMiddleware<RequestContextLoggingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        return app;
    }
}
