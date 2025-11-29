using ErrorOr;
using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Bowlers.BowlerTitles;

internal sealed class GetBowlersTitlesQueryHandler(IWebsiteBowlerQueryRepository websiteBowlerQueryRepository)
        : IQueryHandler<GetBowlersTitlesQuery, IReadOnlyCollection<BowlerTitleDto>>
{
    private readonly IWebsiteBowlerQueryRepository _websiteBowlerQueryRepository = websiteBowlerQueryRepository;

    public async Task<ErrorOr<IReadOnlyCollection<BowlerTitleDto>>> HandleAsync(GetBowlersTitlesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerTitleDto> titles = await _websiteBowlerQueryRepository.GetBowlerTitlesAsync(cancellationToken);

        return titles.ToErrorOr();
    }
}
