using Neba.Application.Messaging;

namespace Neba.Website.Application.Awards.HighAverage;

#pragma warning disable CA1812 // Internal class is instantiated through dependency injection

internal sealed class ListHighAverageAwardsQueryHandler(IWebsiteAwardQueryRepository repository)
        : IQueryHandler<ListHighAverageAwardsQuery, IReadOnlyCollection<HighAverageAwardDto>>
{

    public async Task<IReadOnlyCollection<HighAverageAwardDto>> HandleAsync(ListHighAverageAwardsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<HighAverageAwardDto> awards = await repository.ListHighAverageAwardsAsync(cancellationToken);

        return awards;
    }
}
