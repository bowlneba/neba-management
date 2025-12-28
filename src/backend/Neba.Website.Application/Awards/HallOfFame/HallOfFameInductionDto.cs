using Neba.Domain;

namespace Neba.Website.Application.Awards.HallOfFame;

/// <summary>
/// Data transfer object representing a Hall of Fame induction used by the application layer.
/// </summary>
public sealed record HallOfFameInductionDto
{
    /// <summary>
    /// The year the induction occurred.
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// The full name of the inducted bowler.
    /// </summary>
    public required Name BowlerName { get; init; }

    /// <summary>
    /// The photo associated with this induction.
    /// </summary>
    public StoredFile? Photo { get; init; }

    /// <summary>
    /// A public URI pointing to the bowler's photo.
    /// </summary>
    public Uri? PhotoUri { get; internal set; }

    /// <summary>
    /// The categories associated with this induction.
    /// </summary>
    public required IReadOnlyCollection<HallOfFameCategory> Categories { get; init; }
}
