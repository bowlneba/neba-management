
using Neba.Application.Caching;
using Neba.Application.Documents;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Documents.Bylaws;

/// <summary>
/// Query to retrieve the bylaws as a DocumentDto.
/// </summary>
public sealed record GetBylawsQuery
    : ICachedQuery<DocumentDto>
{
    ///<inheritdoc />
    public string Key
        => CacheKeys.Documents.Content("bylaws");

    ///<inheritdoc />
    public TimeSpan Expiry
        => TimeSpan.FromDays(30);

    ///<inheritdoc />
    public IReadOnlyCollection<string> Tags
        => CacheTags.Documents("bylaws");
}
