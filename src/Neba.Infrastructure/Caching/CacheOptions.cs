using Microsoft.Extensions.Caching.Distributed;

namespace Neba.Infrastructure.Caching;

internal static class CacheOptions
{
    private static DistributedCacheEntryOptions DefaultExpiration
        => new()
        {
#if DEBUG
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
#else
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12)
#endif
        };

    public static DistributedCacheEntryOptions Create(TimeSpan? expiration)
    {
        return expiration is not null
            ? new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration.Value
            }
            : DefaultExpiration;
    }
}