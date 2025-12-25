
using Neba.Application.Caching;
using Neba.Application.Documents;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Tournaments.TournamentRules;

/// <summary>
/// Query to retrieve the rules for a tournament as a DocumentDto.
/// </summary>
public sealed record GetTournamentRulesQuery
    : ICachedQuery<DocumentDto>
{
    ///<inheritdoc />
    public string Key
        => CacheKeys.Documents.Content("tournament-rules");

    ///<inheritdoc />
    public TimeSpan Expiry
        => TimeSpan.FromDays(30);

    ///<inheritdoc />
    public IReadOnlyCollection<string> Tags
        => CacheTags.Documents("tournament-rules");
}
