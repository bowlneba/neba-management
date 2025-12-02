using StronglyTypedIds;

namespace Neba.Domain.Bowlers;

/// <summary>
/// Unique identifier for a bowler.
/// </summary>
[StronglyTypedId(Template.Guid, "guid-efcore")]
public readonly partial struct BowlerId;
