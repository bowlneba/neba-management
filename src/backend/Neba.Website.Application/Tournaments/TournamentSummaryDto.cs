using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;

namespace Neba.Website.Application.Tournaments;

/// <summary>
/// Represents a data transfer object for tournament summaries.
/// </summary>
public sealed record TournamentSummaryDto
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
    public required BowlingCenterId BowlingCenterId { get; init; }

    /// <summary>
    /// Gets the name of the bowling center where the tournament is held.
    /// </summary>
    public required string BowlingCenterName { get; init; }

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
    public required TournamentType TournamentType { get; init; }

    /// <summary>
    /// Gets the pattern length category of the tournament, if applicable.
    /// </summary>
    public PatternLengthCategory? PatternLengthCategory { get; init; }
}
