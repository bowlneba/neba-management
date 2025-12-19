using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using Neba.Domain;

namespace Neba.Contracts.Website.Bowlers;

/// <summary>
/// Represents a single championship title in a bowler's title history.
/// </summary>
/// <remarks>
/// This is a nested response object used within <see cref="BowlerTitlesResponse"/> to represent individual title wins.
/// It contains tournament timing and categorization information but excludes bowler identity since that's provided by the parent response.
/// Each title represents a championship win in a NEBA tournament event. For team tournaments (Doubles, Trios),
/// each team member receives their own individual title record.
/// </remarks>
/// <example>
/// {
///   "month": 3,
///   "year": 2024,
///   "tournamentType": "Singles"
/// }
/// </example>
public sealed record BowlerTitleResponse
{
    /// <summary>
    /// Gets the month in which the tournament was held (1-12).
    /// </summary>
    /// <remarks>
    /// This is serialized as a numeric value (1 for January through 12 for December) using the Month SmartEnum.
    /// </remarks>
    /// <example>3</example>
    [JsonConverter(typeof(SmartEnumValueConverter<Month, int>))]
    public required Month Month { get; init; }

    /// <summary>
    /// Gets the year in which the tournament was held.
    /// </summary>
    /// <example>2024</example>
    public required int Year { get; init; }

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
