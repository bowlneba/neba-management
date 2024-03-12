namespace Neba.Application.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken);

    Task SetAsync<T>(string key, T value, TimeSpan? expiration, CancellationToken cancellationToken);

    Task RemoveAsync(string key, CancellationToken cancellationToken);
}