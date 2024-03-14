using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Neba.Infrastructure.Caching;

namespace Neba.Infrastructure.UnitTests.Caching;

public class CacheServiceTests
{
    private readonly CacheService _cacheService;
    private readonly IDistributedCache _mockCache;

    public CacheServiceTests()
    {
        _mockCache = Substitute.For<IDistributedCache>();

        _cacheService = new CacheService(_mockCache);
    }

    [Fact]
    public async Task GetAsync_ReturnsDeserializedValue_WhenKeyExists()
    {
        // Arrange
        var key = "testKey";
        var expectedValue = "test";
        var serializedValue = JsonSerializer.SerializeToUtf8Bytes(expectedValue);
        _mockCache.GetAsync(key).Returns(serializedValue);

        // Act
        var result = await _cacheService.GetAsync<string>(key, default);

        // Assert
        result.Should().BeEquivalentTo(expectedValue);
    }

    [Fact]
    public async Task SetAsync_UsesDefaultCacheOptions_WhenExpirationIsNull()
    {
        // Arrange
        var key = "testKey";
        var value = new { Prop1 = "value1" };
        var serializedValue = JsonSerializer.SerializeToUtf8Bytes(value);
        var defaultOptions = CacheOptions.Create(null);

        // Act
        await _cacheService.SetAsync(key, value, null, default);

        // Assert
        await _mockCache.Received().SetAsync(key, Arg.Is<byte[]>(x => x.SequenceEqual(serializedValue)),
            Arg.Is<DistributedCacheEntryOptions>(x =>
                x.AbsoluteExpirationRelativeToNow == defaultOptions.AbsoluteExpirationRelativeToNow));
    }

    [Fact]
    public async Task SetAsync_UsesProvidedExpiration()
    {
        // Arrange
        var key = "testKey";
        var value = new { Prop1 = "value1" };
        var serializedValue = JsonSerializer.SerializeToUtf8Bytes(value);
        var expiration = TimeSpan.FromMinutes(5);

        // Act
        await _cacheService.SetAsync(key, value, expiration, default);

        // Assert
        await _mockCache.Received().SetAsync(key, Arg.Is<byte[]>(x => x.SequenceEqual(serializedValue)),
            Arg.Is<DistributedCacheEntryOptions>(x => x.AbsoluteExpirationRelativeToNow == expiration));
    }

    [Fact]
    public async Task RemoveAsync_RemovesValue()
    {
        // Arrange
        var key = "testKey";

        // Act
        await _cacheService.RemoveAsync(key, default);

        // Assert
        await _mockCache.Received().RemoveAsync(key);
    }
}