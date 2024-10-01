using ErrorOr;
using MediatR;

namespace Neba.Application.Messaging;

/// <summary>
/// Defines a handler for a command that returns a success or error result.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
public interface ICommandHandler<in TCommand>
    : IRequestHandler<TCommand, ErrorOr<Success>>
    where TCommand : ICommand;

/// <summary>
/// Defines a handler for a command that returns a result of type <typeparamref name="TResult"/> or an error.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface ICommandHandler<in TCommand, TResult>
    : IRequestHandler<TCommand, ErrorOr<TResult>>
    where TCommand : ICommand<TResult>;