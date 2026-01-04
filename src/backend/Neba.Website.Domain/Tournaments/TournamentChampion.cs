using Neba.Domain.Identifiers;

namespace Neba.Website.Domain.Tournaments;

/// <summary>
/// Internal class used solely for EF Core mapping of champion bowler IDs.
/// This class exists only to satisfy EF Core's requirement that OwnsMany collections
/// must be of an entity type, not a primitive/value type.
/// Represents the many-to-many relationship between tournaments and bowlers.
/// </summary>
internal sealed class TournamentChampion
{
    public required BowlerId BowlerId { get; init; }

    // Set by EF Core based on the owner relationship - needed for navigation from Bowler side
    public TournamentId TournamentId { get; init; } = TournamentId.Empty;
}
