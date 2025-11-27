namespace Neba.Contracts;

/// <summary>
/// Represents a response containing a collection of items of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public interface ICollectionResponse<T>
{
    /// <summary>
    /// Gets the collection of items included in the response.
    /// </summary>
    IReadOnlyCollection<T> Items { get; init; }
}
