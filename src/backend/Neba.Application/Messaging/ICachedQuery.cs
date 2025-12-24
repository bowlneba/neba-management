namespace Neba.Application.Messaging;

/// <summary>
/// Represents a query that can be cached, providing a unique cache key and expiry duration.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the query.</typeparam>
public interface ICachedQuery<TResponse>
    : IQuery<TResponse>
{
    /// <summary>
    /// Gets the unique cache key for this query instance.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Gets the cache expiry duration for this query. Defaults to 7 days.
    /// </summary>
    TimeSpan Expiry
        => TimeSpan.FromDays(7);

    /// <summary>
    /// Gets the collection of tags associated with this query instance.
    /// Tags can be used to categorize or identify the query for various purposes
    /// such as filtering or diagnostics.
    /// </summary>
    IReadOnlyCollection<string> Tags
        => Array.Empty<string>();
}
