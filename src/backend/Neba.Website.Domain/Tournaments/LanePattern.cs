using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using Neba.Domain.Tournaments;

namespace Neba.Website.Domain.Tournaments;

/// <summary>
/// Represents a lane pattern used in a tournament.
/// </summary>
public sealed record LanePattern
{
    /// <summary>
    /// Gets the length of the lane pattern.
    /// </summary>
    [JsonConverter(typeof(SmartEnumValueConverter<PatternLengthCategory, int>))]
    public required PatternLengthCategory LengthCategory { get; init; }

    /// <summary>
    /// Gets the ratio of the lane pattern.
    /// </summary>
    [JsonConverter(typeof(SmartEnumValueConverter<PatternRatioCategory, int>))]
    public required PatternRatioCategory RatioCategory { get; init; }
}
