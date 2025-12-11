namespace Neba.Contracts.Website.Awards;

/// <summary>
/// Represents a High Block award given to a bowler for achieving the highest total pinfall in a block of games during a season.
/// </summary>
/// <remarks>
/// The High Block award recognizes exceptional performance across multiple consecutive games within a bowling season.
/// This response includes both award identification and bowler information for public display on the website.
/// </remarks>
/// <example>
/// {
///   "id": "d2f1e8a5-3b9a-4c6b-8f2a-1a2b3c4d5e6f",
///   "bowlerId": "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
///   "bowlerName": "Jane Doe",
///   "season": "2024/2025"
/// }
/// </example>
public sealed record HighBlockAwardResponse
{
    /// <summary>
    /// Gets the unique identifier of the High Block award record.
    /// </summary>
    /// <example>d2f1e8a5-3b9a-4c6b-8f2a-1a2b3c4d5e6f</example>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the unique identifier of the bowler who received the award.
    /// </summary>
    /// <example>a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d</example>
    public required Guid BowlerId { get; init; }

    /// <summary>
    /// Gets the full display name of the bowler who received the award.
    /// </summary>
    /// <remarks>
    /// This name is suitable for public display on the website and in award listings.
    /// </remarks>
    /// <example>Jane Doe</example>
    public required string BowlerName { get; set; }

    /// <summary>
    /// Gets the bowling season for which the award was given, formatted as a year range.
    /// </summary>
    /// <remarks>
    /// The season format represents the bowling year which typically spans across two calendar years.
    /// </remarks>
    /// <example>2024/2025</example>
    public required string Season { get; set; }
}
