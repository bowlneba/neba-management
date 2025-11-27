using Neba.Domain.Abstractions;
using Neba.Domain.Tournaments;

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
    /// Gets or sets the identifier for the bowler in the legacy website database.
    /// </summary>
    /// <value></value>
    public int? WebsiteId { get; init;}

    /// <summary>
    /// Gets or sets the identifier for the bowler in the legacy application database.
    /// </summary>
    /// <value></value>
    public int? ApplicationId { get; init; }

    internal Bowler()
        : base(BowlerId.New())
    {
        Titles = [];
    }

    internal IReadOnlyCollection<Title> Titles { get; init; }
}
