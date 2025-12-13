namespace Neba.Contracts;

/// <summary>
/// Represents a collection response containing multiple items of the same type.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <remarks>
/// This is the standard implementation of <see cref="ICollectionResponse{T}"/> used across all API endpoints that return collections.
/// The total items count equals the number of items in the collection.
/// </remarks>
/// <example>
/// {
///   "items": [
///     { "id": "123e4567-e89b-12d3-a456-426614174000", "name": "John Doe" },
///     { "id": "223e4567-e89b-12d3-a456-426614174001", "name": "Jane Smith" }
///   ],
///   "totalItems": 2
/// }
/// </example>
public sealed record CollectionResponse<T>
    : ICollectionResponse<T>
{
    /// <inheritdoc />
    public required IReadOnlyCollection<T> Items { get; init; }

    /// <inheritdoc />
    public int TotalItems
        => Items.Count;
}

/// <summary>
/// Provides factory methods for creating instances of <see cref="CollectionResponse{T}"/>.
/// </summary>
/// <remarks>
/// Use this static class to create collection response objects in a fluent and consistent manner.
/// </remarks>
public static class CollectionResponse
{
    /// <summary>
    /// Creates a new instance of <see cref="CollectionResponse{T}"/> with the specified items.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="items">The collection of items to include in the response.</param>
    /// <returns>A new instance of <see cref="CollectionResponse{T}"/> containing the specified items.</returns>
    /// <example>
    /// var response = CollectionResponse.Create(new[] { item1, item2, item3 });
    /// </example>
    public static CollectionResponse<T> Create<T>(IReadOnlyCollection<T> items)
        => new()
        { Items = items };
}
