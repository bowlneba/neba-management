using ErrorOr;
using MediatR;

namespace Neba.Application.Messaging;

/// <summary>
/// Represents a query that returns a result of type <typeparamref name="TResult"/> or an error.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IQuery<TResult>
    : IRequest<ErrorOr<TResult>>;