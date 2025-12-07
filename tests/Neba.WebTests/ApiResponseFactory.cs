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
    /// <returns>A TestApiResponse wrapper that ensures proper disposal.</returns>
    public static TestApiResponse<T> CreateSuccessResponse<T>(T content)
    {
        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        var apiResponse = new Refit.ApiResponse<T>(httpResponse, content, new RefitSettings());
        return new TestApiResponse<T>(apiResponse, httpResponse);
    }

    /// <summary>
    /// Creates an API response with a custom HTTP status code.
    /// </summary>
    /// <typeparam name="T">The type of the response content.</typeparam>
    /// <param name="content">The content to include in the response.</param>
    /// <param name="statusCode">The HTTP status code for the response.</param>
    /// <returns>A TestApiResponse wrapper that ensures proper disposal.</returns>
    public static TestApiResponse<T> CreateResponse<T>(T content, System.Net.HttpStatusCode statusCode)
    {
        var httpResponse = new HttpResponseMessage(statusCode);
        var apiResponse = new Refit.ApiResponse<T>(httpResponse, content, new RefitSettings());
        return new TestApiResponse<T>(apiResponse, httpResponse);
    }
}

/// <summary>
/// Wrapper class that ensures proper disposal of both ApiResponse and HttpResponseMessage.
/// </summary>
public sealed class TestApiResponse<T> : IDisposable
{
    private readonly Refit.ApiResponse<T> _apiResponse;
    private readonly HttpResponseMessage _httpResponse;
    private bool _disposed;

    internal TestApiResponse(Refit.ApiResponse<T> apiResponse, HttpResponseMessage httpResponse)
    {
        _apiResponse = apiResponse;
        _httpResponse = httpResponse;
    }

    /// <summary>
    /// Gets the underlying ApiResponse.
    /// </summary>
    public Refit.ApiResponse<T> ApiResponse => _apiResponse;

    /// <summary>
    /// Converts the TestApiResponse to an ApiResponse.
    /// </summary>
    public Refit.ApiResponse<T> ToApiResponse() => _apiResponse;

    /// <summary>
    /// Implicit conversion to ApiResponse for seamless usage in tests.
    /// </summary>
    public static implicit operator Refit.ApiResponse<T>(TestApiResponse<T> testResponse) => testResponse._apiResponse;

    /// <summary>
    /// Disposes of both the ApiResponse and HttpResponseMessage.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _apiResponse?.Dispose();
            _httpResponse?.Dispose();
            _disposed = true;
        }
    }
}
