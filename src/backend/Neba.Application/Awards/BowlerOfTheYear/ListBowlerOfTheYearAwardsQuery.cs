using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Awards.BowlerOfTheYear;

/// <summary>
/// Represents a query to list all Bowler of the Year awards.
/// </summary>
public sealed record ListBowlerOfTheYearAwardsQuery
    : IQuery<IReadOnlyCollection<BowlerOfTheYearAwardDto>>;
