using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Middleware;

internal sealed class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.ExceptionOccurred(ex, ex.Message);

            var problemDetail = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "ServerError",
                Title = "Server Error",
                Detail = "An unexpected error occurred"
            };

            context.Response.StatusCode = problemDetail.Status.Value;

            await context.Response.WriteAsJsonAsync(problemDetail);
        }
    }
}

internal static partial class ExceptionHandlingMiddlewareLogMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "An exception occurred: {ExceptionMessage}")]
    public static partial void ExceptionOccurred(this ILogger<ExceptionHandlingMiddleware> logger, Exception ex,
        string exceptionMessage);
}
