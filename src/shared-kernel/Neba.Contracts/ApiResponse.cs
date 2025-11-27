namespace Neba.Contracts;

/// <summary>
/// Generic API response wrapper.
/// </summary>
/// <value></value>
public record ApiResponse<T>
{
    /// <summary>
    /// The data contained in the API response.
    /// </summary>
    /// <value></value>
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
/// Non-generic API response wrapper.
/// </summary>
/// <value></value>
public record ApiResponse
{
    /// <summary>
    /// Creates a new ApiResponse with the specified data.
    /// </summary>
    /// <param name="data"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ApiResponse<T> Create<T>(T data)
    {
        return ApiResponse<T>.Create(data);
    }
}
