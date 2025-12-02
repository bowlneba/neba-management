using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Bowlers.BowlerTitles;

/// <summary>
/// Query to retrieve a summary of titles for all bowlers.
/// </summary>
public sealed record ListBowlerTitleSummariesQuery
    : IQuery<IReadOnlyCollection<BowlerTitleSummaryDto>>;
