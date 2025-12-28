namespace Neba.Web.Server.HallOfFame;

/// <summary>
/// View model used by the Hall of Fame UI to display a single induction entry.
/// </summary>
public sealed record HallOfFameInductionViewModel
{
    /// <summary>
    /// The full display name of the bowler inducted into the Hall of Fame.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// The calendar year the bowler was inducted.
    /// </summary>
    public required int InductionYear { get; init; }

    /// <summary>
    /// Human-friendly category names describing the induction reason(s).
    /// Multiple categories may be present; these are the display strings
    /// (for example, "Superior Performance", "Meritorious Service").
    /// </summary>
    public required IReadOnlyCollection<string> Categories { get; init; }

    /// <summary>
    /// URL of the bowler's photo, if available.
    /// </summary>
    /// <value></value>
    public Uri? PhotoUrl { get; init; }
}
