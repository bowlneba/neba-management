using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace Neba.Infrastructure.Http;

#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable S1144 // Remove unused constructor of private type.
#pragma warning disable CA1034 // Do not nest type

/// <summary>
/// Extension methods for converting <see cref="ErrorOr{T}"/> results to ASP.NET Core HTTP responses.
/// </summary>
public static class ResultExtensions
{
    extension<T>(ErrorOr<T> result)
    {
        /// <summary>
        /// Converts an <see cref="ErrorOr{T}"/> error result to an ASP.NET Core problem details response.
        /// </summary>
        /// <returns>
        /// An <see cref="IResult"/> containing RFC 7807 problem details with appropriate HTTP status code.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to create a problem result from a successful result.
        /// </exception>
        /// <remarks>
        /// This method maps <see cref="ErrorType"/> to appropriate HTTP status codes:
        /// <list type="bullet">
        /// <item><description>Validation errors → 400 Bad Request</description></item>
        /// <item><description>Unauthorized errors → 401 Unauthorized</description></item>
        /// <item><description>Forbidden errors → 403 Forbidden</description></item>
        /// <item><description>NotFound errors → 404 Not Found</description></item>
        /// <item><description>Conflict errors → 409 Conflict</description></item>
        /// <item><description>Failure/Unexpected errors → 500 Internal Server Error</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// ErrorOr&lt;User&gt; result = await userService.GetUserAsync(id);
        /// return result.IsError ? result.Problem() : Results.Ok(result.Value);
        /// </code>
        /// </example>
        public IResult Problem()
        {
            if (!result.IsError)
            {
                throw new InvalidOperationException("Cannot create a problem result from a successful result.");
            }

            Error error = result.Errors[0];

            return Results.Problem(
                detail: error.Description,
                statusCode: GetStatusCode(error),
                title: GetTitle(error),
                type: GetErrorRfc(error.Type),
                extensions: GetExtensions(error)
            );

            static string GetTitle(Error error)
                => error.Type switch
                {
                    ErrorType.Failure => error.Code,
                    ErrorType.Unexpected => "An unexpected error occurred.",
                    ErrorType.Validation => error.Code,
                    ErrorType.Conflict => error.Code,
                    ErrorType.NotFound => error.Code,
                    ErrorType.Unauthorized => error.Code,
                    ErrorType.Forbidden => error.Code,
                    var _ => "An error occurred."
                };

            static string GetErrorRfc(ErrorType errorType)
                => errorType switch
                {
                    ErrorType.Failure => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    ErrorType.Unexpected => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                    ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                    ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
                    ErrorType.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    var _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };

            static int GetStatusCode(Error error)
                => error.Type switch
                {
                    ErrorType.Failure or ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
                    ErrorType.Validation => StatusCodes.Status400BadRequest,
                    ErrorType.Conflict => StatusCodes.Status409Conflict,
                    ErrorType.NotFound => StatusCodes.Status404NotFound,
                    ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                    ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                    var _ => StatusCodes.Status500InternalServerError
                };

            static Dictionary<string, object?> GetExtensions(Error error)
            {
                var extensions = new Dictionary<string, object?>();

                if (error.Type == ErrorType.Validation)
                {
                    extensions["errors"] = new[] { error };
                }
                else
                {
                    foreach (KeyValuePair<string, object> kvp in error.Metadata ?? [])
                    {
                        extensions[kvp.Key] = kvp.Value;
                    }
                }

                return extensions;
            }
        }
    }
}
