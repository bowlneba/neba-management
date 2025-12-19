using Neba.Application.Messaging;

namespace Neba.Website.Application.Awards.HighBlock;

#pragma warning disable CA1812 // Internal class is instantiated through dependency injection

internal sealed class ListHigh5GameBlockAwardsQueryHandler(IWebsiteAwardQueryRepository repository)
        : IQueryHandler<ListHigh5GameBlockAwardsQuery, IReadOnlyCollection<HighBlockAwardDto>>
{

    public async Task<IReadOnlyCollection<HighBlockAwardDto>> HandleAsync(
        ListHigh5GameBlockAwardsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<HighBlockAwardDto> awards = await repository.ListHigh5GameBlockAwardsAsync(cancellationToken);

        return awards;
    }
}
