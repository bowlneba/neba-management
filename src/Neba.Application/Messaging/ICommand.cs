using ErrorOr;
using MediatR;

namespace Neba.Application.Messaging;

public interface ICommand : IRequest<ErrorOr<Success>>, IBaseCommand;

public interface ICommand<TResult> : IRequest<ErrorOr<TResult>>, IBaseCommand;

public interface IBaseCommand;