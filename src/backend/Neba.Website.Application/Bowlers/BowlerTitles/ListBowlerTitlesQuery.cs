using Neba.Application.Caching;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Bowlers.BowlerTitles;

/// <summary>
/// Query for retrieving a collection of all bowler titles, including bowler and tournament details for each title.
/// </summary>
public sealed record ListBowlerTitlesQuery
    : ICachedQuery<IReadOnlyCollection<BowlerTitleDto>>
{
    ///<inheritdoc />
    public string Key
        => $"{CacheKeys.WebsiteContext}:{CacheKeys.Types.Query}:ListBowlerTitlesQuery";

    ///<inheritdoc />
    public IReadOnlyCollection<string> Tags
        => CacheTags.AllBowlers();
}
