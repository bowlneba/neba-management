using ErrorOr;

namespace Neba.Application.Messaging;

/// <summary>
/// Non-generic handler interface for commands that return a <see cref="Success"/>.
/// This is a convenience alias for <see cref="ICommandHandler{TCommand, TResponse}"/>
/// with <see cref="Success"/> as the response type.
/// </summary>
/// <typeparam name="TCommand">The command type handled by this handler. Must implement <see cref="ICommand"/>.</typeparam>
public interface ICommandHandler<in TCommand>
    : ICommandHandler<TCommand, Success>
    where TCommand : ICommand;

/// <summary>
/// Represents a handler capable of processing a command of type <typeparamref name="TCommand"/>
/// and producing a response of type <typeparamref name="TResponse"/>.
/// Handlers should encapsulate application logic for a single command and return
/// an <see cref="ErrorOr{TResponse}"/> indicating success or one or more errors.
/// </summary>
/// <typeparam name="TCommand">The command type to handle. Must implement <see cref="ICommand{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The response type returned on successful handling of the command.</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Handles the specified <paramref name="command"/> asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task"/> that yields an <see cref="ErrorOr{TResponse}"/> containing
    /// the response value on success or one or more errors on failure.
    /// </returns>
    Task<ErrorOr<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
