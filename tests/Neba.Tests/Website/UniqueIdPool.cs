namespace Neba.Tests.Website;

/// <summary>
/// Provides a pool of unique, pre-shuffled IDs for test data generation.
/// This ensures test data has unique IDs without collisions across multiple test runs.
/// Supports probabilistic null values for optional IDs.
/// </summary>
public sealed class UniqueIdPool
{
    private readonly List<int> _ids;
    private readonly Random _random;
    private readonly float _probabilityOfValue;
    private int _currentIndex;

    private UniqueIdPool(List<int> ids, Random random, float probabilityOfValue)
    {
        _ids = ids;
        _random = random;
        _probabilityOfValue = probabilityOfValue;
        _currentIndex = 0;
    }

    /// <summary>
    /// Creates a pool of unique IDs with timestamp-based uniqueness and random shuffling.
    /// </summary>
    /// <param name="poolSize">The number of unique IDs to generate in the pool.</param>
    /// <param name="baseOffset">Base offset to add to IDs for different ID types (e.g., 0 for websiteIds, 100_000_000 for applicationIds).</param>
    /// <param name="seed">Optional seed for random number generation. Use the same seed across related pools for reproducible tests.</param>
    /// <param name="probabilityOfValue">Probability (0.0 to 1.0) that GetNext() will return a value instead of null. Default is 0.5.</param>
    /// <returns>A new UniqueIdPool containing shuffled unique IDs.</returns>
#pragma warning disable CA5394 // Random is acceptable here - used only for test data generation, not security
    public static UniqueIdPool Create(int poolSize, int baseOffset, int? seed = null, float probabilityOfValue = 0.5f)
    {
        Random random = seed.HasValue ? new Random(seed.Value) : new Random();

        // Use timestamp to ensure uniqueness across test runs
        long baseTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Generate unique IDs using timestamp, offset, and ranges to avoid collisions
        List<int> ids = [.. Enumerable.Range(1, poolSize)
            .Select(i => (int)(baseTimestamp % 1_000_000) + baseOffset + (i * 1000) + random.Next(1000))
            .OrderBy(_ => random.Next())];

        return new UniqueIdPool(ids, random, probabilityOfValue);
    }
#pragma warning restore CA5394

    /// <summary>
    /// Gets the next unique ID from the pool, or null based on the configured probability.
    /// </summary>
    /// <returns>The next unique ID, or null based on probability.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the pool is exhausted and a non-null value is needed.</exception>
#pragma warning disable CA1024, CA5394 // GetNext() appropriately named (mutates state); Random acceptable for test data
    public int? GetNext()
    {
        // Determine if we should return null based on probability
        if (_random.NextDouble() >= _probabilityOfValue)
        {
            return null;
        }

        if (_currentIndex >= _ids.Count)
        {
            throw new InvalidOperationException(
                $"UniqueIdPool exhausted. Attempted to get ID at index {_currentIndex} but pool only contains {_ids.Count} IDs.");
        }

        return _ids[_currentIndex++];
    }

    /// <summary>
    /// Gets the number of remaining IDs in the pool.
    /// </summary>
    public int RemainingCount => _ids.Count - _currentIndex;
}
