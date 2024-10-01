using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Middleware;

/// <summary>
/// Handles global exceptions and returns a standardized error response.
/// </summary>
public sealed class GlobalExceptionHandler
    : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Tries to handle the exception and returns a standardized error response.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the exception was handled.</returns>
    public async ValueTask<bool> TryHandleAsync([NotNull] HttpContext httpContext, [NotNull] Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogExceptionOccurred(exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server error"
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

/// <summary>
/// Contains log messages for the <see cref="GlobalExceptionHandler"/> class.
/// </summary>
internal static partial class GlobalExceptionHandlerLogMessages
{
    /// <summary>
    /// Logs an error message when an exception occurs.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The error message.</param>
    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred: {Message}")]
    public static partial void LogExceptionOccurred(this ILogger<GlobalExceptionHandler> logger, string message);
}