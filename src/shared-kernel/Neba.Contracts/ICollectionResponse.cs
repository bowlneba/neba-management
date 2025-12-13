namespace Neba.Contracts;

/// <summary>
/// Represents a paginated or filtered response containing a collection of items.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <remarks>
/// This interface is implemented by all collection-based API responses to provide a consistent structure for returning multiple items.
/// </remarks>
/// <example>
/// {
///   "items": [
///     { "id": 1, "name": "Item 1" },
///     { "id": 2, "name": "Item 2" }
///   ],
///   "totalItems": 2
/// }
/// </example>
public interface ICollectionResponse<T>
{
    /// <summary>
    /// Gets the collection of items included in the response.
    /// </summary>
    /// <remarks>
    /// This property contains the actual data items returned by the API endpoint, which may be the complete set or a filtered/paginated subset.
    /// </remarks>
    IReadOnlyCollection<T> Items { get; init; }

    /// <summary>
    /// Gets the total number of items in the collection.
    /// </summary>
    /// <remarks>
    /// For simple collections, this equals the count of items returned. For paginated responses, this represents the total count across all pages.
    /// </remarks>
    int TotalItems { get; }
}
