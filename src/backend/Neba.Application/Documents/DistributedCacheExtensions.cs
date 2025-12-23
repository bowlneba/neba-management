using System.Text.Json;
using Ardalis.SmartEnum.SystemTextJson;
using Microsoft.Extensions.Caching.Distributed;

namespace Neba.Application.Documents;

#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable S1144 // Remove unused constructor of private type.

/// <summary>
/// Extension methods for <see cref="IDistributedCache"/> to support JSON serialization.
/// </summary>
internal static class DistributedCacheExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Converters = { new SmartEnumNameConverter<RefreshStatus, int>() }
    };

    extension(IDistributedCache cache)
    {
        /// <summary>
        /// Gets a value from the cache and deserializes it from JSON.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The deserialized value, or null if not found.</returns>
        public async Task<T?> GetAsync<T>(
            string key,
            CancellationToken cancellationToken = default) where T : class
        {
            byte[]? bytes = await cache.GetAsync(key, cancellationToken);

            if (bytes is null || bytes.Length == 0)
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
        }

        /// <summary>
        /// Serializes a value to JSON and stores it in the cache.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="options">Cache entry options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task SetAsync<T>(
            string key,
            T value,
            DistributedCacheEntryOptions options,
            CancellationToken cancellationToken = default) where T : class
        {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
            await cache.SetAsync(key, bytes, options, cancellationToken);
        }
    }
}
