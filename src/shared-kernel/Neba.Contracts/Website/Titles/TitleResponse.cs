using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;

namespace Neba.Contracts.Website.Titles;

/// <summary>
/// Represents a championship title won by a bowler in a NEBA tournament event.
/// </summary>
/// <remarks>
/// A title represents a championship win and serves as a permanent record of competitive achievement.
/// For team tournaments (Doubles, Trios), each team member receives their own individual title record.
/// Earning at least one title grants eligibility for the Tournament of Champions.
/// This response provides complete information for displaying title records in listings and bowler histories.
/// </remarks>
/// <example>
/// {
///   "bowlerId": "123e4567-e89b-12d3-a456-426614174000",
///   "bowlerName": "John Doe",
///   "tournamentMonth": 3,
///   "tournamentYear": 2024,
///   "tournamentType": "Singles"
/// }
/// </example>
public sealed record TitleResponse
{
    /// <summary>
    /// Gets the unique identifier of the bowler who won the title.
    /// </summary>
    /// <example>"123e4567-e89b-12d3-a456-426614174000"</example>
    public required Ulid BowlerId { get; init; }

    /// <summary>
    /// Gets the full display name of the bowler who won the title.
    /// </summary>
    /// <example>"John Doe"</example>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the month in which the tournament was held (1-12).
    /// </summary>
    /// <remarks>
    /// This is serialized as a numeric value (1 for January through 12 for December) using the Month SmartEnum.
    /// </remarks>
    /// <example>3</example>
    [JsonConverter(typeof(SmartEnumValueConverter<Month, int>))]
    public required Month TournamentMonth { get; init; }

    /// <summary>
    /// Gets the year in which the tournament was held.
    /// </summary>
    /// <example>2024</example>
    public required int TournamentYear { get; init; }

    /// <summary>
    /// Gets the tournament type or event category in which the title was won.
    /// </summary>
    /// <remarks>
    /// Tournament types include individual events (Singles, All Events), team events (Doubles, Trios),
    /// and special major tournaments (Tournament of Champions, Invitational, Masters).
    /// </remarks>
    /// <example>"Singles"</example>
    public required string TournamentType { get; init; }
}
