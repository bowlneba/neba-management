using Neba.Application.Caching;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Awards.BowlerOfTheYear;

/// <summary>
/// Represents a query to list all Bowler of the Year awards.
/// </summary>
public sealed record ListBowlerOfTheYearAwardsQuery
    : ICachedQuery<IReadOnlyCollection<BowlerOfTheYearAwardDto>>
{
    ///<inheritdoc />
    public string Key
        => CacheKeys.Awards.BowlerOfTheYear();

    ///<inheritdoc />
    public IReadOnlyCollection<string> Tags
        => CacheTags.Award(CacheTags.AwardTypes.BowlerOfTheYear);
}
