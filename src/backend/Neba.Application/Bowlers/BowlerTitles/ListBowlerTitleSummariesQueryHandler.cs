using ErrorOr;
using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Bowlers.BowlerTitles;

internal sealed class ListBowlerTitleSummariesQueryHandler(IWebsiteTitleQueryRepository websiteTitleQueryRepository)
        : IQueryHandler<ListBowlerTitleSummariesQuery, IReadOnlyCollection<BowlerTitleSummaryDto>>
{
    private readonly IWebsiteTitleQueryRepository _websiteTitleQueryRepository = websiteTitleQueryRepository;

    public async Task<ErrorOr<IReadOnlyCollection<BowlerTitleSummaryDto>>> HandleAsync(ListBowlerTitleSummariesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerTitleSummaryDto> titlesSummary = await _websiteTitleQueryRepository.ListTitleSummariesAsync(cancellationToken);
        return titlesSummary.ToErrorOr();
    }
}
