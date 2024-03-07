using ErrorOr;
using MediatR;

namespace Neba.Application.Messaging;

public interface IQuery<TResult> : IRequest<ErrorOr<TResult>>;