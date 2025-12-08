using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;

namespace Neba.Contracts.Website.Bowlers;

/// <summary>
/// Represents a title won by a bowler, including the month, year, and type of tournament.
/// </summary>
public sealed record BowlerTitleResponse
{
    /// <summary>
    /// Gets the month in which the title was won.
    /// </summary>
    [JsonConverter(typeof(SmartEnumValueConverter<Month, int>))]
    public required Month Month { get; init; }

    /// <summary>
    /// Gets the year in which the title was won.
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Gets the type of tournament for which the title was awarded.
    /// </summary>
    public required string TournamentType { get; init; }
}
