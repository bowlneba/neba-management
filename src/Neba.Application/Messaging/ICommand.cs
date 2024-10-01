using ErrorOr;
using MediatR;

namespace Neba.Application.Messaging;

/// <summary>
/// Marker interface for base commands.
/// </summary>
public interface IBaseCommand;

/// <summary>
/// Represents a command that returns a success or error result.
/// </summary>
public interface ICommand
    : IRequest<ErrorOr<Success>>, IBaseCommand;

/// <summary>
/// Represents a command that returns a result of type <typeparamref name="TResult"/> or an error.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface ICommand<TResult>
    : IRequest<ErrorOr<TResult>>, IBaseCommand;