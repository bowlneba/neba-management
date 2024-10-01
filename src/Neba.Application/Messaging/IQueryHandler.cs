using ErrorOr;
using MediatR;

namespace Neba.Application.Messaging;

/// <summary>
/// Defines a handler for a query that returns a result of type <typeparamref name="TResult"/> or an error.
/// </summary>
/// <typeparam name="TQuery">The type of the query.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    : IRequestHandler<TQuery, ErrorOr<TResult>>
    where TQuery : IQuery<TResult>;