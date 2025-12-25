using ErrorOr;
using Neba.Application.Caching;
using Neba.Application.Messaging;
using Neba.Domain.Identifiers;

namespace Neba.Website.Application.Bowlers.BowlerTitles;

/// <summary>
/// Query to retrieve the detailed titles for a specific bowler.
/// </summary>
public sealed record BowlerTitlesQuery
    : ICachedQuery<ErrorOr<BowlerTitlesDto>>
{
    /// <summary>
    /// Gets the unique identifier of the bowler whose titles are being requested.
    /// </summary>
    public required BowlerId BowlerId { get; init; }

    ///<inheritdoc />
    public string Key
        => $"{CacheKeys.WebsiteContext}:{CacheKeys.Types.Query}:BowlerTitlesQuery:{BowlerId.Value}";

    ///<inheritdoc />
    public IReadOnlyCollection<string> Tags
        => CacheTags.Bowler(BowlerId);
}
