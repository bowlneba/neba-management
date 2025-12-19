using Neba.Application.Messaging;

namespace Neba.Application.Bowlers.BowlerTitles;

internal sealed class ListBowlerTitlesQueryHandler(IWebsiteTitleQueryRepository websiteTitleQueryRepository)
        : IQueryHandler<ListBowlerTitlesQuery, IReadOnlyCollection<BowlerTitleDto>>
{
    private readonly IWebsiteTitleQueryRepository _websiteTitleQueryRepository = websiteTitleQueryRepository;

    public async Task<IReadOnlyCollection<BowlerTitleDto>> HandleAsync(ListBowlerTitlesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerTitleDto> titles = await _websiteTitleQueryRepository.ListTitlesAsync(cancellationToken);

        return titles;
    }
}
