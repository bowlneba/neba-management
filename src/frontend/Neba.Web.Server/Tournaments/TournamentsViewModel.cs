using Neba.Domain.Identifiers;

namespace Neba.Web.Server.Tournaments;

/// <summary>
/// Represents the view model for a tournament.
/// </summary>
public sealed record TournamentViewModel
{
    /// <summary>>
    /// Gets or sets the unique identifier of the tournament.
    /// </summary>
    public required TournamentId Id { get; init; }

    /// <summary>
    /// Gets or sets the name of the tournament.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the URL of the tournament thumbnail image.
    /// </summary>
    public Uri? ThumbnailUrl { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the bowling center where the tournament is held.
    /// </summary>
    public required BowlingCenterId BowlingCenterId { get; init; }

    /// <summary>
    /// Gets or sets the name of the bowling center where the tournament is held.
    /// </summary>
    public required string BowlingCenterName { get; init; }

    /// <summary>
    /// Gets or sets the start date of the tournament.
    /// </summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>
    /// Gets or sets the end date of the tournament.
    /// </summary>
    public required DateOnly EndDate { get; init; }

    /// <summary>
    /// Gets or sets the type of the tournament.
    /// </summary>
    public required string TournamentType { get; init; }

    /// <summary>
    /// Gets or sets the pattern length category of the tournament.
    /// </summary>
    public string? PatternLengthCategory { get; init; }
}
