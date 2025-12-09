using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Awards;

internal sealed class ListBowlerOfTheYearAwardsQueryHandler(
    IWebsiteAwardQueryRepository websiteAwardQueryRepository)
        : IQueryHandler<ListBowlerOfTheYearAwardsQuery, IReadOnlyCollection<BowlerOfTheYearDto>>
{
    private readonly IWebsiteAwardQueryRepository _websiteAwardQueryRepository = websiteAwardQueryRepository;

    public async Task<IReadOnlyCollection<BowlerOfTheYearDto>> HandleAsync(ListBowlerOfTheYearAwardsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerOfTheYearDto> awards = await _websiteAwardQueryRepository.ListBowlerOfTheYearAwardsAsync(cancellationToken);

        return awards;
    }
}
