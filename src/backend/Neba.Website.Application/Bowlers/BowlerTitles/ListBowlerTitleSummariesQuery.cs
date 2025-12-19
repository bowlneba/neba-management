using Neba.Application.Messaging;

namespace Neba.Website.Application.Bowlers.BowlerTitles;

/// <summary>
/// Query to retrieve a summary of titles for all bowlers.
/// </summary>
public sealed record ListBowlerTitleSummariesQuery
    : IQuery<IReadOnlyCollection<BowlerTitleSummaryDto>>;
