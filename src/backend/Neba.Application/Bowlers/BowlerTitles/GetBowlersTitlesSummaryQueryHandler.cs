using ErrorOr;
using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Bowlers.BowlerTitles;

internal sealed class GetBowlersTitlesSummaryQueryHandler(IWebsiteBowlerQueryRepository websiteBowlerQueryRepository)
        : IQueryHandler<GetBowlersTitlesSummaryQuery, IReadOnlyCollection<BowlerTitlesSummaryDto>>
{
    private readonly IWebsiteBowlerQueryRepository _websiteBowlerQueryRepository = websiteBowlerQueryRepository;

    public async Task<ErrorOr<IReadOnlyCollection<BowlerTitlesSummaryDto>>> HandleAsync(GetBowlersTitlesSummaryQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerTitlesSummaryDto> titlesSummary = await _websiteBowlerQueryRepository.GetAllBowlerTitlesSummaryAsync(cancellationToken);

        return titlesSummary.ToErrorOr();
    }
}
