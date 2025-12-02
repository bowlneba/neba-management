using ErrorOr;
using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Bowlers.BowlerTitles;

internal sealed class ListBowlerTitlesQueryHandler(IWebsiteBowlerQueryRepository websiteBowlerQueryRepository)
        : IQueryHandler<ListBowlerTitlesQuery, IReadOnlyCollection<BowlerTitleDto>>
{
    private readonly IWebsiteBowlerQueryRepository _websiteBowlerQueryRepository = websiteBowlerQueryRepository;

    public async Task<ErrorOr<IReadOnlyCollection<BowlerTitleDto>>> HandleAsync(ListBowlerTitlesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerTitleDto> titles = await _websiteBowlerQueryRepository.ListBowlerTitlesAsync(cancellationToken);

        return titles.ToErrorOr();
    }
}
