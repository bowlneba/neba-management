namespace Neba.Application.Abstractions.Messaging;

/// <summary>
/// Represents a query that returns a response of type <typeparamref name="TResponse"/>.
/// Used in the CQRS pattern to encapsulate a request for data.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the query.</typeparam>
public interface IQuery<TResponse>;
