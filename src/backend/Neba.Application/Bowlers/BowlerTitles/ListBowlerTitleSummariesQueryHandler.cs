using ErrorOr;
using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Bowlers.BowlerTitles;

internal sealed class ListBowlerTitleSummariesQueryHandler(IWebsiteBowlerQueryRepository websiteBowlerQueryRepository)
        : IQueryHandler<ListBowlerTitleSummariesQuery, IReadOnlyCollection<BowlerTitleSummaryDto>>
{
    private readonly IWebsiteBowlerQueryRepository _websiteBowlerQueryRepository = websiteBowlerQueryRepository;

    public async Task<ErrorOr<IReadOnlyCollection<BowlerTitleSummaryDto>>> HandleAsync(ListBowlerTitleSummariesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerTitleSummaryDto> titlesSummary = await _websiteBowlerQueryRepository.ListBowlerTitleSummariesAsync(cancellationToken);

        return titlesSummary.ToErrorOr();
    }
}
