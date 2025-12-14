using ErrorOr;

namespace Neba.Api;

internal static class ResultExtensions
{
    extension<T>(ErrorOr<T> result)
    {
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
                    ErrorType.Failure => StatusCodes.Status500InternalServerError,
                    ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
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
