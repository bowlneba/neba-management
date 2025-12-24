namespace Neba.Application.Caching;

/// <summary>
/// Extension methods for cache key validation and parsing.
/// </summary>
public static class CacheKeyExtensions
{
    /// <summary>
    /// Validates that a cache key follows the ADR-002 naming convention.
    /// </summary>
    /// <param name="key">The cache key to validate.</param>
    /// <returns>True if the key is valid; otherwise, false.</returns>
    /// <remarks>
    /// A valid cache key must:
    /// <list type="bullet">
    /// <item>Not be null or whitespace</item>
    /// <item>Be 512 characters or less</item>
    /// <item>Have at least 3 colon-delimited parts (context:type:identifier)</item>
    /// <item>Have no empty parts</item>
    /// </list>
    /// </remarks>
    public static bool IsValidCacheKey(this string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 512)
            return false;

        string[] parts = key.Split(':');
        return parts.Length >= 3 && parts.All(p => !string.IsNullOrWhiteSpace(p));
    }

    /// <summary>
    /// Extracts the context (first part) from a cache key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The context portion of the key, or empty string if invalid.</returns>
    /// <example>
    /// <code>
    /// string key = "website:doc:bylaws:content";
    /// string context = key.GetContext(); // Returns "website"
    /// </code>
    /// </example>
    public static string GetContext(this string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        string[] parts = key.Split(':');
        return parts.Length > 0 ? parts[0] : string.Empty;
    }

    /// <summary>
    /// Extracts the cache type (second part) from a cache key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The type portion of the key, or empty string if invalid.</returns>
    /// <example>
    /// <code>
    /// string key = "website:doc:bylaws:content";
    /// string type = key.GetCacheType(); // Returns "doc"
    /// </code>
    /// </example>
    public static string GetCacheType(this string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        string[] parts = key.Split(':');
        return parts.Length > 1 ? parts[1] : string.Empty;
    }

    /// <summary>
    /// Extracts the identifier (third part) from a cache key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The identifier portion of the key, or empty string if invalid.</returns>
    /// <example>
    /// <code>
    /// string key = "website:doc:bylaws:content";
    /// string identifier = key.GetIdentifier(); // Returns "bylaws"
    /// </code>
    /// </example>
    public static string GetIdentifier(this string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        string[] parts = key.Split(':');
        return parts.Length > 2 ? parts[2] : string.Empty;
    }
}

