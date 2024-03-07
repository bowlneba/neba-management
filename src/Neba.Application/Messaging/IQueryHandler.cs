using ErrorOr;
using MediatR;

namespace Neba.Application.Messaging;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, ErrorOr<TResult>>
    where TQuery : IQuery<TResult>;