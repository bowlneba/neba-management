using Neba.Application.Caching;
using Neba.Application.Messaging;

namespace Neba.Website.Application.BowlingCenters;

/// <summary>
/// Query for retrieving a collection of all bowling centers for website display.
/// </summary>
public sealed record ListBowlingCentersQuery
    : ICachedQuery<IReadOnlyCollection<BowlingCenterDto>>
{
    ///<inheritdoc />
    public string Key
        => $"{CacheKeys.WebsiteContext}:{CacheKeys.Types.Query}:ListBowlingCentersQuery";

    ///<inheritdoc />
    public IReadOnlyCollection<string> Tags
        => CacheTags.AllBowlingCenters();
}
