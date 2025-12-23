using ErrorOr;

namespace Neba.Application.Messaging;

#pragma warning disable S2326 // Unused type parameters should be removed

/// <summary>
/// Marker interface representing a command that returns a response of type
/// <typeparamref name="TResponse"/>. Implementations represent application
/// commands (e.g. in a CQRS-style design) and carry the data required to
/// perform an operation that yields a result of <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The response type produced by the command.</typeparam>
public interface ICommand<out TResponse>;

/// <summary>
/// Non-generic marker interface for commands that return a <see cref="Success"/> result.
/// This is a convenience alias for <see cref="ICommand{TResponse}"/> with
/// <see cref="Success"/> as the response type.
/// </summary>
public interface ICommand
    : ICommand<Success>;
