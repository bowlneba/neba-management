using Neba.Application.Caching;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Awards.HighBlock;

/// <summary>
/// Query to retrieve a read-only collection of <see cref="HighBlockAwardDto"/> objects.
/// </summary>
public sealed record ListHigh5GameBlockAwardsQuery
    : ICachedQuery<IReadOnlyCollection<HighBlockAwardDto>>
{
    ///<inheritdoc />
    public string Key
        => CacheKeys.Awards.HighBlock();

    ///<inheritdoc />
    public IReadOnlyCollection<string> Tags
        => CacheTags.Award(CacheTags.AwardTypes.HighBlock);
}
