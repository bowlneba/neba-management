using System.Net;
using ErrorOr;

namespace Neba.Web.Server.Services;

/// <summary>
/// Provides API-related errors for the frontend application.
/// </summary>
internal static class ApiErrors
{
    /// <summary>
    /// Returns an error for a failed API request.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="reasonPhrase">The reason phrase or error message.</param>
    /// <returns>An <see cref="Error"/> indicating the API request failed.</returns>
    public static Error RequestFailed(HttpStatusCode statusCode, string? reasonPhrase = null)
        => Error.Failure(
            code: "Api.RequestFailed",
            description: reasonPhrase ?? $"API request failed with status code {(int)statusCode}.",
            metadata: new Dictionary<string, object>
            {
                { "statusCode", statusCode },
                { "statusCodeValue", (int)statusCode }
            }
        );

    /// <summary>
    /// Returns an error for network-related failures.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>An <see cref="Error"/> indicating a network failure.</returns>
    public static Error NetworkError(string message)
        => Error.Failure(
            code: "Api.NetworkError",
            description: $"Network error: {message}"
        );

    /// <summary>
    /// Returns an error for request timeout or cancellation.
    /// </summary>
    /// <returns>An <see cref="Error"/> indicating the request timed out.</returns>
    public static Error Timeout()
        => Error.Failure(
            code: "Api.Timeout",
            description: "Request timeout"
        );

    /// <summary>
    /// Returns an error for unexpected exceptions.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>An <see cref="Error"/> indicating an unexpected error occurred.</returns>
    public static Error Unexpected(string message)
        => Error.Unexpected(
            code: "Api.Unexpected",
            description: $"An unexpected error occurred: {message}"
        );
}
