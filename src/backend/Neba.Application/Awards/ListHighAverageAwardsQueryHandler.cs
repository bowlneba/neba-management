using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Awards;

internal sealed class ListHighAverageAwardsQueryHandler(IWebsiteAwardQueryRepository repository)
        : IQueryHandler<ListHighAverageAwardsQuery, IReadOnlyCollection<HighAverageAwardDto>>
{
    private readonly IWebsiteAwardQueryRepository _repository = repository;

    public async Task<IReadOnlyCollection<HighAverageAwardDto>> HandleAsync(ListHighAverageAwardsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<HighAverageAwardDto> awards = await _repository.ListHighAverageAwardsAsync(cancellationToken);

        return awards;
    }
}
