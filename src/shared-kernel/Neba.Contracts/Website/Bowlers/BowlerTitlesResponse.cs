using Neba.Domain.Identifiers;

namespace Neba.Contracts.Website.Bowlers;

/// <summary>
/// Represents a complete record of all titles won by a specific bowler.
/// </summary>
/// <remarks>
/// This response aggregates a bowler's identity information with their full title history.
/// Typically used when displaying a bowler's profile or detailed achievement record.
/// </remarks>
/// <example>
/// {
///   "bowlerId": "01JEFQH5Y9KM7N3P5Q6R8S9T0W",
///   "bowlerName": "John Doe",
///   "titles": [
///     {
///       "month": 3,
///       "year": 2024,
///       "tournamentType": "Singles"
///     },
///     {
///       "month": 10,
///       "year": 2023,
///       "tournamentType": "Doubles"
///     }
///   ]
/// }
/// </example>
public sealed record BowlerTitlesResponse
{
    /// <summary>
    /// Gets the unique identifier of the bowler.
    /// </summary>
    /// <example>"01JEFQH5Y9KM7N3P5Q6R8S9T0W"</example>
    public required BowlerId BowlerId { get; init; }

    /// <summary>
    /// Gets the full display name of the bowler.
    /// </summary>
    /// <example>"John Doe"</example>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the collection of all titles won by the bowler, ordered chronologically.
    /// </summary>
    /// <remarks>
    /// Each title includes the month, year, and tournament type information.
    /// An empty collection indicates the bowler has not won any titles.
    /// </remarks>
    public required IReadOnlyCollection<BowlerTitleResponse> Titles { get; init; }
}
