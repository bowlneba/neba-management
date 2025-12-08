using ErrorOr;

namespace Neba.Application.Abstractions.Messaging;

/// <summary>
/// Defines a handler for processing a query and returning a response.
/// </summary>
/// <typeparam name="TQuery">The type of the query to handle.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Handles the specified query asynchronously and returns a result.
    /// </summary>
    /// <param name="query">The query to process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response.</returns>
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
