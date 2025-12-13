using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;

namespace Neba.Contracts.Website.Titles;

/// <summary>
/// Represents a single title won by a bowler, including detailed tournament and bowler information.
/// </summary>
/// <remarks>
/// This response provides complete information about a specific title achievement, including the bowler's identity,
/// the tournament details, and the type of competition. Used when displaying individual title records or detailed title histories.
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
    public required Guid BowlerId { get; init; }

    /// <summary>
    /// Gets the full display name of the bowler who won the title.
    /// </summary>
    /// <example>"John Doe"</example>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the month in which the title was won (1-12).
    /// </summary>
    /// <remarks>
    /// This is serialized as a numeric value (1 for January through 12 for December) using the Month SmartEnum.
    /// </remarks>
    /// <example>3</example>
    [JsonConverter(typeof(SmartEnumValueConverter<Month, int>))]
    public required Month TournamentMonth { get; init; }

    /// <summary>
    /// Gets the year in which the title was won.
    /// </summary>
    /// <example>2024</example>
    public required int TournamentYear { get; init; }

    /// <summary>
    /// Gets the type or category of tournament in which the title was won.
    /// </summary>
    /// <remarks>
    /// Common tournament types include Singles, Doubles, Team, All Events, etc.
    /// </remarks>
    /// <example>"Singles"</example>
    public required string TournamentType { get; init; }
}
