using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Bowlers.BowlerAwards;

/// <summary>
/// Represents a query to list all Bowler of the Year awards.
/// </summary>
public sealed record ListBowlerOfTheYearAwardsQuery
    : IQuery<IReadOnlyCollection<BowlerOfTheYearDto>>;
