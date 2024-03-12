using Neba.Application.Messaging;

namespace Neba.Application.Caching;

public interface ICacheQuery
{
    string Key { get; }

    TimeSpan? Expiration { get; }
}

public interface ICacheQuery<TResult> : IQuery<TResult>, ICacheQuery;