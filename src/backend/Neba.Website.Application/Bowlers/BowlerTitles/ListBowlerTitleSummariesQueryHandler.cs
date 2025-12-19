using Neba.Application.Messaging;

namespace Neba.Website.Application.Bowlers.BowlerTitles;

internal sealed class ListBowlerTitleSummariesQueryHandler(IWebsiteTitleQueryRepository websiteTitleQueryRepository)
        : IQueryHandler<ListBowlerTitleSummariesQuery, IReadOnlyCollection<BowlerTitleSummaryDto>>
{

    public async Task<IReadOnlyCollection<BowlerTitleSummaryDto>> HandleAsync(ListBowlerTitleSummariesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerTitleSummaryDto> titlesSummary = await websiteTitleQueryRepository.ListTitleSummariesAsync(cancellationToken);

        return titlesSummary;
    }
}
