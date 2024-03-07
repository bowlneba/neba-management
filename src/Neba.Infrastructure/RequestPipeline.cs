using Microsoft.AspNetCore.Builder;
using Neba.Infrastructure.Middleware;

namespace Neba.Infrastructure;

public static class RequestPipeline
{
    public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        return app;
    }
}
