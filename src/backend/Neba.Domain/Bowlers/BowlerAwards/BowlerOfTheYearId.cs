using StronglyTypedIds;

namespace Neba.Domain.Bowlers.BowlerAwards;

/// <summary>
/// Represents the unique identifier for a Bowler of the Year.
/// </summary>
[StronglyTypedId(Template.Guid, "guid-efcore")]
public readonly partial struct BowlerOfTheYearId;
