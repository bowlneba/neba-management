using Neba.Application.Messaging;

namespace Neba.Application.Awards.BowlerOfTheYear;

internal sealed class ListBowlerOfTheYearAwardsQueryHandler(
    IWebsiteAwardQueryRepository websiteAwardQueryRepository)
        : IQueryHandler<ListBowlerOfTheYearAwardsQuery, IReadOnlyCollection<BowlerOfTheYearAwardDto>>
{
    private readonly IWebsiteAwardQueryRepository _websiteAwardQueryRepository = websiteAwardQueryRepository;

    public async Task<IReadOnlyCollection<BowlerOfTheYearAwardDto>> HandleAsync(ListBowlerOfTheYearAwardsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerOfTheYearAwardDto> awards = await _websiteAwardQueryRepository.ListBowlerOfTheYearAwardsAsync(cancellationToken);

        return awards;
    }
}
