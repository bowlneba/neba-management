using Ardalis.SmartEnum.SystemTextJson;
using System.Text.Json.Serialization;

namespace Neba.Contracts.History.Champions;

/// <summary>
/// Response containing a bowler's identity, name, and the collection of titles they have won.
/// </summary>
public sealed record GetBowlerTitlesResponse
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
    /// Gets the collection of titles won by the bowler.
    /// </summary>
    public required IReadOnlyCollection<TitlesResponse> Titles { get; init; }
}

/// <summary>
/// Represents a title won by a bowler, including the month, year, and type of tournament.
/// </summary>
public sealed record TitlesResponse
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
