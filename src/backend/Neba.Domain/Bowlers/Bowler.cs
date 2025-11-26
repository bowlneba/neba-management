using Neba.Domain.Abstractions;

namespace Neba.Domain.Bowlers;

/// <summary>
/// A bowler is a person who participates in NEBA Tournaments.
/// </summary>
public sealed class Bowler
    : Aggregate<BowlerId>
{
    /// <summary>
    /// Gets or sets the bowler's full name, including first name, last name, and optional middle initial, suffix, and nickname.
    /// </summary>
    public required Name Name { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bowler"/> class with a new unique identifier.
    /// </summary>
    internal Bowler()
        : base(BowlerId.New())
    {}
}
