using Neba.Application.Messaging;

namespace Neba.Website.Application.Bowlers.BowlerTitles;

internal sealed class ListBowlerTitlesQueryHandler(IWebsiteTitleQueryRepository websiteTitleQueryRepository)
        : IQueryHandler<ListBowlerTitlesQuery, IReadOnlyCollection<BowlerTitleDto>>
{

    public async Task<IReadOnlyCollection<BowlerTitleDto>> HandleAsync(ListBowlerTitlesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerTitleDto> titles = await websiteTitleQueryRepository.ListTitlesAsync(cancellationToken);

        return titles;
    }
}
