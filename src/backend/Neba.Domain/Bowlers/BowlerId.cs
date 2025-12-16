using StronglyTypedIds;

namespace Neba.Domain.Bowlers;

/// <summary>
/// Unique identifier for a bowler.
/// </summary>
[StronglyTypedId("ulid-full")]
public readonly partial struct BowlerId;
