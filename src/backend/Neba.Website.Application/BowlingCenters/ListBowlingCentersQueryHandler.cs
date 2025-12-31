using Neba.Application.Messaging;

namespace Neba.Website.Application.BowlingCenters;

internal sealed class ListBowlingCentersQueryHandler(
    IWebsiteBowlingCenterQueryRepository repository)
        : IQueryHandler<ListBowlingCentersQuery, IReadOnlyCollection<BowlingCenterDto>>
{
    private readonly IWebsiteBowlingCenterQueryRepository _repository = repository;

    public async Task<IReadOnlyCollection<BowlingCenterDto>> HandleAsync(
        ListBowlingCentersQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlingCenterDto> bowlingCenters = await _repository.ListBowlingCentersAsync(cancellationToken);

        return bowlingCenters;
    }
}
