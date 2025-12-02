using StronglyTypedIds;

namespace Neba.Domain.Tournaments;

/// <summary>
/// Represents a unique identifier for a Title entity.
/// </summary>
[StronglyTypedId(Template.Guid, "guid-efcore")]
public readonly partial struct TitleId;
