using Neba.Application.Messaging;

namespace Neba.Website.Application.Bowlers.BowlerTitles;

#pragma warning disable CA1812 // Internal class is instantiated through dependency injection

internal sealed class ListBowlerTitlesQueryHandler(IWebsiteTitleQueryRepository websiteTitleQueryRepository)
        : IQueryHandler<ListBowlerTitlesQuery, IReadOnlyCollection<BowlerTitleDto>>
{

    public async Task<IReadOnlyCollection<BowlerTitleDto>> HandleAsync(ListBowlerTitlesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerTitleDto> titles = await websiteTitleQueryRepository.ListTitlesAsync(cancellationToken);

        return titles;
    }
}
