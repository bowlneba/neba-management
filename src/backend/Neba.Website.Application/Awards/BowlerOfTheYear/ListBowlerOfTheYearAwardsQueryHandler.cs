using Neba.Application.Messaging;

namespace Neba.Website.Application.Awards.BowlerOfTheYear;

#pragma warning disable CA1812 // Internal class is instantiated through dependency injection

internal sealed class ListBowlerOfTheYearAwardsQueryHandler(
    IWebsiteAwardQueryRepository websiteAwardQueryRepository)
        : IQueryHandler<ListBowlerOfTheYearAwardsQuery, IReadOnlyCollection<BowlerOfTheYearAwardDto>>
{

    public async Task<IReadOnlyCollection<BowlerOfTheYearAwardDto>> HandleAsync(ListBowlerOfTheYearAwardsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerOfTheYearAwardDto> awards = await websiteAwardQueryRepository.ListBowlerOfTheYearAwardsAsync(cancellationToken);

        return awards;
    }
}
