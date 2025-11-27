using ErrorOr;
using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Bowlers.BowlerTitleCounts;

internal sealed class GetBowlerTitleCountsQueryHandler(
    IWebsiteBowlerQueryRepository bowlerQueryRepository)
    : IQueryHandler<GetBowlerTitleCountsQuery, IReadOnlyCollection<BowlerTitleCountDto>>
{
    private readonly IWebsiteBowlerQueryRepository _bowlerQueryRepository = bowlerQueryRepository;

    public async Task<ErrorOr<IReadOnlyCollection<BowlerTitleCountDto>>> HandleAsync(GetBowlerTitleCountsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerTitleCountDto> bowlerTitleCounts = await _bowlerQueryRepository.GetBowlerTitleCountsAsync(cancellationToken);

        return bowlerTitleCounts.ToErrorOr();
    }
}
