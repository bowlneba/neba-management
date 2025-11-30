using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;

namespace Neba.Contracts.Website.Bowlers;

/// <summary>
/// Response contract representing a single title won by a bowler, including bowler and tournament details.
/// </summary>
public sealed record BowlerTitleResponse
{
    /// <summary>
    /// Gets the unique identifier of the bowler.
    /// </summary>
    public required Guid BowlerId { get; init; }

    /// <summary>
    /// Gets the full name of the bowler.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the month in which the title was won.
    /// </summary>
    [JsonConverter(typeof(SmartEnumValueConverter<Month, int>))]
    public required Month TournamentMonth { get; init; }

    /// <summary>
    /// Gets the year in which the title was won.
    /// </summary>
    public required int TournamentYear { get; init; }

    /// <summary>
    /// Gets the type of tournament in which the title was won.
    /// </summary>
    public required string TournamentType { get; init; }
}
