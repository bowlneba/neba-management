using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Neba.Infrastructure.Middleware;

internal sealed class RequestContextLoggingMiddleware
{
    private const string _correlationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public RequestContextLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", GetCorrelationId(context)))
        {
            using (LogContext.PushProperty("ClientIp", context.Connection.RemoteIpAddress))
            {
                await _next(context);
            }
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(_correlationIdHeaderName, out var correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }
}
