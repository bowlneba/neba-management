using System.Net.Http;
using Refit;

namespace Neba.WebTests;

/// <summary>
/// Factory class for creating API response objects for testing.
/// </summary>
public static class ApiResponseFactory
{
    /// <summary>
    /// Creates a successful API response with the specified content.
    /// </summary>
    /// <typeparam name="T">The type of the response content.</typeparam>
    /// <param name="content">The content to include in the response.</param>
    /// <returns>An ApiResponse with HTTP 200 OK status containing the specified content.</returns>
    public static Refit.ApiResponse<T> CreateSuccessResponse<T>(T content)
    {
        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        return new Refit.ApiResponse<T>(httpResponse, content, new RefitSettings());
    }

    /// <summary>
    /// Creates an API response with a custom HTTP status code.
    /// </summary>
    /// <typeparam name="T">The type of the response content.</typeparam>
    /// <param name="content">The content to include in the response.</param>
    /// <param name="statusCode">The HTTP status code for the response.</param>
    /// <returns>An ApiResponse with the specified status code containing the specified content.</returns>
    public static Refit.ApiResponse<T> CreateResponse<T>(T content, System.Net.HttpStatusCode statusCode)
    {
        var httpResponse = new HttpResponseMessage(statusCode);
        return new Refit.ApiResponse<T>(httpResponse, content, new RefitSettings());
    }
}