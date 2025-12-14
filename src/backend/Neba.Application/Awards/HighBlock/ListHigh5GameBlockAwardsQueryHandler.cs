using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Awards.HighBlock;

internal sealed class ListHigh5GameBlockAwardsQueryHandler(IWebsiteAwardQueryRepository repository)
        : IQueryHandler<ListHigh5GameBlockAwardsQuery, IReadOnlyCollection<HighBlockAwardDto>>
{
    private readonly IWebsiteAwardQueryRepository _repository = repository;

    public async Task<IReadOnlyCollection<HighBlockAwardDto>> HandleAsync(
        ListHigh5GameBlockAwardsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<HighBlockAwardDto> awards = await _repository.ListHigh5GameBlockAwardsAsync(cancellationToken);

        return awards;
    }
}
