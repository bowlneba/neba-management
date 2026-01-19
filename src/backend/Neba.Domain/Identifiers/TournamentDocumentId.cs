using StronglyTypedIds;

namespace Neba.Domain.Identifiers;

/// <summary>
/// Unique identifier for a tournament.
/// </summary>
[StronglyTypedId("ulid-full")]
public readonly partial struct TournamentDocumentId;
