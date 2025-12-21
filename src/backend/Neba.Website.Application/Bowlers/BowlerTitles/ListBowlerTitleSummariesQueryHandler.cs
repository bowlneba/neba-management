using Neba.Application.Messaging;

namespace Neba.Website.Application.Bowlers.BowlerTitles;

#pragma warning disable CA1812 // Internal class is instantiated through dependency injection

internal sealed class ListBowlerTitleSummariesQueryHandler(IWebsiteTitleQueryRepository websiteTitleQueryRepository)
        : IQueryHandler<ListBowlerTitleSummariesQuery, IReadOnlyCollection<BowlerTitleSummaryDto>>
{

    public async Task<IReadOnlyCollection<BowlerTitleSummaryDto>> HandleAsync(ListBowlerTitleSummariesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerTitleSummaryDto> titlesSummary = await websiteTitleQueryRepository.ListTitleSummariesAsync(cancellationToken);

        return titlesSummary;
    }
}
