namespace Neba.Contracts;

/// <inheritdoc />
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
public static class CollectionResponse
{
    /// <summary>
    /// Creates a new instance of <see cref="CollectionResponse{T}"/> with the specified items.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="items">The collection of items to include in the response.</param>
    /// <returns>A new instance of <see cref="CollectionResponse{T}"/> containing the specified items.</returns>
    public static CollectionResponse<T> Create<T>(IReadOnlyCollection<T> items)
        => new()
        { Items = items };
}
