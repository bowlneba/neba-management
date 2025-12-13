using StronglyTypedIds;

namespace Neba.Domain.Awards;

/// <summary>
/// Represents a unique identifier for a season award in NEBA.
/// </summary>
[StronglyTypedId(Template.Guid, "guid-efcore")]
public readonly partial struct SeasonAwardId;
