using ErrorOr;
using MediatR;

namespace Neba.Application.Messaging;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, ErrorOr<Success>>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResult> : IRequestHandler<TCommand, ErrorOr<TResult>>
    where TCommand : ICommand<TResult>;