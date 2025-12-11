using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Awards;

internal sealed class ListHighBlockAwardsQueryHandler(IWebsiteAwardQueryRepository repository)
        : IQueryHandler<ListHighBlockAwardsQuery, IReadOnlyCollection<HighBlockAwardDto>>
{
    private readonly IWebsiteAwardQueryRepository _repository = repository;

    public async Task<IReadOnlyCollection<HighBlockAwardDto>> HandleAsync(
        ListHighBlockAwardsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<HighBlockAwardDto> awards = await _repository.ListHigh5GameBlockAwardsAsync(cancellationToken);

        return awards;
    }
}
