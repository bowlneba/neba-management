using Neba.Application.Caching;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Awards.HallOfFame;

/// <summary>
/// Query to retrieve a list of Hall of Fame inductions.
/// </summary>
public sealed record GetHallOfFameInductionsQuery
    : ICachedQuery<IReadOnlyCollection<HallOfFameInductionDto>>
{
    /// <inheritdoc/>
    public string Key
        => CacheKeys.Awards.HallOfFameInductions();

    /// <inheritdoc/>
    public TimeSpan Expiry
        => TimeSpan.FromDays(65);

    /// <inheritdoc/>
    public IReadOnlyCollection<string> Tags
        => CacheTags.Award(CacheTags.AwardTypes.HallOfFame);
}
