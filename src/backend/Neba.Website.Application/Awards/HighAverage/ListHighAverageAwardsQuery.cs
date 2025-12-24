using Neba.Application.Caching;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Awards.HighAverage;

/// <summary>
/// Query requesting the list of all High Average awards.
/// </summary>
/// <remarks>
/// The query returns a read-only collection of <see cref="HighAverageAwardDto"/>.
/// </remarks>
public sealed record ListHighAverageAwardsQuery
    : ICachedQuery<IReadOnlyCollection<HighAverageAwardDto>>
{
    ///<inheritdoc />
    public string Key
        => CacheKeys.Awards.HighAverage();

    ///<inheritdoc />
    public IReadOnlyCollection<string> Tags
        => CacheTags.Award(CacheTags.AwardTypes.HighAverage);
}
