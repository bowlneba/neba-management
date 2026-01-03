using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using Neba.Domain.Tournaments;

namespace Neba.Website.Application.Tournaments;

/// <summary>
/// Data transfer object representing the details of a single title won by a bowler.
/// </summary>
public sealed record TitleDto
{
    /// <summary>
    /// Gets the month when the tournament ended.
    /// </summary>
    public required DateOnly TournamentDate { get; init; }

    /// <summary>
    /// Gets the type of tournament in which the title was won.
    /// </summary>
    [JsonConverter(typeof(SmartEnumValueConverter<TournamentType, int>))]
    public required TournamentType TournamentType { get; init; }
}
