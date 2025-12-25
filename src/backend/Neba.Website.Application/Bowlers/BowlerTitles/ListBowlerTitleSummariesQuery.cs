using Neba.Application.Caching;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Bowlers.BowlerTitles;

/// <summary>
/// Query to retrieve a summary of titles for all bowlers.
/// </summary>
public sealed record ListBowlerTitleSummariesQuery
    : ICachedQuery<IReadOnlyCollection<BowlerTitleSummaryDto>>
{
    ///<inheritdoc />
    public string Key
        => $"{CacheKeys.WebsiteContext}:{CacheKeys.Types.Query}:ListBowlerTitleSummariesQuery";

    ///<inheritdoc />
    public IReadOnlyCollection<string> Tags
        => CacheTags.AllBowlers();
}
