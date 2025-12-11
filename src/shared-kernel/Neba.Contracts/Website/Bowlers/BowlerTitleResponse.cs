using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;

namespace Neba.Contracts.Website.Bowlers;

/// <summary>
/// Represents a single title achievement in a bowler's title history.
/// </summary>
/// <remarks>
/// This is a nested response object used within <see cref="BowlerTitlesResponse"/> to represent individual title wins.
/// It contains tournament timing and categorization information but excludes bowler identity since that's provided by the parent response.
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
    /// Gets the month in which the title was won (1-12).
    /// </summary>
    /// <remarks>
    /// This is serialized as a numeric value (1 for January through 12 for December) using the Month SmartEnum.
    /// </remarks>
    /// <example>3</example>
    [JsonConverter(typeof(SmartEnumValueConverter<Month, int>))]
    public required Month Month { get; init; }

    /// <summary>
    /// Gets the year in which the title was won.
    /// </summary>
    /// <example>2024</example>
    public required int Year { get; init; }

    /// <summary>
    /// Gets the type or category of tournament for which the title was awarded.
    /// </summary>
    /// <remarks>
    /// Common tournament types include Singles, Doubles, Team, All Events, etc.
    /// </remarks>
    /// <example>Singles</example>
    public required string TournamentType { get; init; }
}
