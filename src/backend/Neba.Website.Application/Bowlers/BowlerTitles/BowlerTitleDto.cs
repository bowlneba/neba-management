using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using Neba.Domain;
using Neba.Domain.Identifiers;
using Neba.Website.Domain.Tournaments;

namespace Neba.Website.Application.Bowlers.BowlerTitles;

/// <summary>
/// Data transfer object representing a single title won by a bowler, including bowler and tournament details.
/// </summary>
public sealed record BowlerTitleDto
{
    /// <summary>
    /// Gets the unique identifier of the bowler.
    /// </summary>
    public required BowlerId BowlerId { get; init; }

    /// <summary>
    /// Gets the full name of the bowler.
    /// </summary>
    public required Name BowlerName { get; init; }

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
    [JsonConverter(typeof(SmartEnumValueConverter<TournamentType, int>))]
    public required TournamentType TournamentType { get; init; }
}
