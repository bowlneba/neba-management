namespace Neba.Contracts;

/// <summary>
/// Generic API response wrapper that encapsulates data returned by API endpoints.
/// </summary>
/// <typeparam name="T">The type of data contained in the response.</typeparam>
/// <remarks>
/// All successful API responses are wrapped in this structure to provide a consistent response format across all endpoints.
/// </remarks>
/// <example>
/// {
///   "data": {
///     "id": "123e4567-e89b-12d3-a456-426614174000",
///     "name": "John Doe"
///   }
/// }
/// </example>
public record ApiResponse<T>
{
    /// <summary>
    /// Gets the data payload contained in the API response.
    /// </summary>
    /// <remarks>
    /// The data type varies depending on the endpoint and operation being performed.
    /// </remarks>
    public required T Data { get; init; }

    internal static ApiResponse<T> Create(T data)
    {
        return new ApiResponse<T>
        {
            Data = data
        };
    }
}

/// <summary>
/// Non-generic API response factory for creating strongly-typed response wrappers.
/// </summary>
/// <remarks>
/// This static class provides factory methods to create <see cref="ApiResponse{T}"/> instances.
/// </remarks>
public record ApiResponse
{
    /// <summary>
    /// Creates a new ApiResponse wrapper containing the specified data.
    /// </summary>
    /// <typeparam name="T">The type of data to wrap in the response.</typeparam>
    /// <param name="data">The data payload to include in the response.</param>
    /// <returns>A new <see cref="ApiResponse{T}"/> instance containing the specified data.</returns>
    public static ApiResponse<T> Create<T>(T data)
    {
        return ApiResponse<T>.Create(data);
    }
}
