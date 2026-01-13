using Neba.Domain.Identifiers;

namespace Neba.Website.Contracts.Tournaments;

/// <summary>
/// Response DTO representing a summary of a tournament returned by the website API.
/// </summary>
public sealed record TournamentSummaryResponse
{
    /// <summary>
    /// Gets the unique identifier of the tournament.
    /// </summary>
    public required TournamentId Id { get; init; }

    /// <summary>
    /// Gets the name of the tournament.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the URL of the tournament thumbnail image.
    /// </summary>
    public Uri? ThumbnailUrl { get; init; }

    /// <summary>
    /// Gets the identifier of the bowling center where the tournament is held.
    /// </summary>
    public BowlingCenterId? BowlingCenterId { get; init; }

    /// <summary>
    /// Gets the name of the bowling center where the tournament is held.
    /// </summary>
    public string? BowlingCenterName { get; init; }

    /// <summary>
    /// Gets the start date of the tournament.
    /// </summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>
    /// Gets the end date of the tournament.
    /// </summary>
    public required DateOnly EndDate { get; init; }

    /// <summary>
    /// Gets the type of the tournament.
    /// </summary>
    /// <example>"Singles"</example>
    public required string TournamentType { get; init; }

    /// <summary>
    /// Gets the pattern length category of the tournament, if applicable.
    /// </summary>
    /// <example>"Short"</example>
    public string? PatternLengthCategory { get; init; }
}
