namespace Neba.Application.Caching;

/// <summary>
/// Extension methods for cache key validation and parsing.
/// </summary>
internal static class CacheKeyExtensions
{
    #pragma warning disable S1144 // Unused private method is intentional for extension method syntax
    #pragma warning disable S2325 // Method could be static is intentional for extension method syntax

    extension(string key)
    {
        /// <summary>
        /// Validates that a cache key follows the ADR-002 naming convention.
        /// </summary>
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
        public bool IsValidCacheKey()
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length > 512)
                return false;

            string[] parts = key.Split(':');
            return parts.Length >= 3 && parts.All(p => !string.IsNullOrWhiteSpace(p));
        }

        /// <summary>
        /// Extracts the context (first part) from a cache key.
        /// </summary>
        /// <returns>The context portion of the key, or empty string if invalid.</returns>
        /// <example>
        /// <code>
        /// string key = "website:doc:bylaws:content";
        /// string context = key.GetContext(); // Returns "website"
        /// </code>
        /// </example>
        public string GetContext()
        {
            ArgumentNullException.ThrowIfNull(key);

            string[] parts = key.Split(':');
            return parts.Length > 0 ? parts[0] : string.Empty;
        }

        /// <summary>
        /// Extracts the cache type (second part) from a cache key.
        /// </summary>
        /// <returns>The type portion of the key, or empty string if invalid.</returns>
        /// <example>
        /// <code>
        /// string key = "website:doc:bylaws:content";
        /// string type = key.GetCacheType(); // Returns "doc"
        /// </code>
        /// </example>
        public string GetCacheType()
        {
            ArgumentNullException.ThrowIfNull(key);

            string[] parts = key.Split(':');
            return parts.Length > 1 ? parts[1] : string.Empty;
        }
        /// <summary>
        /// Extracts the identifier (third part) from a cache key.
        /// </summary>
        /// <returns>The identifier portion of the key, or empty string if invalid.</returns>
        /// <example>
        /// <code>
        /// string key = "website:doc:bylaws:content";
        /// string identifier = key.GetIdentifier(); // Returns "bylaws"
        /// </code>
        /// </example>
        public string GetIdentifier()
        {
            ArgumentNullException.ThrowIfNull(key);

            string[] parts = key.Split(':');
            return parts.Length > 2 ? parts[2] : string.Empty;
        }
    }
}

